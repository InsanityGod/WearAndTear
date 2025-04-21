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
        public static WearAndTearPartProps DefaultHelveItemPartProps => new()
        {
            Code = "wearandtear:helveitem",
            Decay = Array.Empty<WearAndTearDecayProps>()
        };

        public static WearAndTearPartProps DefaultPulverizerItemPartProps => new()
        {
            Code = "wearandtear:pulverizeritem",
            Decay = Array.Empty<WearAndTearDecayProps>()
        };

        public static void PatchClutch(Block block)
        {
            if (WearAndTearServerConfig.Instance.SpecialParts.Clutch == null || block is not BlockClutch) return;
            block.EnsureBaseWearAndTear(true);
            //TODO special part
        }

        public static void PatchWindmill(Block block)
        {
            if (WearAndTearServerConfig.Instance.SpecialParts.WindmillSails == null) return;

            if (block is BlockWindmillRotor || block.GetType().Name == "BlockWindmillRotorEnhanced")
            {
                block.EnsureBaseWearAndTear(true);
                block.MergeOrAddBehavior("WearAndTearSail", (JContainer)JToken.FromObject(WearAndTearServerConfig.Instance.SpecialParts.WindmillSails));

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
            if (!WearAndTearServerConfig.Instance.SpecialParts.Molds || block is not BlockIngotMold) return;

            block.EnsureBaseWearAndTear(true);
            var frameProps = WearAndTearServerConfig.Instance.AutoPartRegistry.DefaultFrameProps.GetValueOrDefault(block.BlockMaterial);
            var frame = JToken.FromObject(frameProps);
            frame[nameof(WearAndTearPartProps.Decay)] = JToken.FromObject(Array.Empty<WearAndTearDecayProps>());
            ((JContainer)frame).Merge(JToken.FromObject(new WearAndTearDurabilityPartProps()));

            frame["Code"] = "wearandtear:ingotmold-left";
            block.MergeOrAddBehavior("WearAndTearIngotMold", (JContainer)frame.DeepClone());

            frame["Code"] = "wearandtear:ingotmold-right";
            block.MergeOrAddBehavior("WearAndTearIngotMold", (JContainer)frame);
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