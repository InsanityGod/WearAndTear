using HarmonyLib;
using System;
using Vintagestory.API.Common;
using WearAndTear.Behaviours;
using WearAndTear.Config;

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
            }

            LoadConfig(api);
            RegisterBehaviours(api);
        }

        private static void RegisterBehaviours(ICoreAPI api)
        {
            api.RegisterBlockEntityBehaviorClass("WearAndTear", typeof(WearAndTearBlockEntityBehavior));
            api.RegisterBlockEntityBehaviorClass("WearAndTearSail", typeof(WearAndTearSailBlockEntityBehavior));
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