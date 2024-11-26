using HarmonyLib;
using System;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using WearAndTear.Behaviours;
using WearAndTear.Config;
using WearAndTear.HarmonyPatches;

namespace WearAndTear
{
    public class WearAndTearModSystem : ModSystem
    {
        private const string ConfigName = "WearAndTearConfig.json";

        private Harmony harmony;

        public static ModConfig Config { get; private set; }

        public override void Start(ICoreAPI api)
        {
            if (!Harmony.HasAnyPatches(Mod.Info.ModID))
            {
                harmony = new Harmony(Mod.Info.ModID);
                harmony.PatchAll();

                var millwright = api.ModLoader.GetMod("millwright");
                if (millwright != null)
                {
                    try
                    {
                        var sys = millwright.Systems.First();
                        var beh = AccessTools.GetTypesFromAssembly(sys.GetType().Assembly).First(type => type.Name == "BEBehaviorWindmillRotorEnhanced");
                        if (beh != null)
                        {
                            harmony.Patch(AccessTools.Method(beh, "Obstructed", new Type[] { typeof(int) }), postfix: new HarmonyMethod(typeof(FixObstructedItemDrop).GetMethod(nameof(FixObstructedItemDrop.Postfix))));
                            harmony.Patch(AccessTools.Method(beh, "OnBlockBroken", new Type[] { typeof(IPlayer) }), prefix: new HarmonyMethod(typeof(FixSailItemDrops).GetMethod(nameof(FixSailItemDrops.Prefix))));
                            harmony.Patch(AccessTools.Method(beh, "ToTreeAttributes", new Type[] { typeof(ITreeAttribute) }), postfix: new HarmonyMethod(typeof(SetWearAndTearEnabledForWindmillRotor).GetMethod(nameof(SetWearAndTearEnabledForWindmillRotor.Postfix))));
                            harmony.Patch(AccessTools.PropertyGetter(beh, "TorqueFactor"), postfix: new HarmonyMethod(typeof(IncludeWearAndTearEfficiencyInTorqueFactorAndTargetSpeed).GetMethod(nameof(IncludeWearAndTearEfficiencyInTorqueFactorAndTargetSpeed.Postfix))));
                            harmony.Patch(AccessTools.PropertyGetter(beh, "TargetSpeed"), postfix: new HarmonyMethod(typeof(IncludeWearAndTearEfficiencyInTorqueFactorAndTargetSpeed).GetMethod(nameof(IncludeWearAndTearEfficiencyInTorqueFactorAndTargetSpeed.Postfix))));
                            harmony.Patch(AccessTools.PropertyGetter(beh, "AccelerationFactor"), postfix: new HarmonyMethod(typeof(IncludeWearAndTearEfficiencyInAccelerationFactor).GetMethod(nameof(IncludeWearAndTearEfficiencyInAccelerationFactor.Postfix))));
                        }
                    }
                    catch (Exception ex)
                    {
                        api.Logger.Error(ex);
                        api.Logger.Warning("Failed to do compatibility patches between WearAndTear and Millwright");
                    }
                }
            }

            LoadConfig(api);
            RegisterBehaviours(api);
        }

        private static void RegisterBehaviours(ICoreAPI api)
        {
            api.RegisterBlockEntityBehaviorClass("WearAndTear", typeof(WearAndTearBlockEntityBehavior));
            api.RegisterBlockEntityBehaviorClass("WearAndTearSail", typeof(WearAndTearSailBlockEntityBehavior));
            api.RegisterBlockEntityBehaviorClass("WearAndTearHelveBase", typeof(WearAndTearHelveHammerBlockEntityBehavior));
            api.RegisterCollectibleBehaviorClass("WearAndTearRepairTool", typeof(WearAndTearRepairToolCollectibleBehavior));
        }

        private static void LoadConfig(ICoreAPI api)
        {
            try
            {
                Config ??= api.LoadModConfig<ModConfig>(ConfigName);
                if (Config == null)
                {
                    Config = new();
                    api.StoreModConfig(Config, ConfigName);
                }
            }
            catch (Exception ex)
            {
                api.Logger.Error(ex);
                api.Logger.Warning("Failed to load config, using default values instead");
                Config = new();
            }
        }

        public override void Dispose()
        {
            harmony?.UnpatchAll(Mod.Info.ModID);
            Config = null;
        }
    }
}