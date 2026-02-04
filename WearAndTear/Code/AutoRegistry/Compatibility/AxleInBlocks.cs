using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Vintagestory.API.Common;
using WearAndTear.Config.Server;

namespace WearAndTear.Code.AutoRegistry.Compatibility;

public static class AxleInBlocks
{
    public static void Register(Block block)
    {
        var woodenPart = AutoPartRegistryConfig.Instance.DefaultFrameProps.GetValueOrDefault(EnumBlockMaterial.Wood);
        if (woodenPart == null) return;

        //TODO make these encased blocks turn into a full block rubble pile

        var props = JToken.FromObject(woodenPart);
        props["Code"] = "wearandtear:mechanism-encased";
        props["RepairType"] = null; //It's encased so you can't propperly repair it
        props["AvgLifeSpanInYears"] = props["AvgLifeSpanInYears"].Value<float>() * CompatibilityConfig.Instance.EncasedPartLifeSpanMultiplier;
        block.MergeOrAddBehavior("wearandtear:Part", (JContainer)props);
    }
}