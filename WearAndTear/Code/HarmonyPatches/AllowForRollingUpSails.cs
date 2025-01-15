using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code.Behaviours.Parts;

namespace WearAndTear.HarmonyPatches
{
    [HarmonyPatch(typeof(BEBehaviorWindmillRotor), "OnInteract")]
    public static class AllowForRollingUpSails
    {
        public static bool Prefix(BlockEntityBehavior __instance, IPlayer byPlayer, ref bool __result)
        {
            if (byPlayer.InventoryManager.ActiveHotbarSlot.Empty && byPlayer.Entity.Controls.ShiftKey)
            {
                var sail = __instance.Blockentity.GetBehavior<WearAndTearSailBehavior>();
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
    }
}