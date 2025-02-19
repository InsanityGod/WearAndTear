using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using XLib.XLeveling;

namespace WearAndTear.Code.XLib
{
    public static class SkillsAndAbilities
    {
        public static void RegisterSkills(ICoreAPI api)
        {
            XLeveling leveling = api.ModLoader.GetModSystem<XLeveling>();

            var mechanics = new Skill(
                "mechanics",
                Lang.GetUnformatted("wearandtear:skill-mechanics"),
                "Mechanics"
            );

            leveling.RegisterSkill(mechanics);
        }

        public static void RegisterAbilities(ICoreAPI api)
        {
            XLeveling leveling = api.ModLoader.GetModSystem<XLeveling>();

            var metalworking = leveling.GetSkill("metalworking");

            if(metalworking != null)
            {
                var carefulcaster = new Ability(
                    "carefulcaster",
                    "wearandtear:ability-careful-caster",
                    "wearandtear:abilitydesc-careful-caster",
                    1, 3, new int[] { 10, 20, 30 }
                );
                metalworking.AddAbility(carefulcaster);
                
                var expertCaster = new Ability(
                    "expertcaster",
                    "wearandtear:ability-expert-caster",
                    "wearandtear:abilitydesc-expert-caster"
                );
                expertCaster.AddRequirement(new SkillRequirement(metalworking, 20));
                expertCaster.AddRequirement(new AbilityRequirement(carefulcaster, 3));
                metalworking.AddAbility(expertCaster);
            }

            var mechanics = leveling.GetSkill("mechanics");
            
            var handyMan = new Ability(
                "handyman",
                Lang.GetUnformatted("wearandtear:ability-handyman"),
                Lang.GetUnformatted("wearandtear:abilitydesc-handyman"),
                1, 3, new int[] { 15, 25, 30}
            );
            mechanics.AddAbility(handyMan);
        }

        public static float ApplyHandyManBonus(ICoreAPI api, IPlayer player, float repairStrength)
        {
            var xleveling = api.ModLoader.GetModSystem<XLeveling>();
            Console.WriteLine($"original repair strength: {repairStrength}");
            var ability = xleveling.IXLevelingAPI.GetPlayerSkillSet(player)?.FindSkill("mechanics")?.FindAbility("handyman");
            if(ability == null) return repairStrength;
            return repairStrength * (1 + (ability.Value(0) * 0.01f));
        }

        public static float ApplyMoldDurabilityCostModifier(ICoreAPI api, IPlayer player, float durabilityCost)
        {
            var xleveling = api.ModLoader.GetModSystem<XLeveling>();
            var ability = xleveling.IXLevelingAPI.GetPlayerSkillSet(player)?.FindSkill("metalworking")?.FindAbility("carefulcaster");
            if(ability == null) return durabilityCost;
            return durabilityCost * (1 - (ability.Value(0) * 0.01f));
        }

        public static bool IsExpertCaster(ICoreAPI api, IPlayer player)
        {
            var xleveling = api.ModLoader.GetModSystem<XLeveling>();
            var ability = xleveling.IXLevelingAPI.GetPlayerSkillSet(player)?.FindSkill("metalworking")?.FindAbility("expertcaster");
            return ability != null && ability.Tier > 0;
        }

        public static void GiveMechanicExp(ICoreAPI api, IPlayer player, float exp = 1)
        {
            if(api.Side == EnumAppSide.Client) return;
            var xleveling = api.ModLoader.GetModSystem<XLeveling>();
            var skill = xleveling.IXLevelingAPI.GetPlayerSkillSet(player).FindSkill("mechanics");
            if(skill == null) return;
            skill.AddExperience(exp);
        }
    }
}
