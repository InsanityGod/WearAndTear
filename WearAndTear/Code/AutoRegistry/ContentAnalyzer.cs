﻿using InsanityLib.Util;
using InsanityLib.Util.SpanUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code.Enums;
using WearAndTear.Code.Extensions;
using WearAndTear.Config.Server;

namespace WearAndTear.Code.AutoRegistry
{
    public class ContentAnalyzer
    {
        public static Dictionary<string, ContentAnalyzer> Lookup { get; set; } = new();

        public bool EncounteredDeadlock { get; private set; } = false;
        public CollectibleObject Collectible { get; set; }
        public ICoreAPI Api { get; set; }

        public static ContentAnalyzer GetOrCreate(ICoreAPI api, CollectibleObject collectible)
        {
            if(collectible is Block block) collectible = block.GetPlacedByItem(api);

            if (Lookup.TryGetValue(collectible.Code, out var result)) return result;
            return Lookup[collectible.Code] = new ContentAnalyzer(api, collectible);
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

        private (Dictionary<string, float> woodContent, Dictionary<string, float> metalContent, Dictionary<string, float> rockContent) ExtractContent(GridRecipe recipe)
        {
            var outputAmmount = recipe.Output.Quantity;
            //TODO see if we can resolve wildcard to some degree

            var validIngredients = recipe.resolvedIngredients
            .Where(item => item != null && !item.IsTool
            && (
                item.ResolvedItemstack != null
                || item.IsWildCard && item.AllowedVariants?.Length == 1
            ))
            .Select(ingredient =>
            {
                var amount = (float)ingredient.Quantity / (float)outputAmmount;

                if(ingredient.ResolvedItemstack != null) return (ingredient.ResolvedItemstack.Collectible, amount);
                return (Collectible: Api.World.GetCollectibleObject(ingredient.Code.FillWildCard(ingredient.AllowedVariants[0])), amount);
            })
            .Where(pair => pair.Collectible != null)
            .GroupBy(item => item.Collectible)
            .ToDictionary(item => item.Key, item => item.Sum(a => a.amount));
            
            //TODO see if we can ignore stuff that is returned like buckets
            var woodContent = new Dictionary<string, float>();
            var metalContent = new Dictionary<string, float>();
            var rockContent = new Dictionary<string, float>();

            foreach ((var ingredient, var amount) in validIngredients)
            {
                var smeltStack = ingredient.CombustibleProps?.SmeltedStack?.ResolvedItemstack;
                if (smeltStack != null && smeltStack.Collectible is ItemIngot ingot)
                {
                    var metalType = ingot.GetMetalType();
                    var output = (float)smeltStack.StackSize / (float)ingredient.CombustibleProps.SmeltedRatio;
                    output = output * 4 * amount;
                    metalContent[metalType] = metalContent.TryGetValue(metalType, out var current) ? current + output : output;
                    continue;
                }

                if (ingredient is ItemIngot ingot2)
                {
                    var metalType = ingot2.GetMetalType();
                    var output = amount * 4;
                    metalContent[metalType] = metalContent.TryGetValue(metalType, out var current) ? current + output : output;
                    continue;
                }

                //TODO other means of getting metal (like smithing recipes from ingot)
                //TODO make amore generic and extensible scrap system
                var firstCodePart = ingredient.FirstCodePartAsSpan();
                if (firstCodePart.SequenceEqual("log"))
                {
                    var woodType = ingredient.Variant["wood"];
                    if (!string.IsNullOrEmpty(woodType))
                    {
                        var woodAmount = 12 * amount;
                        woodContent[woodType] = woodContent.TryGetValue(woodType, out var current) ? current + woodAmount : woodAmount;
                        continue;
                    }
                }
                else if (firstCodePart.SequenceEqual("planks"))
                {
                    var woodType = ingredient.Variant["wood"];
                    if (!string.IsNullOrEmpty(woodType))
                    {
                        var woodAmount = 4 * amount;
                        woodContent[woodType] = woodContent.TryGetValue(woodType, out var current) ? current + woodAmount : woodAmount;
                        continue;
                    }
                }
                else if (firstCodePart.SequenceEqual("plank"))
                {
                    var woodType = ingredient.Variant["wood"];
                    if (!string.IsNullOrEmpty(woodType))
                    {
                        var woodAmount = amount;
                        woodContent[woodType] = woodContent.TryGetValue(woodType, out var current) ? current + woodAmount : woodAmount;
                        continue;
                    }
                }
                else if (firstCodePart.SequenceEqual("rock"))
                {
                    var rockType = ingredient.Variant["rock"];
                    if (!string.IsNullOrEmpty(rockType))
                    {
                        var rockAmount = amount;
                        rockContent[rockType] = woodContent.TryGetValue(rockType, out var current) ? current + rockAmount : rockAmount;
                        continue;
                    }
                }
                else if (firstCodePart.SequenceEqual("stone"))
                {
                    var rockType = ingredient.Variant["rock"];
                    if (!string.IsNullOrEmpty(rockType))
                    {
                        var rockAmount = 4 * amount;
                        rockContent[rockType] = woodContent.TryGetValue(rockType, out var current) ? current + rockAmount : rockAmount;
                        continue;
                    }
                }

                var analyzer = GetOrCreate(Api, ingredient);
                if (analyzer.State == EAnalyzeState.Analyzing)
                {
                    EncounteredDeadlock = true;
                    continue; //Skipping recursive recipe
                }

                analyzer.Analyze(Api);

                foreach ((var analyzedContent, var AnalyzedAmount) in analyzer.WoodContent)
                {
                    var woodAmount = AnalyzedAmount * amount;
                    woodContent[analyzedContent] = woodContent.TryGetValue(analyzedContent, out var current) ? current + woodAmount : woodAmount;
                }

                foreach ((var analyzedContent, var AnalyzedAmount) in analyzer.MetalContent)
                {
                    var metalAmount = AnalyzedAmount * amount;
                    metalContent[analyzedContent] = metalContent.TryGetValue(analyzedContent, out var current) ? current + metalAmount : metalAmount;
                }

                foreach ((var analyzedContent, var AnalyzedAmount) in analyzer.RockContent)
                {
                    var rockAmount = AnalyzedAmount * amount;
                    rockContent[analyzedContent] = metalContent.TryGetValue(analyzedContent, out var current) ? current + rockAmount : rockAmount;
                }
            }

            return (woodContent, metalContent, rockContent);
        }

        private bool AnalyzeRecipes(ICoreAPI api)
        {
            var craftedBy = api.World.GridRecipes.Where(recipe => recipe.Output?.Type == Collectible.ItemClass && ComparisonUtil.CompareWithoutOrientation(recipe.Output?.ResolvedItemstack?.Collectible, Collectible)).ToList();
            if (craftedBy.Count == 0) return false;

            var recipeContent = craftedBy.Select(ExtractContent).ToList();

            WoodContent = recipeContent
                .SelectMany(item => item.woodContent)
                .GroupBy(item => item.Key)
                .ToDictionary(item => item.Key, item => item.Average(a => a.Value));

            if (AutoPartRegistryConfig.Instance.RequireAllRecipesToContainMetal && recipeContent.Exists(item => item.metalContent.Count == 0)) return true;

            MetalContent = recipeContent
                .SelectMany(item => item.metalContent)
                .GroupBy(item => item.Key)
                .ToDictionary(item => item.Key, item => item.Average(a => a.Value));

            RockContent = recipeContent
                .SelectMany(item => item.rockContent)
                .GroupBy(item => item.Key)
                .ToDictionary(item => item.Key, item => item.Average(a => a.Value));

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
            if (MetalContent.Count == 0) return null; //Doesn't contain metal

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
}