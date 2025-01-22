using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;
using Vintagestory.GameContent.Mechanics;

namespace WearAndTear.Code.AutoRegistry.Compatibility
{
    public static class AxleInBlocks
    {
        public static void Register(Block block)
        {
            var woodenPart = WearAndTearModSystem.Config.AutoPartRegistry.DefaultFrameProps.GetValueOrDefault(EnumBlockMaterial.Wood);
            if(woodenPart == null) return;
            var props = new JsonObject(JToken.FromObject(woodenPart));
            props.Token["Name"] = "Encased Mechanism (Wood)";
            //props.Token["IsCritical"] = false;
            //TODO non destructive critical isntead (where the block just stops working instead)
            props.Token["RepairType"] = null; //It's encased so you can't propperly repair it
            props.Token["AvgLifeSpanInYears"] = props.Token["AvgLifeSpanInYears"].Value<float>() * WearAndTearModSystem.Config.Compatibility.EncasedPartLifeSpanMultiplier;
            block.BlockEntityBehaviors = block.BlockEntityBehaviors.Append(new BlockEntityBehaviorType
            {
                Name = "WearAndTearPart",
                properties = props
            });
        }
    }
}
