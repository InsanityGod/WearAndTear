using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code.XLib.Containers;
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

            var butterfingers = new Ability(
                "butterfingers",
                Lang.GetUnformatted("wearandtear:ability-butterfingers"),
                Lang.GetUnformatted("wearandtear:abilitydesc-butterfingers"),
                1, 3, new int[] { 10, 20, 20, 30, 30, 40 }
            );
            mechanics.AddAbility(butterfingers);

            if (!WearAndTearModSystem.Config.AllowForInfiniteMaintenance)
            {
                var limitBreaker = new Ability(
                    "limitbreaker",
                    Lang.GetUnformatted("wearandtear:ability-limitbreaker"),
                    Lang.GetUnformatted("wearandtear:abilitydesc-limitbreaker"),
                    1, 4, new int[] { 25, 50, 75, 100 }
                );
                mechanics.AddAbility(limitBreaker);
            }

            var expertAssembler = new Ability(
                "reinforcer",
                Lang.GetUnformatted("wearandtear:ability-reinforcer"),
                Lang.GetUnformatted("wearandtear:abilitydesc-reinforcer"),
                1, 5, new int[] { 10, 20, 30, 40, 50 }
            );
            mechanics.AddAbility(expertAssembler);

            var mechanicsSpecialisation = new Ability(
                "engineer",
                Lang.GetUnformatted("wearandtear:ability-engineer"),
                Lang.GetUnformatted("wearandtear:abilitydesc-engineer"),
                1, 1, new int[] { 40 }
            );

            mechanics.SpecialisationID = mechanics.AddAbility(mechanicsSpecialisation);


            var preciseMeasurements = new Ability(
                "precisemeasurements",
                Lang.GetUnformatted("wearandtear:ability-precise-measurements"),
                Lang.GetUnformatted("wearandtear:abilitydesc-precise-measurements")
            );
            preciseMeasurements.AddRequirement(new AbilityRequirement(mechanicsSpecialisation, 1));
            mechanics.AddAbility(preciseMeasurements);
            //TODO maybe things limited to specialization?

            //TODO (once we have the rubble mechanism setup, so we can refund) Temporal Tinkerer //I think we'll put this temporal gear right here
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
        
        public static void ApplyButterFingerBonus(PartBonuses partBonuses, ICoreAPI api, IPlayer player)
        {
            var xleveling = api.ModLoader.GetModSystem<XLeveling>();
            var ability = xleveling.IXLevelingAPI.GetPlayerSkillSet(player)?.FindSkill("mechanics")?.FindAbility("butterfingers");
            if(ability == null) return;
            
            partBonuses.ProtectionModifier *= 1 + (ability.Value(0) * 0.01f);
            partBonuses.DecayModifier *= 1 + (ability.Value(1) * 0.01f);
        }
        
        public static void ApplyReinforcerBonus(PartBonuses partBonuses, ICoreAPI api, IPlayer player)
        {
            var xleveling = api.ModLoader.GetModSystem<XLeveling>();
            var ability = xleveling.IXLevelingAPI.GetPlayerSkillSet(player)?.FindSkill("mechanics")?.FindAbility("reinforcer");
            if(ability == null) return;
            
            partBonuses.ProtectionModifier *= 1 + (ability.Value(0) * 0.01f);
        }

        public static float ApplyLimitBreakerBonus(ICoreAPI api, IPlayer player, float maintenanceLimit)
        {
            var xleveling = api.ModLoader.GetModSystem<XLeveling>();
            var ability = xleveling.IXLevelingAPI.GetPlayerSkillSet(player)?.FindSkill("mechanics")?.FindAbility("limitbreaker");
            if(ability == null) return maintenanceLimit;
            return maintenanceLimit * (1 + (ability.Value(0) * 0.01f));
        }

        public static void GiveMechanicExp(ICoreAPI api, IPlayer player, float exp = 1)
        {
            if(api.Side == EnumAppSide.Client) return;
            var xleveling = api.ModLoader.GetModSystem<XLeveling>();
            var skill = xleveling.IXLevelingAPI.GetPlayerSkillSet(player).FindSkill("mechanics");
            if(skill == null) return;
            skill.AddExperience(exp);
        }

        public static bool HasPreciseMeasurementsSkill(ICoreAPI api, IPlayer player)
        {
            var xleveling = api.ModLoader.GetModSystem<XLeveling>();
            var ability = xleveling.IXLevelingAPI.GetPlayerSkillSet(player)?.FindSkill("mechanics")?.FindAbility("precisemeasurements");
            if(ability == null) return false;
            return ability.Tier > 0;
        }
    }
}
