using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using WearAndTear.Code.XLib.Containers;
using WearAndTear.Config.Server;
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

            if (metalworking != null)
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

            var mechanicsSpecialisation = new Ability(
                "engineer",
                Lang.GetUnformatted("wearandtear:ability-engineer"),
                Lang.GetUnformatted("wearandtear:abilitydesc-engineer"),
                1, 1, new int[] { 40 }
            );

            mechanics.SpecialisationID = mechanics.AddAbility(mechanicsSpecialisation);
            var specialiationRequirement = new AbilityRequirement(mechanicsSpecialisation, 1);

            var preciseMeasurements = new Ability(
                "precisemeasurements",
                Lang.GetUnformatted("wearandtear:ability-precise-measurements"),
                Lang.GetUnformatted("wearandtear:abilitydesc-precise-measurements")
            );
            preciseMeasurements.AddRequirement(specialiationRequirement);
            mechanics.AddAbility(preciseMeasurements);

            var handyMan = new Ability(
                "handyman",
                Lang.GetUnformatted("wearandtear:ability-handyman"),
                Lang.GetUnformatted("wearandtear:abilitydesc-handyman"),
                1, 3, new int[] { 15, 25, 30 }
            );
            mechanics.AddAbility(handyMan);

            var butterfingers = new Ability(
                "butterfingers",
                Lang.GetUnformatted("wearandtear:ability-butterfingers"),
                Lang.GetUnformatted("wearandtear:abilitydesc-butterfingers"),
                1, 3, new int[] { 10, 20, 20, 30, 30, 40 }
            );
            mechanics.AddAbility(butterfingers);

            if (!WearAndTearServerConfig.Instance.AllowForInfiniteMaintenance)
            {
                var limitBreaker = new Ability(
                    "limitbreaker",
                    Lang.GetUnformatted("wearandtear:ability-limitbreaker"),
                    Lang.GetUnformatted("wearandtear:abilitydesc-limitbreaker"),
                    1, 4, new int[] { 25, 50, 75, 100 }
                );
                limitBreaker.AddRequirement(specialiationRequirement);
                mechanics.AddAbility(limitBreaker);
            }

            var expertAssembler = new Ability(
                "reinforcer",
                Lang.GetUnformatted("wearandtear:ability-reinforcer"),
                Lang.GetUnformatted("wearandtear:abilitydesc-reinforcer"),
                1, 5, new int[] { 10, 20, 30, 40, 50 }
            );
            mechanics.AddAbility(expertAssembler);

            var strongFeet = new Ability(
                "strongfeet",
                Lang.GetUnformatted("wearandtear:ability-strong-feet"),
                Lang.GetUnformatted("wearandtear:abilitydesc-strong-feet"),
                1, 3, new int[] { 25, 50, 100 }
            );
            mechanics.AddAbility(strongFeet);

            var scrapper = new Ability(
                "scrapper",
                Lang.GetUnformatted("wearandtear:ability-scrapper"),
                Lang.GetUnformatted("wearandtear:abilitydesc-scrapper"),
                1, 3, new int[] { 10, 20, 25 }
            );
            mechanics.AddAbility(scrapper);

            //TODO maybe more things limited to specialization?
            //TODO Temporal Tinkerer
        }

        public static void FixAbilityLangStrings(ICoreAPI api)
        {
            XLeveling leveling = api.ModLoader.GetModSystem<XLeveling>();
            var mechanics = leveling.GetSkill("mechanics");

            mechanics.Group = Lang.GetUnformatted(mechanics.Group);
            mechanics.DisplayName = Lang.GetUnformatted(mechanics.DisplayName);
            foreach(var ability in mechanics.Abilities)
            {
                ability.DisplayName = Lang.GetUnformatted(ability.DisplayName);
                ability.Description = Lang.GetUnformatted(ability.Description);
            }
        }


        public static float ApplyHandyManBonus(ICoreAPI api, IPlayer player, float repairStrength)
        {
            var xleveling = api.ModLoader.GetModSystem<XLeveling>();
            var ability = xleveling.IXLevelingAPI.GetPlayerSkillSet(player)?.FindSkill("mechanics")?.FindAbility("handyman");
            if (ability == null) return repairStrength;
            return repairStrength * (1 + (ability.Value(0) * 0.01f));
        }

        public static float ApplyMoldDurabilityCostModifier(ICoreAPI api, IPlayer player, float durabilityCost)
        {
            var xleveling = api.ModLoader.GetModSystem<XLeveling>();
            var ability = xleveling.IXLevelingAPI.GetPlayerSkillSet(player)?.FindSkill("metalworking")?.FindAbility("carefulcaster");
            if (ability == null) return durabilityCost;
            return durabilityCost * (1 - (ability.Value(0) * 0.01f));
        }

        public static float ApplyStrongFeetBonus(ICoreAPI api, IPlayer player, float damage)
        {
            var xleveling = api.ModLoader.GetModSystem<XLeveling>();
            var ability = xleveling.IXLevelingAPI.GetPlayerSkillSet(player)?.FindSkill("mechanics")?.FindAbility("strongfeet");
            if (ability == null) return damage;
            return damage * (1 - (ability.Value(0) * 0.01f));
        }

        public static int ApplyScrapperBonus(ICoreAPI api, IPlayer player, int amount)
        {
            var xleveling = api.ModLoader.GetModSystem<XLeveling>();
            var ability = xleveling.IXLevelingAPI.GetPlayerSkillSet(player)?.FindSkill("mechanics")?.FindAbility("scrapper");
            if (ability == null) return amount;
            return GameMath.RoundRandom(api.World.Rand, amount * (1 + (ability.Value(0) * 0.01f)));
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
            if (ability == null) return;

            partBonuses.ProtectionModifier *= 1 + (ability.Value(0) * 0.01f);
            partBonuses.DecayModifier *= 1 + (ability.Value(1) * 0.01f);
        }

        public static void ApplyReinforcerBonus(PartBonuses partBonuses, ICoreAPI api, IPlayer player)
        {
            var xleveling = api.ModLoader.GetModSystem<XLeveling>();
            var ability = xleveling.IXLevelingAPI.GetPlayerSkillSet(player)?.FindSkill("mechanics")?.FindAbility("reinforcer");
            if (ability == null) return;

            partBonuses.ProtectionModifier *= 1 + (ability.Value(0) * 0.01f);
        }

        public static float ApplyLimitBreakerBonus(ICoreAPI api, IPlayer player, float maintenanceLimit)
        {
            var xleveling = api.ModLoader.GetModSystem<XLeveling>();
            var ability = xleveling.IXLevelingAPI.GetPlayerSkillSet(player)?.FindSkill("mechanics")?.FindAbility("limitbreaker");
            if (ability == null) return maintenanceLimit;
            return maintenanceLimit * (1 + (ability.Value(0) * 0.01f));
        }

        public static void GiveMechanicExp(ICoreAPI api, IPlayer player, float exp = 1)
        {
            if (api.Side == EnumAppSide.Client) return;
            var xleveling = api.ModLoader.GetModSystem<XLeveling>();
            var skill = xleveling.IXLevelingAPI.GetPlayerSkillSet(player).FindSkill("mechanics");
            if (skill == null) return;
            skill.AddExperience(exp);
        }

        public static bool HasPreciseMeasurementsSkill(ICoreAPI api, IPlayer player)
        {
            var xleveling = api.ModLoader.GetModSystem<XLeveling>();
            var ability = xleveling.IXLevelingAPI.GetPlayerSkillSet(player)?.FindSkill("mechanics")?.FindAbility("precisemeasurements");
            if (ability == null) return false;
            return ability.Tier > 0;
        }
    }
}