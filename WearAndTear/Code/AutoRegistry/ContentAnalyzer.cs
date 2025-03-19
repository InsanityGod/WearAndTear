using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code.Enums;

namespace WearAndTear.Code.AutoRegistry
{
    public class ContentAnalyzer
    {
        public static Dictionary<string, ContentAnalyzer> Lookup { get; set; } = new();

        public CollectibleObject Collectible { get; set; }

        public static ContentAnalyzer GetOrCreate(CollectibleObject collectible)
        {
            if(Lookup.TryGetValue(collectible.Code, out var result)) return result;
            return Lookup[collectible.Code] = new ContentAnalyzer(collectible);
        }
        private ContentAnalyzer(CollectibleObject collectible) => Collectible = collectible;

        public Dictionary<string, float> MetalContent { get; set; } = new();
        public Dictionary<string, float> WoodContent { get; set; } = new();

        private void AnalyzeTextures()
        {
            if(Collectible is not Block block) return;
            if (block.Textures == null) return;

            if (block is BlockPulverizer && block.Code.Domain == "vanvar") return; //Thank you other mod creator for putting a bunch of unused metal textures in your block type definition...

            var metalKeys = block.Textures.Keys.Where(key => WearAndTearModSystem.Config.AutoPartRegistry.MetalConfig.ContainsKey(key.ToLower())).ToList();
            if (metalKeys.Count != 0)
            {
                var metal = metalKeys.Count == 1 ? metalKeys[0] : "default";

                MetalContent[metal] = MetalContent.TryGetValue(metal, out var current) ? current : 0;
                return;
                //TODO we could probably look through shape to calculate metal composition percentage but ehh let's not go overkill for now anyway
            }

            var pathMetals = block.Textures.Values.Select(path => WearAndTearModSystem.Config.AutoPartRegistry.MetalConfig.Keys.FirstOrDefault(metal => path.ToString().Contains(metal)))
                .Where(value => value != null)
                .Distinct()
                .ToList();

            if (pathMetals.Count != 0)
            {
                var metal = pathMetals.Count == 1 ? pathMetals[0] : "default";
                MetalContent[metal] = MetalContent.TryGetValue(metal, out var current) ? current : 0;
            }
        }

