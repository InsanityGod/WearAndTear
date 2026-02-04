using InsanityLib.Extensions;
using InsanityLib.Util.FastComparisons;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code.Enums;
using WearAndTear.Config.Server;

namespace WearAndTear.Code.AutoRegistry;

public class ContentAnalyzer
{
    public static Dictionary<string, ContentAnalyzer> Lookup { get; set; } = new();

    public bool EncounteredDeadlock { get; private set; } = false;
    public CollectibleObject Collectible { get; set; }
    public ICoreAPI Api { get; set; }

    public static string GetLookupKey(CollectibleObject collectible)
    {
        var code = collectible.Code;
        
        if(collectible is not Block) return code.ToString(); //Only blocks have orientation
        
        var orientationIndex = collectible.GetOrientationVariantIndex();
        if(orientationIndex == -1) return code.ToString();
        
        ReadOnlySpan<char> path = code.Path;
        var index1 = path.NthIndexOf('-', orientationIndex) + 1;
        var index2 = path.NthIndexOf('-', orientationIndex + 1);
        if(index2 == -1) index2 = path.Length;

        return $"{code.Domain}:{path[..index1]}{orientationIndex}{path[index2..]}";
    }

    public static ContentAnalyzer GetOrCreate(ICoreAPI api, CollectibleObject collectible)
    {
        if(collectible is Block block) collectible = block.GetPlacedByItem(api);
        
        var key = GetLookupKey(collectible);
        if (Lookup.TryGetValue(key, out var result)) return result;
        return Lookup[key] = new ContentAnalyzer(api, collectible);
    }

    private ContentAnalyzer(ICoreAPI api, CollectibleObject collectible)
    {
        Api = api;
        Collectible = collectible;
    }

    public Dictionary<string, float> MetalContent { get; set; } = new();
    public Dictionary<string, float> WoodContent { get; set; } = new();
    public Dictionary<string, float> RockContent { get; set; } = new();

    private void AnalyzeTextures()
    {
        if (Collectible is not Block block) return;
        if (block.Textures == null) return;

        if (block is BlockPulverizer && block.Code.Domain == "vanvar") return; //Thank you other mod creator for putting a bunch of unused metal textures in your block type definition...

        var metalKeys = block.Textures.Keys.Where(key => AutoPartRegistryConfig.Instance.MetalConfig.ContainsKey(key.ToLower())).ToList();
        if (metalKeys.Count != 0)
        {
            var metal = metalKeys.Count == 1 ? metalKeys[0] : "default";

            MetalContent[metal] = MetalContent.TryGetValue(metal, out var current) ? current : 0;
            return;
        }

        var pathMetals = block.Textures.Values.Select(path => AutoPartRegistryConfig.Instance.MetalConfig.Keys.FirstOrDefault(metal => path.ToString().Contains(metal)))
            .Where(value => value != null)
            .Distinct()
            .ToList();

        if (pathMetals.Count != 0)
        {
            var metal = pathMetals.Count == 1 ? pathMetals[0] : "default";
            MetalContent[metal] = MetalContent.TryGetValue(metal, out var current) ? current : 0;
        }
    }

    private void ExtractGridRecipe(GridRecipe recipe)
    {
        var ingredients = new Dictionary<CollectibleObject, float>();
        var outputAmount = recipe.Output.Quantity;
        foreach(var ingredient in recipe.resolvedIngredients)
        {
            if(ingredient is null || ingredient.IsTool || (ingredient.ResolvedItemstack is null && (!ingredient.IsWildCard || ingredient.AllowedVariants?.Length != 1))) continue;
            
            var collectible = ingredient.ResolvedItemstack is null ? Api.World.GetCollectibleObject(ingredient.Code.FillWildCard(ingredient.AllowedVariants[0])) : ingredient.ResolvedItemstack.Collectible;
            if(collectible is null) continue;
            
            var amount = (float)ingredient.Quantity / (float)outputAmount;
            if(ingredients.TryGetValue(collectible, out float currentValue))
            {
                ingredients[collectible] = currentValue + amount;
            }
            else ingredients[collectible] = amount;
        }

        ExtractIngredients(ingredients);
    }
    
    private bool TryAddContent(Dictionary<string, float> content, string key, float amount)
    {
        if(string.IsNullOrEmpty(key)) return false;

        if(content.TryGetValue(key, out float currentValue))
        {
            content[key] = currentValue + amount;
        }
        else content[key] = amount;

        return true;
    }

