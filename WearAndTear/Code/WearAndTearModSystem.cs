using HarmonyLib;
using InsanityLib.Util;
using InsanityLib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code.AutoRegistry;
using WearAndTear.Code.DecayEngines;
using WearAndTear.Code.Extensions;
using WearAndTear.Code.HarmonyPatches.AutoRegistry;
using WearAndTear.Code.Interfaces;
using WearAndTear.Code.Rendering;
using WearAndTear.Code.XLib;
using WearAndTear.Config.Server;
using WearAndTear.DynamicPatches;

namespace WearAndTear.Code;

public partial class WearAndTearModSystem : ModSystem
{
    public static bool XlibEnabled { get; private set; }
    public static bool HelveAxeModLoaded { get; private set; }

    public Dictionary<string, IDecayEngine> DecayEngines { get; } = new Dictionary<string, IDecayEngine>
    {
        { "wind", new WindDecayEngine()},
        { "humidity", new HumidityDecayEngine()},
        { "time", new TimeDecayEngine()},
    };

    public override void StartPre(ICoreAPI api)
    {
        AutoSetup(api);
        AutoPartRegistry.Api = api;

        XlibEnabled = api.ModLoader.IsModEnabled("xlib");
        if (XlibEnabled) SkillsAndAbilities.RegisterSkills(api);
    }

    public override void Start(ICoreAPI api)
    {
        HelveAxeModLoaded = api.ModLoader.IsModEnabled("mechanicalwoodsplitter");
        MechNetworkRenderer.RendererByCode["wearandtear:windmillrotor"] = typeof(WindmillRenderer);

        if (XlibEnabled) SkillsAndAbilities.RegisterAbilities(api);
    }

    public override void AssetsLoaded(ICoreAPI api)
    {
        base.AssetsLoaded(api);

        if(XlibEnabled) SkillsAndAbilities.FixAbilityLangStrings(api);
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
            if (ingotMoldMethod != null) AutoRegistryPatches.EnsureBaseMethodCall(api, harmony, ingotMoldMethod);
        }

        FinalizeScrap(api);
        if (api.Side != EnumAppSide.Server) return;
        
        api.Logger.VerboseDebug("[WearAndTear] Starting part registration");
        foreach (var block in api.World.Blocks)
        {
            //Dynammically add BlockEntityBehaviors server side
            if (block.IsMissing || block?.Code is null) continue;
            BlockPatches.PatchWindmill(block);
            BlockPatches.PatchIngotMold(block);
            BlockPatches.PatchHelve(block);
            BlockPatches.PatchPulverizer(block);
            BlockPatches.PatchClutch(block);

            if (AutoPartRegistryConfig.Instance.Enabled)
            {
                try
                {
                    AutoPartRegistry.Register(block);
                }
                catch (Exception e)
                {
                    api.Logger.Error("[WearAndTear] Failed AutoRegistry for block {0}: {1}", block.Code, e);
                }
            }
        }
        api.Logger.VerboseDebug("[WearAndTear] Finished part registration");

        AutoPartRegistry.ClearAnalyzerCache();
    }

    public static void FinalizeScrap(ICoreAPI api)
    {
        foreach(var item in api.World.Items)
        {
            if(item.IsMissing || item.Code?.Domain != "wearandtear") continue;
            var firstCodePart = item.FirstCodePartAsSpan();

            if (firstCodePart.SequenceEqual("woodscrap"))
            {
                var woodVariant = item.Variant["wood"];
                var plank = api.World.GetItem($"game:plank-{woodVariant}");
                plank ??= api.World.GetItem($"wildcrafttree:plank-{woodVariant}");
                if (plank == null)
                {
                    api.Logger.Error("[WearAndTear] No plank for {0} variant and this is required for wearandtear:woodscrap to function propperly", woodVariant);
                    continue;
                }

                item.MaterialDensity = plank.MaterialDensity;
                item.CombustibleProps = plank.CombustibleProps?.Clone();
            }
            else if (firstCodePart.SequenceEqual("metalscrap"))
            {
                var metalVariant = item.Variant["metal"];
                var ingot = api.World.GetItem($"game:ingot-{metalVariant}");
                if (ingot == null)
                {
                    api.Logger.Error("[WearAndTear] No ingot for {0} variant and this is required for wearandtear:metalscrap to function propperly", metalVariant);
                    continue;
                }

                item.MaterialDensity = ingot.MaterialDensity;
                item.CombustibleProps = ingot.CombustibleProps?.Clone();
                if (item.CombustibleProps == null) continue;
                item.CombustibleProps.SmeltedRatio = 4;
            }
        }
    }

    public static bool IsRoughEstimateEnabled(ICoreAPI api, IPlayer player)
    {
        if (CompatibilityConfig.Instance.RoughDurabilityEstimate.IsFullfilled())
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
        AutoDispose();
        MechNetworkRenderer.RendererByCode.Remove("wearandtear:windmillrotor");
    }
}