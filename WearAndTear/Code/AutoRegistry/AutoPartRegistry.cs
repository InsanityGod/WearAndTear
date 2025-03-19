using HarmonyLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code.AutoRegistry.Compatibility;
using WearAndTear.Code.Interfaces;
using WearAndTear.Config.Props;

namespace WearAndTear.Code.AutoRegistry
{
    public static class AutoPartRegistry
    {

        internal static ICoreAPI Api { get; set; }

        public static bool HasWearAndTearBehavior(this Block block) => Array.Exists(
            block.BlockEntityBehaviors,
            beh => typeof(IWearAndTear).IsAssignableFrom(Api.ClassRegistry.GetBlockEntityBehaviorClass(beh.Name))
        );

        public static BlockEntityBehaviorType FindWearAndTearBehavior(this Block block) => Array.Find(
            block.BlockEntityBehaviors,
            beh => typeof(IWearAndTear).IsAssignableFrom(Api.ClassRegistry.GetBlockEntityBehaviorClass(beh.Name))
        );

        public static bool HasWearAndTearFramePart(this Block block) => Array.Exists(
            block.BlockEntityBehaviors,
            beh => typeof(IWearAndTearPart).IsAssignableFrom(Api.ClassRegistry.GetBlockEntityBehaviorClass(beh.Name))
                    && beh.properties != null
                    && beh.properties["Code"].AsString(string.Empty).Split(":")[0] == "frame"
        );

        public static bool HasWearAndTearPart(this Block block, string name) => Array.Exists(
            block.BlockEntityBehaviors,
            beh => typeof(IWearAndTearPart).IsAssignableFrom(Api.ClassRegistry.GetBlockEntityBehaviorClass(beh.Name))
                    && beh.properties != null
                    && beh.properties["Code"].AsString(string.Empty) == name
        );

        public static bool MayMergeBehaviors(this Block block)
        {
            var wearAndTearBehavior = block.FindWearAndTearBehavior();
            return wearAndTearBehavior?.properties != null && wearAndTearBehavior.properties["MergeWithAutoRegistry"].AsBool();
        }

        public static bool OnlyMergeIfNormallyAutoReg(this Block block)
        {
            var wearAndTearBehavior = block.FindWearAndTearBehavior();
            return wearAndTearBehavior?.properties != null && wearAndTearBehavior.properties["OnlyMergeIfNormallyAutoReg"].AsBool();
        }

        public static void MergeOrAddBehavior(this Block block, string behaviorName, JContainer properties)
        {
            var toMerge = block.MayMergeBehaviors() ? Array.Find(block.BlockEntityBehaviors, item =>
            {
                if (item.Name != behaviorName) return false;
                if (item.properties != null && item.properties["Code"].AsString() != null && item.properties["Code"].AsString() != properties["Code"].Value<string>()) return false;
                //TODO maybe compare material variant as well
                
                return true;
            }) : null;

            if (toMerge != null)
            {
                properties.Merge(toMerge.properties?.Token);
                toMerge.properties = new JsonObject(properties);
            }
            else
            {
                block.BlockEntityBehaviors = block.BlockEntityBehaviors.Append(new BlockEntityBehaviorType
                {
                    Name = behaviorName,
                    properties = new JsonObject(properties)
                });
            }
        }

        //TODO check medieval expasion waterwheel (and add to blacklist if needed)
        public static bool IsBlacklisted(Block block) =>
            Array.Exists(
                WearAndTearModSystem.Config.AutoPartRegistry.ModBlacklist,
                modId => block.Code.Domain == modId
            ) ||
            Array.Exists(
                WearAndTearModSystem.Config.AutoPartRegistry.CodeBlacklist,
                codeMatch => MatchString(codeMatch, block.Code)
            );

        public static bool MatchString(string needle, AssetLocation haystack) =>
            needle.Contains(':') ? WildcardUtil.Match((AssetLocation)needle, haystack) : WildcardUtil.Match(needle, haystack.Path);