    private void ExtractIngredients(Dictionary<CollectibleObject, float> ingredients)
    {
        bool metal = false;
        foreach((var ingredient, var amount) in ingredients)
        {
            var firstCodePart = ingredient.FirstCodePartAsSpan();

            //Scan wood

            if (firstCodePart.SequenceEqual("log"))
            {
                if(TryAddContent(WoodContent, ingredient.Variant["wood"], amount * 12)) continue;
            }
            else if (firstCodePart.SequenceEqual("planks"))
            {
                if(TryAddContent(WoodContent, ingredient.Variant["wood"], amount * 4)) continue;
            }
            else if (firstCodePart.SequenceEqual("plank"))
            {
                if(TryAddContent(WoodContent, ingredient.Variant["wood"], amount)) continue;
            }
            //Scan rock
            else if (firstCodePart.SequenceEqual("rock")) 
            {
                if(TryAddContent(RockContent, ingredient.Variant["rock"], amount)) continue;
            }
            else if (firstCodePart.SequenceEqual("stone"))
            {
                if(TryAddContent(RockContent, ingredient.Variant["rock"], amount * 4)) continue;
            }
            

            if (AutoPartRegistryConfig.Instance.RequireAllRecipesToContainMetal && foundRecipeWithNoMetal) continue;

            //Scan metal
            if (ingredient is ItemIngot ingot && TryAddContent(MetalContent, ingot.GetMetalType(), amount * 4))
            {
                metal = true;
                continue;
            }

            var smeltStack = ingredient.CombustibleProps?.SmeltedStack?.ResolvedItemstack;
            if (smeltStack != null && smeltStack.Collectible is ItemIngot ingot2 && TryAddContent(MetalContent, ingot2.GetMetalType(), (smeltStack.StackSize / (float)ingredient.CombustibleProps.SmeltedRatio) * 4 * amount))
            {
                metal = true;
                continue;
            }

            var analyzer = GetOrCreate(Api, ingredient);
            if (analyzer.State == EAnalyzeState.Analyzing)
            {
                EncounteredDeadlock = true;
                continue; //Skipping recursive recipe
            }

            analyzer.Analyze(Api);

            foreach ((var analyzedContent, var AnalyzedAmount) in analyzer.WoodContent) TryAddContent(WoodContent, analyzedContent, AnalyzedAmount * amount);
            foreach ((var analyzedContent, var AnalyzedAmount) in analyzer.RockContent) TryAddContent(RockContent, analyzedContent, AnalyzedAmount * amount);
            foreach ((var analyzedContent, var AnalyzedAmount) in analyzer.MetalContent) TryAddContent(MetalContent, analyzedContent, AnalyzedAmount * amount);
            if(!metal && analyzer.MetalContent.Any(static item => item.Value > 0)) metal = true;
        }

        if (!metal) foundRecipeWithNoMetal = true;
    }

    private static void DivideContent(Dictionary<string, float> content, int count)
    {
        foreach (var key in content.Keys)
        {
            content[key] /= count;
        }
    }

    private bool foundRecipeWithNoMetal;
    private bool AnalyzeRecipes(ICoreAPI api)
    {
        var comparator = new WithoutOrientationComparator(Collectible);
        var itemClass = Collectible.ItemClass;
        int recipeCount = 0;
        foreach(var recipe in api.World.GridRecipes)
        {
            if(recipe.Output?.ResolvedItemstack is null || recipe.Output.Type != itemClass || !comparator.IsMatch(recipe.Output.ResolvedItemstack.Collectible)) continue;
            recipeCount++;
            ExtractGridRecipe(recipe);
        }
        if(recipeCount == 0) return false;
        if(recipeCount == 1) return true;
        
        DivideContent(WoodContent, recipeCount);
        DivideContent(RockContent, recipeCount);
        DivideContent(MetalContent, recipeCount);
        return true;
    }

    public EAnalyzeState State { get; private set; } = EAnalyzeState.NotStarted;

    public void Analyze(ICoreAPI api)
    {
        if (State == EAnalyzeState.Analyzed) return; //Already analyzed
        if (State == EAnalyzeState.Analyzing) throw new InvalidOperationException("Attempt at recursive analysis");
        State = EAnalyzeState.Analyzing;
        if (!AnalyzeRecipes(api))
        {
            //Just in case recipe analysis fails, we can still analyze textures
            AnalyzeTextures();
        }

        State = EAnalyzeState.Analyzed;
    }

    public (string Metal, float ContentLevel)? FindReinforcementMetal()
    {
        if (!MetalContent.Values.Any(static metalContent => metalContent > AutoPartRegistryConfig.Instance.MinimalMetalContentLevel)) return null; //Doesn't contain any significant metal

        var totalMetalCount = MetalContent.Sum(metal => metal.Value);
        var metalWithHighestCompositionRate = MetalContent.OrderByDescending(metal => metal.Value).First();

        if (metalWithHighestCompositionRate.Value / totalMetalCount > AutoPartRegistryConfig.Instance.MinimalMetalCompositionPercentage) return (metalWithHighestCompositionRate.Key, metalWithHighestCompositionRate.Value);

        return ("unknown", 0);
    }

    public (string Wood, float ContentLevel)? FindFrameWood()
    {
        if (WoodContent.Count == 0) return null; //Doesn't contain wood

        var totalWoodCount = WoodContent.Sum(wood => wood.Value);
        var woodWithHighestCompositionRate = WoodContent.OrderByDescending(wood => wood.Value).First();

        if (woodWithHighestCompositionRate.Value / totalWoodCount > AutoPartRegistryConfig.Instance.MinimalWoodCompositionPercentage) return (woodWithHighestCompositionRate.Key, woodWithHighestCompositionRate.Value);

        return null; //no specific wood type
    }

    public (string Rock, float ContentLevel)? FindFrameRock()
    {
        if (RockContent.Count == 0) return null; //Doesn't contain wood

        var totalRockCount = RockContent.Sum(wood => wood.Value);
        var rockWithHighestCompositionRate = RockContent.OrderByDescending(rock => rock.Value).First();

        if (rockWithHighestCompositionRate.Value / totalRockCount > AutoPartRegistryConfig.Instance.MinimalRockCompositionPercentage) return (rockWithHighestCompositionRate.Key, rockWithHighestCompositionRate.Value);

        return null; //no specific wood type
    }
}