using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Config.Props;

namespace WearAndTear.DynamicPatches
{
    public static class BlockPatches
    {

        public static bool HasWearAndTearBehavior(this Block block) => Array.Exists(block.BlockEntityBehaviors, beh => beh.Name == "WearAndTear");

        public static bool AllowedToBePatched(this Block block)
        {
            if(block.HasWearAndTearBehavior()) return false;

            var blacklist = WearAndTearModSystem.Config.Features.GenericBlacklist;
            if(blacklist != null && Array.Exists(blacklist, blacklist => block.Code.ToString().Contains(blacklist))) return false;
            return true;
        }

        public static bool GenericWoodAllowedToBePatched(this Block block)
        {
            if(!genericWoodDomains.Contains(block.Code.Domain)) return false;

            var codeParts = block.Code.Path.Split('-');
            if (!genericWood.Contains(codeParts[0])) return false;

            if(!block.AllowedToBePatched()) return false;

            return true;
        }

        private static readonly string[] genericWoodDomains = new string[]
        {
            "game",
            "vanvar",
            "wildcrafttree"
        };

        private static readonly string[] genericWood = new string[]
        {
            "woodentoggle",
            "angledgears",
            "clutch",
            "largegear3",
            "pulverizerframe"
        };

        public static WearAndTearPartProps DefaultWoodFramePartProps => new()
        {
            Name = "Frame (Wood)",
            IsCritical = true,
            RepairType = "wood",
            MaintenanceLimit = .5f,
            Decay = new WearAndTearDecayProps[]
            {
                new()
                {
                    Type = "humidity"
                },
                new()
                {
                    Type = "time"
                }
            }
        };

        public static WearAndTearPartProps DefaultWaxPartProps => new()
        {
            Name = "Wax",
            RepairType = "wax",
            Decay = new WearAndTearDecayProps[]
            {
                new()
                {
                    Type = "humidity"
                }
            }
        };

        public static WearAndTearProtectivePartProps DefaultWaxProtectivePartProps => new()
        {
            EffectiveFor = new WearAndTearProtectiveTargetProps[]
            {
                new()
                {
                    RepairType = "wood"
                }
            }
        };

        public static void AddWearAndTearIfMissing(this Block block)
        {
            if (!block.HasWearAndTearBehavior())
            {
                block.BlockEntityBehaviors = block.BlockEntityBehaviors.Append(
                    new BlockEntityBehaviorType
                    {
                        Name = "WearAndTear"
                    }
                );
            }
        }

        public static void PatchDefaultWood(Block block)
        {
            if(!block.GenericWoodAllowedToBePatched()) return;

            var waxPartProps = (JContainer)JToken.FromObject(DefaultWaxPartProps);
            waxPartProps.Merge(JToken.FromObject(DefaultWaxProtectivePartProps));

            block.BlockEntityBehaviors = block.BlockEntityBehaviors.Append(
                new BlockEntityBehaviorType
                {
                    Name = "WearAndTear"
                },
                new BlockEntityBehaviorType
                {
                    Name = "WearAndTearPart",
                    properties = new JsonObject(JToken.FromObject(DefaultWoodFramePartProps))
                },
                new BlockEntityBehaviorType
                {
                    Name = "WearAndTearProtectivePart",
                    properties = new JsonObject(waxPartProps)
                }
            );
        }

        public static bool ContainsPartOfMaterial(this Block block, string material, string type = "WearAndTearPart") => Array.Exists(
            block.BlockEntityBehaviors,
            beh => (type == null || beh.Name == type) && beh.properties?.AsObject<WearAndTearPartProps>()?.RepairType == material
        );

        public static void Experimental_PatchWood(Block block)
        {
            if (block is BlockMPBase && block.BlockMaterial == EnumBlockMaterial.Wood && !block.ContainsPartOfMaterial("wood", null))
            {
                block.AddWearAndTearIfMissing();
                var woodPartProps = DefaultWoodFramePartProps;
                woodPartProps.AvgLifeSpanInYears *= 1.5f;

                var waxProps = DefaultWaxPartProps;
                waxProps.AvgLifeSpanInYears *= 1.5f;

                var waxPartProps = (JContainer)JToken.FromObject(waxProps);
                waxPartProps.Merge(JToken.FromObject(DefaultWaxProtectivePartProps));

                block.BlockEntityBehaviors = block.BlockEntityBehaviors.Append(
                    new BlockEntityBehaviorType
                    {
                        Name = "WearAndTearPart",
                        properties = new JsonObject(JToken.FromObject(woodPartProps))
                    },
                    new BlockEntityBehaviorType
                    {
                        Name = "WearAndTearProtectivePart",
                        properties = new JsonObject(waxPartProps)
                    }
                );
            }
        }

        public static WearAndTearPartProps DefaultSailPartProps => new()
        {
            Name = "sail",
            RepairType = "cloth",
            Decay = new WearAndTearDecayProps[]
            {
                new()
                {
                    Type = "wind"
                }
            }
        };

        public static WearAndTearPartProps DefaultHelveItemPartProps => new()
        {
            Name = "HelveItem"
        };

        public static WearAndTearPartProps DefaultPulverizerItemPartProps => new()
        {
            Name = "PulverizerItem"
        };

        public static void PatchWindmill(Block block)
        {
            if (block is BlockWindmillRotor || block.GetType().Name == "BlockWindmillRotorEnhanced" && block.AllowedToBePatched())
            {
                block.BlockEntityBehaviors = block.BlockEntityBehaviors.Append(
                    new BlockEntityBehaviorType
                    {
                        Name = "WearAndTear"
                    },
                    new BlockEntityBehaviorType
                    {
                        Name = "WearAndTearSail",
                        properties = new JsonObject(JToken.FromObject(DefaultSailPartProps))
                    }
                );

                ((JContainer)block.Attributes.Token).Merge(JToken.FromObject(new
                {
                    mechanicalPower = new
                    {
                        renderer = "wearandtear:windmillrotor"
                    }
                }));
            }
        }

        public static void PatchHelve(Block block)
        {
            if (block is BlockHelveHammer && block.AllowedToBePatched())
            {
                block.BlockEntityBehaviors = block.BlockEntityBehaviors.Append(
                    new BlockEntityBehaviorType
                    {
                        Name = "WearAndTear"
                    },
                    new BlockEntityBehaviorType
                    {
                        Name = "WearAndTearHelveItem",
                        properties = new JsonObject(JToken.FromObject(DefaultHelveItemPartProps))
                    }
                );
            }
        }

        public static void PatchPulverizer(Block block)
        {
            if(block is BlockPulverizer)
            {
                block.AddWearAndTearIfMissing();

                block.BlockEntityBehaviors = block.BlockEntityBehaviors.Append(
                    new BlockEntityBehaviorType
                    {
                        Name = "WearAndTearPulverizerItem",
                        properties = new JsonObject(JToken.FromObject(DefaultPulverizerItemPartProps))
                    }
                );

                //TODO other parts
            }
        }
    }
}