        private bool AnalyzeRecipes(ICoreAPI api)
        {
            var craftedBy = api.World.GridRecipes.Where(recipe => recipe.Output.ResolvedItemstack.Collectible.CodeWithoutOrientation() == Collectible.CodeWithoutOrientation()).ToList();

            if (craftedBy.Count == 0) return false;

            var recipeContent = craftedBy.Select(recipe =>
            {
                var outputAmmount = recipe.Output.ResolvedItemstack.StackSize;

                var validIngredients = recipe.resolvedIngredients
                .Where(item => item?.ResolvedItemstack != null && !item.IsTool)
                .Select(ingredient => (ingredient.ResolvedItemstack.Collectible, ingredient.ResolvedItemstack.StackSize / outputAmmount))
                .GroupBy(item => item.Collectible)
                .ToDictionary(item => item.Key, item => item.Sum(a => a.Item2));

                var woodContent = new Dictionary<string, float>();
                var metalContent = new Dictionary<string, float>();

                foreach ((var ingredient, var amount) in validIngredients)
                {
                    var smeltStack = ingredient.CombustibleProps?.SmeltedStack?.ResolvedItemstack;
                    if(smeltStack != null && smeltStack.Collectible is ItemIngot ingot)
                    {
                        var metalType = ingot.GetMetalType();
                        var output = (float)smeltStack.StackSize / (float)ingredient.CombustibleProps.SmeltedRatio;
                        output = output * 4 * amount;
                        metalContent[metalType] = metalContent.TryGetValue(metalType, out var current) ? current + output : output;
                        continue;
                    }

                    if(ingredient is ItemIngot ingot2)
                    {
                        var metalType = ingot2.GetMetalType();
                        var output = amount * 4;
                        metalContent[metalType] = metalContent.TryGetValue(metalType, out var current) ? current + output : output;
                        continue;
                    }
                    //TODO other means of getting metal (like smithing recipes from ingot)

                    if (ingredient.FirstCodePart() == "log")
                    {
                        var woodType = ingredient.Variant["wood"];
                        if (!string.IsNullOrEmpty(woodType))
                        {
                            var woodAmmount = 12 * amount;
                            woodContent[woodType] = woodContent.TryGetValue(woodType, out var current) ? current + woodAmmount : woodAmmount;
                            continue;
                        }
                    }

                    if(ingredient.FirstCodePart() == "planks")
                    {
                        var woodType = ingredient.Variant["wood"];
                        if (!string.IsNullOrEmpty(woodType))
                        {
                            var woodAmmount = 4 * amount;
                            woodContent[woodType] = woodContent.TryGetValue(woodType, out var current) ? current + woodAmmount : woodAmmount;
                            continue;
                        }
                    }

                    if(ingredient.FirstCodePart() == "plank")
                    {
                        var woodType = ingredient.Variant["wood"];
                        if (!string.IsNullOrEmpty(woodType))
                        {
                            var woodAmmount = amount;
                            woodContent[woodType] = woodContent.TryGetValue(woodType, out var current) ? current + woodAmmount : woodAmmount;
                            continue;
                        }
                    }

                    var analyzer = GetOrCreate(ingredient);
                    if(analyzer.State == EAnalyzeState.Analyzing) continue; //Skipping recursive recipes
                    analyzer.Analyze(api);
                    
                    foreach((var analyzedContent, var AnalyzedAmount) in analyzer.WoodContent)
                    {
                        woodContent[analyzedContent] = woodContent.TryGetValue(analyzedContent, out var current) ? current + AnalyzedAmount : AnalyzedAmount;
                    }

                    foreach ((var analyzedContent, var AnalyzedAmount) in analyzer.MetalContent)
                    {
                        metalContent[analyzedContent] = metalContent.TryGetValue(analyzedContent, out var current) ? current + AnalyzedAmount : AnalyzedAmount;
                    }
                }

                return (woodContent, metalContent);
            }).ToList();

            WoodContent = recipeContent
                .SelectMany(item => item.woodContent)
                .GroupBy(item => item.Key)
                .ToDictionary(item => item.Key, item => item.Average(a => a.Value));

            if (WearAndTearModSystem.Config.AutoPartRegistry.RequireAllRecipesToContainMetal && recipeContent.Exists(item => item.metalContent.Count == 0)) return true;

            MetalContent = recipeContent
                .SelectMany(item => item.metalContent)
                .GroupBy(item => item.Key)
                .ToDictionary(item => item.Key, item => item.Average(a => a.Value));

            return true;
        }

        public EAnalyzeState State { get; private set; } = EAnalyzeState.NotStarted;
        
        public void Analyze(ICoreAPI api)
        {
            if(State == EAnalyzeState.Analyzed) return; //Already analyzed
            if(State == EAnalyzeState.Analyzing) throw new InvalidOperationException("Attempt at recursive analysis");
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
            if (MetalContent.Count == 0) return null; //Doesn't contain metal

            var totalMetalCount = MetalContent.Sum(metal => metal.Value);
            var metalWithHighestCompositionRate = MetalContent.OrderByDescending(metal => metal.Value).First();

            if (metalWithHighestCompositionRate.Value / totalMetalCount > WearAndTearModSystem.Config.AutoPartRegistry.MinimalMetalCompositionPercentage) return (metalWithHighestCompositionRate.Key, metalWithHighestCompositionRate.Value);

            return ("unknown", 0);
        }

        public (string Wood, float ContentLevel)? FindFrameWood()
        {
            if (WoodContent.Count == 0) return null; //Doesn't contain wood

            var totalWoodCount = WoodContent.Sum(wood => wood.Value);
            var woodWithHighestCompositionRate = WoodContent.OrderByDescending(wood => wood.Value).First();

            if (woodWithHighestCompositionRate.Value / totalWoodCount > WearAndTearModSystem.Config.AutoPartRegistry.MinimalWoodCompositionPercentage) return (woodWithHighestCompositionRate.Key, woodWithHighestCompositionRate.Value);

            return null; //no specific wood type
        }
    }
}
