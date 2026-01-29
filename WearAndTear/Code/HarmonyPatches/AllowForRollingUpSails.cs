using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using WearAndTear.Code.Behaviours.Parts;
using WearAndTear.Code.HarmonyPatches;

namespace WearAndTear.HarmonyPatches;

[HarmonyPatch]
public static class AllowForRollingUpSails
{
    [HarmonyPrefix]
    public static bool Prefix(BlockEntityBehavior __instance, IPlayer byPlayer, ref bool __result)
    {
        if (byPlayer.InventoryManager.ActiveHotbarSlot.Empty && byPlayer.Entity.Controls.ShiftKey)
        {
            var sail = __instance.Blockentity.GetBehavior<WindmillSailPart>();
            if (sail != null && sail.Durability >= 0.05)
            {
                if (sail.IsActive)
                {
                    if (__instance.Api is ICoreClientAPI clientApi) clientApi.TriggerIngameError(__instance, "wearandtear:failed-maintenance-active", Lang.Get("wearandtear:failed-maintenance-active"));
                    return true;
                }

                sail.AreSailsRolledUp = !sail.AreSailsRolledUp;
                __instance.Blockentity.MarkDirty();
                __result = true;
                return false;
            }
        }

        return true;
    }

    [HarmonyTargetMethods]
    public static IEnumerable<MethodBase> TargetMethods()
    {
        foreach(var type in Helpers.WindmillRotorBehaviorTypes())
        {
            var method = type.GetMethod("OnInteract", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (method != null)
            {
                yield return method;
            }
        }
    }
}