using Vintagestory.API.Common;
using Vintagestory.API.Config;
using XLib.XLeveling;

namespace WearAndTear.Code.XLib;

public static class SkillsAndAbilities
{
    public static void RegisterSkills(ICoreAPI api)
    {
        XLeveling leveling = api.ModLoader.GetModSystem<XLeveling>();

        var mechanics = new Skill(
            "wearandtear:mechanics",
            Lang.GetUnformatted("wearandtear:skill-mechanics"),
            "Mechanics"
        );

        leveling.RegisterSkill(mechanics);
    }
}