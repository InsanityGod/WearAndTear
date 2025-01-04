using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Behaviours;
using WearAndTear.Behaviours.Parts;
using WearAndTear.Behaviours.Parts.Item;
using WearAndTear.Behaviours.Parts.Protective;
using WearAndTear.Config;
using WearAndTear.DecayEngines;
using WearAndTear.DynamicPatches;
using WearAndTear.HarmonyPatches;
using WearAndTear.HarmonyPatches.indappledgroves;
using WearAndTear.Interfaces;
using WearAndTear.Rendering;

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

        public override void StartPre(ICoreAPI api)
        {
            AutoPartRegistry.Api = api;
            LoadConfig(api);
        }

        public override void Start(ICoreAPI api)
        {
            if (!Harmony.HasAnyPatches(Mod.Info.ModID))
            {
                harmony = new Harmony(Mod.Info.ModID);
                harmony.PatchAllUncategorized();

                //TODO: revamp to use patch category
                var indappledgroves = api.ModLoader.GetMod("indappledgroves");
                if(indappledgroves != null)
                {
                    try
                    {
                        var sys = indappledgroves.Systems.First();
                        var sawbuck = AccessTools.GetTypesFromAssembly(sys.GetType().Assembly).First(type => type.Name == "IDGBESawBuck");
                        var tool = AccessTools.GetTypesFromAssembly(sys.GetType().Assembly).First(type => type.Name == "BehaviorIDGTool");

                        harmony.Patch(sawbuck.GetMethod("SpawnOutput"), postfix: new HarmonyMethod(typeof(CreateSawDust).GetMethod(nameof(CreateSawDust.SpawnSawDustSawBuck))));
                        harmony.Patch(tool.GetMethod("SpawnOutput"), postfix: new HarmonyMethod(typeof(CreateSawDust).GetMethod(nameof(CreateSawDust.SpawnSawDustGroundRecipe))));
                    }
                    catch (Exception ex)
                    {
                        api.Logger.Error(ex);
                        api.Logger.Warning("Failed to do compatibility patches between WearAndTear and InDappledGroves");
                    }
                }

                //TODO: revamp to use patch category
                var millwright = api.ModLoader.GetMod("millwright");
                if (millwright != null)
                {
                    try
                    {
                        var sys = millwright.Systems.First();
                        var beh = AccessTools.GetTypesFromAssembly(sys.GetType().Assembly).First(type => type.Name == "BEBehaviorWindmillRotorEnhanced");

                        harmony.Patch(AccessTools.Method(beh, "Obstructed", new Type[] { typeof(int) }), postfix: new HarmonyMethod(typeof(FixObstructedItemDrop).GetMethod(nameof(FixObstructedItemDrop.Postfix))));
                        harmony.Patch(AccessTools.Method(beh, "OnBlockBroken", new Type[] { typeof(IPlayer) }), prefix: new HarmonyMethod(typeof(FixSailItemDrops).GetMethod(nameof(FixSailItemDrops.Prefix))));
                        harmony.Patch(AccessTools.Method(beh, "OnInteract", new Type[] { typeof(IPlayer) }), prefix: new HarmonyMethod(typeof(AllowForRollingUpSails).GetMethod(nameof(AllowForRollingUpSails.Prefix))));

                        harmony.Patch(AccessTools.Method(beh, "updateShape", new Type[] { typeof(IWorldAccessor) }), postfix: new HarmonyMethod(typeof(FixWindmillShape).GetMethod(nameof(FixWindmillShape.Postfix))));
                    }
                    catch (Exception ex)
                    {
                        api.Logger.Error(ex);
                        api.Logger.Warning("Failed to do compatibility patches between WearAndTear and Millwright");
                    }
                }

                if (api.ModLoader.IsModEnabled("immersiveorecrush"))
                {
                    harmony.PatchCategory("immersiveorecrush");
                }
            }

            MechNetworkRenderer.RendererByCode["wearandtear:windmillrotor"] = typeof(WindmillRenderer);
            RegisterBehaviours(api);
        }

        private static void RegisterBehaviours(ICoreAPI api)
        {
            api.RegisterBlockEntityBehaviorClass("WearAndTear", typeof(WearAndTearBehavior));
            api.RegisterBlockEntityBehaviorClass("WearAndTearPart", typeof(WearAndTearPartBehavior));
            api.RegisterBlockEntityBehaviorClass("WearAndTearProtectivePart", typeof(WearAndTearProtectivePartBehavior));
            api.RegisterBlockEntityBehaviorClass("WearAndTearOptionalProtectivePart", typeof(WearAndTearOptionalProtectivePartBehavior));
            api.RegisterBlockEntityBehaviorClass("WearAndTearSail", typeof(WearAndTearSailBehavior));
            api.RegisterBlockEntityBehaviorClass("WearAndTearHelveItem", typeof(WearAndTearHelveItemBehavior));
            api.RegisterBlockEntityBehaviorClass("WearAndTearPulverizerItem", typeof(WearAndTearPulverizerItemBehavior));

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
            foreach(var prop in typeof(SpecialPartConfig).GetProperties())
            {
                var obj = prop.GetValue(Config.SpecialParts);
                api.World.Config.SetBool($"WearAndTear_Feature_{prop.Name}", obj is bool turned_on ? turned_on : obj != null);
            }
        }

        public override void AssetsFinalize(ICoreAPI api)
        {
            if(api.Side != EnumAppSide.Server) return;

            foreach (var block in api.World.Blocks)
            {
                //Dynammically add BlockEntityBehaviors server side
                if(block?.Code == null) continue;
                BlockPatches.PatchWindmill(block);
                BlockPatches.PatchHelve(block);
                BlockPatches.PatchPulverizer(block);
                BlockPatches.PatchClutch(block);

                if (Config.AutoPartRegistry.Enabled)
                {
                    AutoPartRegistry.Register(block);
                }
            }
        }

        public override void Dispose()
        {
            MechNetworkRenderer.RendererByCode.Remove("wearandtear:windmillrotor");
            harmony?.UnpatchAll(Mod.Info.ModID);
            AutoPartRegistry.Api = null;
            Config = null;
        }
    }
}