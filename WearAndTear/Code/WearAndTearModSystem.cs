using HarmonyLib;
using InsanityLib.Attributes.Auto;
using InsanityLib.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code.AutoRegistry;
using WearAndTear.Code.Behaviours;
using WearAndTear.Code.Behaviours.Parts;
using WearAndTear.Code.Behaviours.Parts.Item;
using WearAndTear.Code.Behaviours.Parts.Protective;
using WearAndTear.Code.Behaviours.Rubble;
using WearAndTear.Code.Behaviours.Util;
using WearAndTear.Code.BlockEntities;
using WearAndTear.Code.Blocks;
using WearAndTear.Code.DecayEngines;
using WearAndTear.Code.Extensions;
using WearAndTear.Code.HarmonyPatches.AutoRegistry;
using WearAndTear.Code.Interfaces;
using WearAndTear.Code.Rendering;
using WearAndTear.Code.XLib;
using WearAndTear.Config;
using WearAndTear.DynamicPatches;
using InsanityLib.Attributes.Auto.Harmony;

[assembly:AutoPatcher("wearandtear")]
namespace WearAndTear.Code
{
    public class WearAndTearModSystem : ModSystem
    {
        private const string ConfigName = "WearAndTearConfig.json";

        public static bool XlibEnabled { get; private set; }
        public static bool HelveAxeModLoaded { get; private set; }

        //TODO: auto loading as well
        [AutoDefaultValue]
        public static ModConfig Config { get; private set; }

        public static bool TraitRequirements { get; private set; }

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
            
            //TODO move this over to world config once they actually support language strings
            if(api.Side == EnumAppSide.Server)
            {
                TraitRequirements = Config.TraitRequirements;
                api.World.Config.SetBool("wearandtear-traitrequirements", Config.TraitRequirements);
            }
            else
            {
                TraitRequirements = api.World.Config.GetBool("wearandtear-traitrequirements");
            }

            XlibEnabled = api.ModLoader.IsModEnabled("xlib");
            if(XlibEnabled) SkillsAndAbilities.RegisterSkills(api);
        }

        public override void Start(ICoreAPI api)
        {
            HelveAxeModLoaded = api.ModLoader.IsModEnabled("mechanicalwoodsplitter");
            MechNetworkRenderer.RendererByCode["wearandtear:windmillrotor"] = typeof(WindmillRenderer);
            RegisterBehaviours(api);
            RegisterOther(api);

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
            api.RegisterCollectibleBehaviorClass("WearAndTearMaterialName", typeof(WearAndTearMaterialName));

            api.RegisterBlockBehaviorClass("WearAndTearRubble", typeof(RubbleBehavior));
        }

        private static void RegisterOther(ICoreAPI api)
        {
            api.RegisterBlockClass("WearAndTear.RubbleBlock", typeof(RubbleBlock));
            api.RegisterBlockEntityClass("WearAndTear.RubbleBlockEntity", typeof(RubbleBlockEntity));
        }


        private static void LoadConfig(ICoreAPI api)
        {
            try
            {
                Config = api.LoadModConfig<ModConfig>(ConfigName) ?? new()
                {
                    ConfigCompatibilityVersion = ModConfig.LatestConfigCompatibilityVersion,
                };

                if(Config.ConfigCompatibilityVersion != ModConfig.LatestConfigCompatibilityVersion)
                {
                    //TODO test this and see about part remapping
                    api.Logger.Warning("[WearAndTear] Config version mismatch, attempting auto merge with default config");
                    var newConfig = (JContainer) JToken.FromObject(new ModConfig
                    {
                        ConfigCompatibilityVersion = ModConfig.LatestConfigCompatibilityVersion,
                    });
                    newConfig.Merge(JToken.FromObject(Config), new JsonMergeSettings
                    {
                        MergeArrayHandling = MergeArrayHandling.Merge,
                    });

                    Config = newConfig.ToObject<ModConfig>();
                    Config.ConfigCompatibilityVersion = ModConfig.LatestConfigCompatibilityVersion;
                }

                api.StoreModConfig(Config, ConfigName);
            }
            catch (Exception ex)
            {
                api.Logger.Error(ex);
                api.Logger.Warning("[WearAndTear] Failed to load config, using default values instead");
                Config = new()
                {
                    ConfigCompatibilityVersion = ModConfig.LatestConfigCompatibilityVersion,
                };
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
            if (!ReflectionUtil.SideLoaded(EnumAppSide.Server) || api.Side == EnumAppSide.Server)
            {
                //TODO find beter solution for this
                var harmony = new Harmony(Mod.Info.ModID);
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
            
            FinalizeScrap(api);
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
                    AutoPartRegistry.Register(block);
                }
            }

            AutoPartRegistry.ClearAnalyzerCache();
        }

        public void FinalizeScrap(ICoreAPI api)
        {
            foreach(var woodscrap in api.World.Items.Where(item => item.FirstCodePart() == "woodscrap"))
            {
                var woodVariant = woodscrap.Variant["wood"];
                var plank = api.World.GetItem($"game:plank-{woodVariant}");
                if(plank == null)
                {
                    api.Logger.Error("[WearAndTear] No plank for {0} variant and this is required for wearandtear:woodscrap to function propperly", woodVariant);
                    continue;
                }

                woodscrap.MaterialDensity = plank.MaterialDensity;
                //woodscrap.Textures = plank.Textures.ToDictionary(item => item.Key, item => item.Value);
                woodscrap.CombustibleProps = plank.CombustibleProps.Clone();
            }

            foreach(var metalscrap in api.World.Items.Where(item => item.FirstCodePart() == "metalscrap"))
            {
                var metalVariant = metalscrap.Variant["metal"];
                var ingot = api.World.GetItem($"game:ingot-{metalVariant}");
                if(ingot == null)
                {
                    api.Logger.Error("[WearAndTear] No ingot for {0} variant and this is required for wearandtear:metalscrap to function propperly", metalVariant);
                    continue;
                }

                metalscrap.MaterialDensity = ingot.MaterialDensity;
                //metalscrap.Textures = ingot.Textures.ToDictionary(item => item.Key, item => item.Value); //TODO copy correct texture only
                metalscrap.CombustibleProps = ingot.CombustibleProps?.Clone();
                if(metalscrap.CombustibleProps == null) continue;
                metalscrap.CombustibleProps.SmeltedRatio = 4;
            }
        }

        public static bool IsRoughEstimateEnabled(ICoreAPI api, IPlayer player)
        {
            if (Config.Compatibility.RoughDurabilityEstimate.IsFullfilled())
            {
                if (XlibEnabled)
                {
                    //Check xlib
                    return !SkillsAndAbilities.HasPreciseMeasurementsSkill(api, player);
                }
                else
                {
                    //Check traits
                    return !api.ModLoader.GetModSystem<CharacterSystem>().HasTrait(player, "wearandtear-precisemeasurements");
                }
            }

            return false;
        }

        public override void Dispose()
        {
            MechNetworkRenderer.RendererByCode.Remove("wearandtear:windmillrotor");
        }
    }
}