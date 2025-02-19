using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code.AutoRegistry;
using WearAndTear.Code.Behaviours;
using WearAndTear.Code.Behaviours.Parts;
using WearAndTear.Code.Behaviours.Parts.Item;
using WearAndTear.Code.Behaviours.Parts.Protective;
using WearAndTear.Code.Commands;
using WearAndTear.Code.DecayEngines;
using WearAndTear.Code.HarmonyPatches.AutoRegistry;
using WearAndTear.Code.Interfaces;
using WearAndTear.Code.Rendering;
using WearAndTear.Code.XLib;
using WearAndTear.Config;
using WearAndTear.DynamicPatches;
using WearAndTear.HarmonyPatches;

namespace WearAndTear.Code
{
    public class WearAndTearModSystem : ModSystem
    {
        private const string ConfigName = "WearAndTearConfig.json";

        private Harmony harmony;

        public static bool XlibEnabled { get; private set; }
        public static bool HelveAxeModLoaded { get; private set; }
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
            XlibEnabled = api.ModLoader.IsModEnabled("xlib");
            if(XlibEnabled) SkillsAndAbilities.RegisterSkills(api);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            SetDurabilityCommand.Register(api);
        }

        #region HarmonyWorkAround

        private static ICoreAPI apiCache;

        public static IEnumerable<Assembly> ModAssembliesForHarmonyScan => apiCache.ModLoader.Mods.Select(mod => mod.Systems.FirstOrDefault())
            .Where(modSystem => modSystem != null)
            .Select(modSystem => modSystem.GetType().Assembly);

        public static IEnumerable<Type> ModTypesForHarmonyScan => ModAssembliesForHarmonyScan.SelectMany(assembly =>
        {
            try
            {
                return assembly.GetTypes();
            }
            catch
            {
                try
                {
                    apiCache.Logger.Warning($"Could not get types from assembly '{assembly.FullName}', WearAndTear Harmony Patches might not have applied propperly for this mod");
                }
                catch { }
                return Enumerable.Empty<Type>();
            }
        });

        #endregion HarmonyWorkAround

        public void TryPatchCompatibility(ICoreAPI api, string modId)
        {
            var mod = api.ModLoader.GetMod(modId);
            if (mod != null)
            {
                try
                {
                    harmony.PatchCategory(modId);
                }
                catch (Exception ex)
                {
                    api.Logger.Error(ex);
                    api.Logger.Warning($"Failed to do compatibility patches between WearAndTear and {mod.Info.Name}");
                }
            }
        }

        public override void Start(ICoreAPI api)
        {
            HelveAxeModLoaded = api.ModLoader.IsModEnabled("mechanicalwoodsplitter");
            apiCache = api;

            if (!Harmony.HasAnyPatches(Mod.Info.ModID))
            {
                harmony = new Harmony(Mod.Info.ModID);
                harmony.PatchAllUncategorized();

                TryPatchCompatibility(api, "indappledgroves");
                TryPatchCompatibility(api, "linearpower");

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

            apiCache = null;
            if(XlibEnabled) SkillsAndAbilities.RegisterAbilities(api);
        }

        private static void RegisterBehaviours(ICoreAPI api)
        {
            api.RegisterBlockEntityBehaviorClass("WearAndTear", typeof(WearAndTearBehavior));
            api.RegisterBlockEntityBehaviorClass("WearAndTearPart", typeof(WearAndTearPartBehavior));
            api.RegisterBlockEntityBehaviorClass("WearAndTearProtectivePart", typeof(WearAndTearProtectivePartBehavior));
            api.RegisterBlockEntityBehaviorClass("WearAndTearOptionalProtectivePart", typeof(WearAndTearOptionalProtectivePartBehavior));
            api.RegisterBlockEntityBehaviorClass("WearAndTearSail", typeof(WearAndTearSailBehavior));
            api.RegisterBlockEntityBehaviorClass("WearAndTearMold", typeof(WearAndTearMoldPartBehavior));
            api.RegisterBlockEntityBehaviorClass("WearAndTearIngotMold", typeof(WearAndTearIngotMoldPartBehavior));
            api.RegisterBlockEntityBehaviorClass("WearAndTearGenericItemDisplay", typeof(WearAndTearGenericItemDisplayBehavior));
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
            foreach (var prop in typeof(SpecialPartConfig).GetProperties())
            {
                var obj = prop.GetValue(Config.SpecialParts);
                api.World.Config.SetBool($"WearAndTear_Feature_{prop.Name}", obj is bool turned_on ? turned_on : obj != null);
            }
        }

        public override void AssetsFinalize(ICoreAPI api)
        {
            if (harmony != null)
            {
                foreach (var block in api.World.Blocks.Where(block => block is BlockToolMold && block.BlockMaterial == EnumBlockMaterial.Ceramic))
                {
                    var entityClass = string.IsNullOrEmpty(block.EntityClass) ? null : api.ClassRegistry.GetBlockEntity(block.EntityClass);
                    if (entityClass == null) continue;

                    var getBlockInfoMethod = entityClass.GetMethod(nameof(BlockEntity.GetBlockInfo));
                    if (getBlockInfoMethod != null && getBlockInfoMethod.DeclaringType != typeof(BlockEntity))
                    {
                        AutoRegistryPatches.EnsureBaseMethodCall(api, harmony, getBlockInfoMethod);
                    }
                    if (block.GetType() != typeof(Block))
                    {
                        AutoRegistryPatches.EnsureBlockDropsConnected(api, harmony, block);
                    }
                }

                var ingotMoldMethod = AccessTools.Method(typeof(BlockEntityIngotMold), nameof(BlockEntityIngotMold.GetBlockInfo));
                if(ingotMoldMethod != null) AutoRegistryPatches.EnsureBaseMethodCall(api, harmony, ingotMoldMethod);
            }

            if (api.Side != EnumAppSide.Server) return;

            foreach (var block in api.World.Blocks)
            {
                //Dynammically add BlockEntityBehaviors server side
                if (block?.Code == null) continue;
                BlockPatches.PatchWindmill(block);
                BlockPatches.PatchIngotMold(block);
                BlockPatches.PatchHelve(block);
                BlockPatches.PatchPulverizer(block);
                BlockPatches.PatchClutch(block);

                if (Config.AutoPartRegistry.Enabled)
                {
                    AutoPartRegistry.Register(api, block, harmony);
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