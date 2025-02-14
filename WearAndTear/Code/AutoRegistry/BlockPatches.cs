using Newtonsoft.Json.Linq;
using Vintagestory.API.Common;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code;
using WearAndTear.Code.AutoRegistry;
using WearAndTear.Config.Props;

namespace WearAndTear.DynamicPatches
{
    public static class BlockPatches
    {
        //TODO Molds should live outside AutoPartRegistry scope as well
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
            if (WearAndTearModSystem.Config.SpecialParts.Clutch == null || block is not BlockClutch) return;
            block.EnsureBaseWearAndTear(true);
            //TODO special part
        }

        public static void PatchWindmill(Block block)
        {
            if (WearAndTearModSystem.Config.SpecialParts.WindmillSails == null) return;

            if (block is BlockWindmillRotor || block.GetType().Name == "BlockWindmillRotorEnhanced")
            {
                block.EnsureBaseWearAndTear(true);
                block.MergeOrAddBehavior("WearAndTearSail", (JContainer)JToken.FromObject(WearAndTearModSystem.Config.SpecialParts.WindmillSails));

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
                block.EnsureBaseWearAndTear(true);
                block.MergeOrAddBehavior("WearAndTearHelveItem", (JContainer)JToken.FromObject(DefaultHelveItemPartProps));
            }
        }

        public static void PatchPulverizer(Block block)
        {
            if (block is BlockPulverizer)
            {
                block.EnsureBaseWearAndTear(true);
                block.MergeOrAddBehavior("WearAndTearPulverizerItem", (JContainer)JToken.FromObject(DefaultPulverizerItemPartProps));

                //TODO other parts
            }
        }
    }
}