        public static BlockEntityBehaviorType EnsureBaseWearAndTear(this Block block, bool allowMerge = false)
        {
            var behavior = block.FindWearAndTearBehavior();
            if (behavior == null)
            {
                behavior = new BlockEntityBehaviorType { Name = "WearAndTear" };

                if (allowMerge)
                {
                    behavior.properties = new JsonObject(JToken.FromObject(new { MergeWithAutoRegistry = true }));
                }

                block.BlockEntityBehaviors = block.BlockEntityBehaviors.Append(behavior);
            }
            return behavior;
        }

        public static void EnsureProtectivePart(this Block block, WearAndTearProtectivePartConfig part) => block.MergeOrAddBehavior("WearAndTearOptionalProtectivePart", part.AsMergedJContainer());

        public static void EnsureProtectivePart(this Block block, EnumBlockMaterial protectiveType)
        {
            var protectiveDefinitions = WearAndTearModSystem.Config.AutoPartRegistry.DefaultProtectivePartProps.GetValueOrDefault(protectiveType);
            if (protectiveDefinitions == null) return;

            foreach (var definition in protectiveDefinitions) block.EnsureProtectivePart(definition);
        }

        public static void EnsureFrameWearAndTearPart(this Block block)
        {
            var frameProps = WearAndTearModSystem.Config.AutoPartRegistry.DefaultFrameProps.GetValueOrDefault(block.BlockMaterial);
            if (frameProps == null) return;

            var behaviorName = "WearAndTearPart";
            var behaviorProperties = JToken.FromObject(frameProps);
            if (block is BlockToolMold)
            {
                behaviorName = "WearAndTearMold";
                behaviorProperties[nameof(WearAndTearPartProps.Code)] = "wearandtear:mold";
                behaviorProperties[nameof(WearAndTearPartProps.Decay)] = JToken.FromObject(Array.Empty<WearAndTearDecayProps>());
            }

            if(block.BlockMaterial == EnumBlockMaterial.Wood)
            {
                var analyzer = ContentAnalyzer.GetOrCreate(block);
                analyzer.Analyze(Api);

                var frameWood = analyzer.FindFrameWood();
                if(frameWood != null)
                {
                    behaviorProperties["MaterialVariant"] = frameWood.Value.Wood;
                    behaviorProperties["ContentLevel"] = frameWood.Value.ContentLevel;
                    behaviorProperties["ScrapCode"] = behaviorProperties.Value<string>(nameof(WearAndTearPartProps.ScrapCode))?.Replace("*", frameWood.Value.Wood);
                }
            }

            block.MergeOrAddBehavior(behaviorName, (JContainer)behaviorProperties);

            block.EnsureProtectivePart(block.BlockMaterial);
        }

        public static void EnsureMetalReinforcement(this Block block, string metal, float contentLevel)
        {
            var template = WearAndTearModSystem.Config.AutoPartRegistry.MetalReinforcementTemplate;
            if (template == null) return; // Not sure why you would do this but oh well

            var props = template.AsMergedJContainer();
            props[nameof(WearAndTearPartProps.MaterialVariant)] = metal;
            props[nameof(WearAndTearPartProps.ContentLevel)] = contentLevel;
            props[nameof(WearAndTearPartProps.ScrapCode)] = props.Value<string>(nameof(WearAndTearPartProps.ScrapCode))?.Replace("*", metal);

            if (!WearAndTearModSystem.Config.AutoPartRegistry.MetalConfig.TryGetValue(metal, out var metalConfig) && !WearAndTearModSystem.Config.AutoPartRegistry.MetalConfig.TryGetValue("default", out metalConfig))
            {
                metalConfig = new();
            }

            props[nameof(WearAndTearPartProps.AvgLifeSpanInYears)] = metalConfig.AvgLifeSpanInYears;
            foreach (var target in props[nameof(WearAndTearProtectivePartProps.EffectiveFor)])
            {
                target[nameof(WearAndTearProtectiveTargetProps.DecayMultiplier)] = metalConfig.DecayMultiplier;
            }

            block.MergeOrAddBehavior("WearAndTearProtectivePart", props);
        }

