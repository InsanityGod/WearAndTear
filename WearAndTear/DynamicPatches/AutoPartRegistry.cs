﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Config.Props;
using WearAndTear.Interfaces;

namespace WearAndTear.DynamicPatches
{
    public static class AutoPartRegistry
    {
        public const string FramePrefix = "Frame ";

        internal static ICoreAPI Api { get; set; }

        public static bool HasWearAndTearBehavior(this Block block) => Array.Exists(
            block.BlockEntityBehaviors,
            beh => typeof(IWearAndTear).IsAssignableFrom(Api.ClassRegistry.GetBlockEntityBehaviorClass(beh.Name))
        );
        
        public static bool HasWearAndTearFramePart(this Block block) => Array.Exists(
            block.BlockEntityBehaviors,
            beh => typeof(IWearAndTearPart).IsAssignableFrom(Api.ClassRegistry.GetBlockEntityBehaviorClass(beh.Name))
                    && beh.properties != null
                    && beh.properties["Name"].AsString(string.Empty).StartsWith(FramePrefix)
        );

        public static bool HasWearAndTearPart(this Block block, string name) => Array.Exists(
            block.BlockEntityBehaviors,
            beh => typeof(IWearAndTearPart).IsAssignableFrom(Api.ClassRegistry.GetBlockEntityBehaviorClass(beh.Name))
                    && beh.properties != null
                    && beh.properties["Name"].AsString(string.Empty) == name
        );

        //TODO check medieval expasion waterwheel (and add to blacklist if needed)
        public static bool IsBlacklisted(Block block) => 
            Array.Exists(
                WearAndTearModSystem.Config.AutoPartRegistry.ModBlacklist,
                modId => block.Code.Domain == modId
            ) || 
            Array.Exists(
                WearAndTearModSystem.Config.AutoPartRegistry.CodeBlacklist,
                codeMatch => WildcardUtil.Match(codeMatch, block.Code.ToString())
            );

        public static void EnsureBaseWearAndTear(this Block block)
        {
            if (!block.HasWearAndTearBehavior())
            {
                block.BlockEntityBehaviors = block.BlockEntityBehaviors.Append(new BlockEntityBehaviorType { Name = "WearAndTear" });
            }
        }

        public static void EnsureProtectivePart(this Block block, WearAndTearProtectivePartConfig part)
        {
            if(block.HasWearAndTearPart(part.PartProps.Name)) return;

            block.BlockEntityBehaviors = block.BlockEntityBehaviors.Append(new BlockEntityBehaviorType
            {
                Name = "WearAndTearOptionalProtectivePart",
                properties = new JsonObject(part.AsMergedJContainer())
            });
        }

        public static void EnsureProtectivePart(this Block block, EnumBlockMaterial protectiveType)
        {
            var protectiveDefinitions = WearAndTearModSystem.Config.AutoPartRegistry.DefaultProtectivePartProps.GetValueOrDefault(protectiveType);
            if(protectiveDefinitions == null) return;

            foreach ( var definition in protectiveDefinitions) block.EnsureProtectivePart(definition);
        }


        public static void EnsureFrameWearAndTearPart(this Block block)
        {
            if(block.HasWearAndTearFramePart()) return;

            var frameProps = WearAndTearModSystem.Config.AutoPartRegistry.DefaultFrameProps.GetValueOrDefault(block.BlockMaterial);
            if(frameProps == null) return;

            block.BlockEntityBehaviors = block.BlockEntityBehaviors.Append(new BlockEntityBehaviorType
            {
                Name = "WearAndTearPart",
                properties = new JsonObject(JToken.FromObject(frameProps))
            });

            EnsureProtectivePart(block, block.BlockMaterial);
        }

        public static void EnsureMetalReinforcement(this Block block, string metal)
        {
            var template = WearAndTearModSystem.Config.AutoPartRegistry.MetalReinforcementTemplate;
            if(template == null) return; // Not sure why you would do this but oh well

            var props = template.AsMergedJContainer();
            props[nameof(WearAndTearPartProps.Name)] = props.Value<string>(nameof(WearAndTearPartProps.Name)).Replace("*", CultureInfo.InvariantCulture.TextInfo.ToTitleCase(metal));
            props[nameof(WearAndTearPartProps.RepairType)] = props.Value<string>(nameof(WearAndTearPartProps.RepairType)).Replace("*", metal);

            if (!WearAndTearModSystem.Config.AutoPartRegistry.MetalConfig.TryGetValue(metal, out var metalConfig) && !WearAndTearModSystem.Config.AutoPartRegistry.MetalConfig.TryGetValue("default", out metalConfig))
            {
                metalConfig = new();
            }

            props[nameof(WearAndTearPartProps.AvgLifeSpanInYears)] = metalConfig.AvgLifeSpanInYears;
            foreach(var target in props[nameof(WearAndTearProtectivePartProps.EffectiveFor)])
            {
                target[nameof(WearAndTearProtectiveTargetProps.DecayMultiplier)] = metalConfig.DecayMultiplier;
            }

            block.BlockEntityBehaviors = block.BlockEntityBehaviors.Append(new BlockEntityBehaviorType
            {
                Name = "WearAndTearProtectivePart",
                properties = new JsonObject(props)
            });
        }

        public static void Register(Block block)
        {
            //Millwright sail is deciding to turn again despite sail entirely broken
            if(
                (
                    block is not BlockMPBase
                    && (block is not BlockFruitPress || !WearAndTearModSystem.Config.AutoPartRegistry.IncludeFruitPress)
                ) 
                && !block.HasWearAndTearBehavior()) return; //Only handle non mechanical blocks if they where already marked as WearAndTear

            if(IsBlacklisted(block)) return;

            block.EnsureBaseWearAndTear();
            block.EnsureFrameWearAndTearPart();
            block.DetectAndAddMetalReinforcements();
            block.CleanupWearAndTearAutoRegistry();
        }

