using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using WearAndTear.Config.Props.rubble;

namespace WearAndTear.Code.AutoRegistry.Compatibility
{
    public static class AxleInBlocks
    {
        public static void Register(Block block)
        {
            var woodenPart = WearAndTearModSystem.Config.AutoPartRegistry.DefaultFrameProps.GetValueOrDefault(EnumBlockMaterial.Wood);
            if (woodenPart == null) return;
            
            //TODO make these encased blocks turn into a full block rubble pile
            //block.Attributes ??= new JsonObject(JToken.Parse("{}"));
            //block.Attributes.Token[WearAndTearRubbleProps.Key] = JToken.FromObject(new WearAndTearRubbleProps
            //{
            //    DamageOnTouch = false,
            //    CollisionSelectionBoxes = Block.DefaultCollisionSelectionBoxes,
            //    Unstable = false,
            //    Shape = block.Shape.Base
            //});
            var props = JToken.FromObject(woodenPart);
            props["Code"] = "wearandtear:mechanism-encased";
            props["RepairType"] = null; //It's encased so you can't propperly repair it
            props["AvgLifeSpanInYears"] = props["AvgLifeSpanInYears"].Value<float>() * WearAndTearModSystem.Config.Compatibility.EncasedPartLifeSpanMultiplier;
            block.MergeOrAddBehavior("WearAndTearPart", (JContainer)props);
        }
    }
}