        public static void Register(Block block)
        {
            if (IsBlacklisted(block)) return;
            var hasWearAndTear = block.HasWearAndTearBehavior();
            var isMechanicalBlock = block is BlockMPBase;
            var acceptFruitPress = WearAndTearModSystem.Config.AutoPartRegistry.IncludeFruitPress && block is BlockFruitPress;
            var entityClass = string.IsNullOrEmpty(block.EntityClass) ? null : Api.ClassRegistry.GetBlockEntity(block.EntityClass);

            var acceptMold = WearAndTearModSystem.Config.SpecialParts.Molds && entityClass != null && block is BlockToolMold && block.BlockMaterial == EnumBlockMaterial.Ceramic;

            if (!isMechanicalBlock && !acceptFruitPress && !acceptMold)
            {
                if (hasWearAndTear)
                {
                    if (!block.MayMergeBehaviors() || block.OnlyMergeIfNormallyAutoReg())
                    {
                        block.CleanupWearAndTearAutoRegistry();
                        return;
                    }
                }
                else return;
            }

            block.EnsureBaseWearAndTear();
            if (hasWearAndTear && !block.MayMergeBehaviors())
            {
                block.CleanupWearAndTearAutoRegistry();
                return;
            }

            if (block.Code.Domain == "axleinblocks")
            {
                AxleInBlocks.Register(block);
            }
            else if(block is not BlockIngotMold) //Ingot molds just have to be very special -_-
            {
                block.EnsureFrameWearAndTearPart();
                block.DetectAndAddMetalReinforcements();
            }

            if (block.Code.Domain == "linearpower" && block.Code.FirstCodePart() == "sawmill")
            {
                var props = JToken.FromObject(new WearAndTearPartProps
                {
                    Code = "linearpower:sawblade"
                }) as JContainer;
                props.Merge(JToken.FromObject(new WearAndTearGenericItemDisplayProps { ItemSlotIndex = 1 }));

                block.BlockEntityBehaviors = block.BlockEntityBehaviors.Append(new BlockEntityBehaviorType
                {
                    Name = "WearAndTearGenericItemDisplay",
                    properties = new JsonObject(props)
                });
            }

            block.CleanupWearAndTearAutoRegistry();
        }

        public static string CodeWithoutOrientation(this CollectibleObject obj)
        {
            int index = obj.VariantStrict.IndexOfKey("side");
            if (index == -1) index = obj.VariantStrict.IndexOfKey("rotation");
            if (index == -1) index = obj.VariantStrict.IndexOfKey("orientation");
            if (index == -1) return obj.Code.ToString();

            return string.Join('-', obj.Code.ToString().Split('-').RemoveAt(index + 1));
        }

        public static void DetectAndAddMetalReinforcements(this Block block)
        {
            if (block.BlockMaterial == EnumBlockMaterial.Metal) return; //Otherwise all metal objects would end up being metal reinforced :p

            var analyzer = ContentAnalyzer.GetOrCreate(block);
            analyzer.Analyze(Api);

            var reinforcementMetal = analyzer.FindReinforcementMetal();
            if (reinforcementMetal != null) block.EnsureMetalReinforcement(reinforcementMetal.Value.Metal, reinforcementMetal.Value.ContentLevel);
        }

        public static void CleanupWearAndTearAutoRegistry(this Block block)
        {
            //Removing all parts that do not have name (for overrides specified in patch that did not match an actual part)
            block.BlockEntityBehaviors = block.BlockEntityBehaviors.Where(beh =>
            {
                if (typeof(IWearAndTearPart).IsAssignableFrom(Api.ClassRegistry.GetBlockEntityBehaviorClass(beh.Name)))
                {
                    return beh.properties != null && !string.IsNullOrEmpty(beh.properties["Code"].AsString());
                }
                return true;
            }).ToArray();

            if (block.BlockEntityBehaviors.Count(beh => beh.Name.StartsWith("WearAndTear")) == 1)
            {
                block.BlockEntityBehaviors = block.BlockEntityBehaviors.Where(beh => !beh.Name.StartsWith("WearAndTear")).ToArray();
                return;
            }

            block.BlockEntityBehaviors = block.BlockEntityBehaviors.OrderBy(SortOrder).ToArray();
        }

        public static int SortOrder(BlockEntityBehaviorType blockEntityBehaviorType)
        {
            var type = Api.ClassRegistry.GetBlockEntityBehaviorClass(blockEntityBehaviorType.Name);
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

            if (isPart && !isProtectivePart)
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

        public static void ClearAnalyzerCache() => ContentAnalyzer.Lookup.Clear();
    }
}