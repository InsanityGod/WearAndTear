using HarmonyLib;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Behaviours;

namespace WearAndTear.HarmonyPatches
{
    [HarmonyPatch(typeof(BEBehaviorWindmillRotor), nameof(BEBehaviorWindmillRotor.OnBlockBroken))]
    public static class FixSailItemDrops
    {
        public static void Prefix(BEBehaviorMPRotor __instance)
        {
            var wearAndTearBehaviour = __instance.Blockentity.GetBehavior<WearAndTearSailBlockEntityBehavior>();
            if (wearAndTearBehaviour == null) return;

            WearAndTearSailBlockEntityBehavior.DropSails(__instance.Api, wearAndTearBehaviour, __instance.Pos);
            wearAndTearBehaviour.SailLength = 0;
        }
    }
}