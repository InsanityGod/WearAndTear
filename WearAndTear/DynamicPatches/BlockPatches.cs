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
        public static WearAndTearPartProps DefaultHelveItemPartProps => new()
        {
            Name = "HelveItem"
        };

        public static WearAndTearPartProps DefaultPulverizerItemPartProps => new()
        {
            Name = "PulverizerItem"
        };

        public static void PatchClutch(Block block)
        {
            if(WearAndTearModSystem.Config.SpecialParts.Clutch == null || block is not BlockClutch) return;
            block.EnsureBaseWearAndTear();
            //TODO special part
        }

        public static void PatchWindmill(Block block)
        {
            if(WearAndTearModSystem.Config.SpecialParts.WindmillSails == null) return;

            if (block is BlockWindmillRotor || block.GetType().Name == "BlockWindmillRotorEnhanced")
            {
                block.EnsureBaseWearAndTear();

                block.BlockEntityBehaviors = block.BlockEntityBehaviors.Append(
                    new BlockEntityBehaviorType
                    {
                        Name = "WearAndTearSail",
                        properties = new JsonObject(JToken.FromObject(WearAndTearModSystem.Config.SpecialParts.WindmillSails))
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
            if (block is BlockHelveHammer)
            {
                block.EnsureBaseWearAndTear();
                block.BlockEntityBehaviors = block.BlockEntityBehaviors.Append(
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
                block.EnsureBaseWearAndTear();
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
