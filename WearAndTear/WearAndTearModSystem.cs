﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using WearAndTear.Behaviours;
using WearAndTear.Behaviours.Parts;
using WearAndTear.Behaviours.Parts.Protective;
using WearAndTear.Config;
using WearAndTear.DecayEngines;
using WearAndTear.DynamicPatches;
using WearAndTear.HarmonyPatches;
using WearAndTear.Interfaces;

namespace WearAndTear
{
    public class WearAndTearModSystem : ModSystem
    {
        private const string ConfigName = "WearAndTearConfig.json";

        private Harmony harmony;

        public static ModConfig Config { get; private set; }

        public Dictionary<string, IDecayEngine> DecayEngines { get; } = new Dictionary<string, IDecayEngine>
        {
            { "wind", new WindDecayEngine()},
            { "humidity", new HumidityDecayEngine()},
            { "time", new TimeDecayEngine()},
        };

        public override void StartPre(ICoreAPI api) => LoadConfig(api);

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
                            harmony.Patch(AccessTools.Method(beh, "OnInteract", new Type[] { typeof(IPlayer) }), prefix: new HarmonyMethod(typeof(AllowForRollingUpSails).GetMethod(nameof(AllowForRollingUpSails.Prefix))));
                        }
                    }
                    catch (Exception ex)
                    {
                        api.Logger.Error(ex);
                        api.Logger.Warning("Failed to do compatibility patches between WearAndTear and Millwright");
                    }
                }
            }
            RegisterBehaviours(api);
        }

        private static void RegisterBehaviours(ICoreAPI api)
        {
            api.RegisterBlockEntityBehaviorClass("WearAndTear", typeof(WearAndTearBehavior));
            api.RegisterBlockEntityBehaviorClass("WearAndTearPart", typeof(WearAndTearPartBehavior));
            api.RegisterBlockEntityBehaviorClass("WearAndTearProtectivePart", typeof(WearAndTearProtectivePartBehavior));
            api.RegisterBlockEntityBehaviorClass("WearAndTearSail", typeof(WearAndTearSailBehavior));
            api.RegisterBlockEntityBehaviorClass("WearAndTearHelveItem", typeof(WearAndTearHelveItemBehavior));

            api.RegisterCollectibleBehaviorClass("WearAndTearRepairItem", typeof(WearAndTearRepairItemBehavior));
        }

        private static void LoadConfig(ICoreAPI api)
        {
            try
            {
                Config = api.LoadModConfig<ModConfig>(ConfigName) ?? new();
                api.StoreModConfig(Config, ConfigName);
            }
            catch (Exception ex)
            {
                api.Logger.Error(ex);
                api.Logger.Warning("Failed to load config, using default values instead");
                Config = new();
            }
            LoadFeatureFlags(api);
        }

        public static void LoadFeatureFlags(ICoreAPI api)
        {
            foreach(var prop in typeof(FeatureConfig).GetProperties())
            {
                var obj = prop.GetValue(Config.Features);
                if(obj is bool turned_on) api.World.Config.SetBool($"WearAndTear_Feature_{prop.Name}", turned_on);
            }
        }

        public override void AssetsFinalize(ICoreAPI api)
        {
            if(api.Side != EnumAppSide.Server) return;

            foreach (var block in api.World.Blocks)
            {
                if(block?.Code == null) continue;
                BlockPatches.PatchGenericWood(block);
                if(Config.Features.WindmillRotor) BlockPatches.PatchWindmill(block);
                BlockPatches.PatchHelve(block);
            }
        }

        public override void Dispose()
        {
            harmony?.UnpatchAll(Mod.Info.ModID);
            Config = null;
        }
    }
}