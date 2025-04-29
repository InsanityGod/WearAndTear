using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code.AutoRegistry;
using WearAndTear.Config.Props;
using WearAndTear.Config.Server;

namespace WearAndTear.DynamicPatches
{
    public static class BlockPatches
    {
        //TODO Molds should live outside AutoPartRegistry scope as well
        public static PartProps DefaultHelveItemPartProps => new()
        {
            Code = "wearandtear:helveitem",
            Decay = Array.Empty<DecayProps>()
        };

        public static PartProps DefaultPulverizerItemPartProps => new()
        {
            Code = "wearandtear:pulverizeritem",
            Decay = Array.Empty<DecayProps>()
        };

        public static void PatchClutch(Block block)
        {
            if (SpecialPartsConfig.Instance.Clutch == null || block is not BlockClutch) return;
            block.EnsureBaseWearAndTear(true);
            //TODO special part
        }

        public static void PatchWindmill(Block block)
        {
            if (SpecialPartsConfig.Instance.WindmillSails == null) return;

            if (block is BlockWindmillRotor || block.GetType().Name == "BlockWindmillRotorEnhanced")
            {
                block.EnsureBaseWearAndTear(true);
                block.MergeOrAddBehavior("wearandtear:WindmillSailPart", (JContainer)JToken.FromObject(SpecialPartsConfig.Instance.WindmillSails));

                ((JContainer)block.Attributes.Token).Merge(JToken.FromObject(new
                {
                    mechanicalPower = new
                    {
                        renderer = "wearandtear:windmillrotor"
                    }
                }));
            }
        }

        public static void PatchIngotMold(Block block)
        {
            if (!SpecialPartsConfig.Instance.Molds || block is not BlockIngotMold) return;

            block.EnsureBaseWearAndTear(true);
            var frameProps = AutoPartRegistryConfig.Instance.DefaultFrameProps.GetValueOrDefault(block.BlockMaterial);
            var frame = JToken.FromObject(frameProps);
            frame[nameof(PartProps.Decay)] = JToken.FromObject(Array.Empty<DecayProps>());
            ((JContainer)frame).Merge(JToken.FromObject(new DurabilityUsageProps()));

            frame["Code"] = "wearandtear:ingotmold-left";
            block.MergeOrAddBehavior("wearandtear:IngotMoldPart", (JContainer)frame.DeepClone());

            frame["Code"] = "wearandtear:ingotmold-right";
            block.MergeOrAddBehavior("wearandtear:IngotMoldPart", (JContainer)frame);
        }

        public static void PatchHelve(Block block)
        {
            if (block is BlockHelveHammer)
            {
                block.EnsureBaseWearAndTear(true);
                block.MergeOrAddBehavior("wearandtear:HelveItemPart", (JContainer)JToken.FromObject(DefaultHelveItemPartProps));
            }
        }

        public static void PatchPulverizer(Block block)
        {
            if (block is BlockPulverizer)
            {
                block.EnsureBaseWearAndTear(true);
                block.MergeOrAddBehavior("wearandtear:PulverizerItemPart", (JContainer)JToken.FromObject(DefaultPulverizerItemPartProps));

                //TODO other parts
            }
        }
    }
}