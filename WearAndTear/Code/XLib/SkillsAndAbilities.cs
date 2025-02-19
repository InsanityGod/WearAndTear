using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using XLib.XLeveling;

namespace WearAndTear.Code.XLib
{
    public static class SkillsAndAbilities
    {

        public static void RegisterSkills(ICoreAPI api)
        {
            XLeveling leveling = api.ModLoader.GetModSystem<XLeveling>();
        }

        public static void RegisterAbilities(ICoreAPI api)
        {
            XLeveling leveling = api.ModLoader.GetModSystem<XLeveling>();

            var metalworking = leveling.GetSkill("metalworking");

            if(metalworking != null)
            {
                var careFullCaster = new Ability(
                    "carefullcaster",
                    "wearandtear:ability-carefull-caster",
                    "wearandtear:abilitydesc-carefull-caster",
                    1, 3, new int[] { 10, 20, 30 }
                );
                metalworking.AddAbility(careFullCaster);
                
                var expertCaster = new Ability(
                    "expertcaster",
                    "wearandtear:ability-expert-caster",
                    "wearandtear:abilitydesc-expert-caster"
                );
                expertCaster.AddRequirement(new SkillRequirement(metalworking, 20));
                expertCaster.AddRequirement(new AbilityRequirement(careFullCaster, 3));
                metalworking.AddAbility(expertCaster);
            }

        }

        public static float ApplyMoldDurabilityCostModifier(ICoreAPI api, IPlayer player, float durabilityCost)
        {
            var xleveling = api.ModLoader.GetModSystem<XLeveling>();
            Console.WriteLine($"original durability cost: {durabilityCost}");
            var ability = xleveling.IXLevelingAPI.GetPlayerSkillSet(player)?.FindSkill("metalworking")?.FindAbility("carefullcaster");
            if(ability == null) return durabilityCost;
            return durabilityCost * (1 - (ability.Value(0) * 0.01f));
        }

        public static bool IsExpertCaster(ICoreAPI api, IPlayer player)
        {
            var xleveling = api.ModLoader.GetModSystem<XLeveling>();
            var ability = xleveling.IXLevelingAPI.GetPlayerSkillSet(player)?.FindSkill("metalworking")?.FindAbility("expertcaster");
            return ability != null && ability.Tier > 0;
        }
    }
}