        public static string CodeWithoutOrientation(this CollectibleObject obj)
        {
            int index = obj.VariantStrict.IndexOfKey("side");
            if(index == -1) index = obj.VariantStrict.IndexOfKey("rotation");
            if(index == -1) index = obj.VariantStrict.IndexOfKey("orientation");
            if(index == -1) return obj.Code.ToString();


            return string.Join('-', obj.Code.ToString().Split('-').RemoveEntry(index + 1));
        }

        public static string TryGetMetalFromTexture(this Block block)
        {
            if(block.Textures == null) return null;

            var metalKeys = block.Textures.Keys.Where(key => WearAndTearModSystem.Config.AutoPartRegistry.MetalConfig.ContainsKey(key.ToLower())).ToList();
            if(metalKeys.Count != 0)
            {
                if(metalKeys.Count == 1) return metalKeys[0];
                return "unknown";
                //TODO we could probably look through shape to calculate metal composition percentage but ehh let's not go overkill for now anyway
            }

            var pathMetals = block.Textures.Values.Select(path => WearAndTearModSystem.Config.AutoPartRegistry.MetalConfig.Keys.FirstOrDefault(metal => path.ToString().Contains(metal)))
                .Where(value => value != null)
                .Distinct()
                .ToList();

            if(pathMetals.Count != 0)
            {
                if(pathMetals.Count == 1) return pathMetals[0];
                return "unknown";
            }

            return null;
        }

        public static void DetectAndAddMetalReinforcements(this Block block)
        {
            if(block.BlockMaterial == EnumBlockMaterial.Metal) return; //Otherwise all metal objects would end up being metal reinforced :p

            var metalVariant = block.Variant["metal"];
            if(metalVariant != null)
            {
                block.EnsureMetalReinforcement(metalVariant);
                return;
            }

            //TODO make an abstract method for this
            var craftedBy = Api.World.GridRecipes.Where(recipe => recipe.Output.ResolvedItemstack.Collectible.CodeWithoutOrientation() == block.CodeWithoutOrientation()).ToList();

            if(craftedBy.Count == 0)
            {
                //Seems like we can't craft this item so let's try a texture search :p
                var textureMetal = block.TryGetMetalFromTexture();
                if(textureMetal != null) block.EnsureMetalReinforcement(textureMetal);
                return;
            }

            var metalComponents = craftedBy.Select(
                recipe => recipe.resolvedIngredients.Where(
                    item => item != null && !item.IsTool
                ).Select(
                    item => (item.ResolvedItemstack?.Collectible.Variant["metal"], item.ResolvedItemstack?.StackSize ?? 1)
                ).Where(
                    metal => metal.Item1 != null
                ).ToList()
            ).Where(item => item.Any())
            .ToList();

            if(metalComponents.Count == 0) return; //Doesn't contain metal
            if(metalComponents.Count != craftedBy.Count) return; //This would mean you can craft it with metal but you are not required to? not sure what to do in this case

            var metalComposition = metalComponents
                .SelectMany(metal => metal)
                .GroupBy(metal => metal.Item1)
                .ToDictionary(metal => metal.Key, metal => metal.Sum(item => item.Item2));
            
            var totalMetalCount = metalComposition.Sum(metal => metal.Value);
            var metalWithHighestCompositionRate = metalComposition.OrderByDescending(metal => metal.Value).First();

            if((float)metalWithHighestCompositionRate.Value / totalMetalCount > WearAndTearModSystem.Config.AutoPartRegistry.MinimalMetalCompositionPercentage)
            {
                block.EnsureMetalReinforcement(metalWithHighestCompositionRate.Key);
                return;
            }
            block.EnsureMetalReinforcement("unknown");
        }

        public static void CleanupWearAndTearAutoRegistry(this Block block)
        {
            if(block.BlockEntityBehaviors.Count(beh => beh.Name.StartsWith("WearAndTear")) == 1)
            {
                block.BlockEntityBehaviors = block.BlockEntityBehaviors.Where(beh => !beh.Name.StartsWith("WearAndTear")).ToArray();
                return;
            }

            block.BlockEntityBehaviors = block.BlockEntityBehaviors.OrderBy(type => SortOrder(Api, type)).ToArray();
        }
        public static int SortOrder(ICoreAPI api, BlockEntityBehaviorType blockEntityBehaviorType)
        {
            var type = api.ClassRegistry.GetBlockEntityBehaviorClass(blockEntityBehaviorType.Name);
            if (type == null) return 0; // This should never happen
        
            if (typeof(IWearAndTear).IsAssignableFrom(type))
            {
                return 2;
            }

            bool isPart = typeof(IWearAndTearPart).IsAssignableFrom(type);
            bool isProtectivePart = typeof(IWearAndTearProtectivePart).IsAssignableFrom(type);
            bool isOptionalPart = typeof(IWearAndTearOptionalPart).IsAssignableFrom(type);
        
            if (isPart && !isOptionalPart)
            {
                return 3;
            }

            if (isProtectivePart && !isOptionalPart)
            {
                return 4;
            }

            if(isPart && !isProtectivePart)
            {
                return 5;
            }
        
            if (isOptionalPart && !isProtectivePart)
            {
                return 6;
            }
        
            if (isProtectivePart)
            {
                return 7;
            }
        
            return 1; // Fallback case for behaviors that are not from this mod
        }
    }
}
