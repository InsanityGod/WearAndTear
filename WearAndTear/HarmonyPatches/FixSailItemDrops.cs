using HarmonyLib;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Behaviours.Parts;

namespace WearAndTear.HarmonyPatches
{
    [HarmonyPatch(typeof(BEBehaviorWindmillRotor), nameof(BEBehaviorWindmillRotor.OnBlockBroken))]
    public static class FixSailItemDrops
    {
        public static void Prefix(BEBehaviorMPRotor __instance) => __instance.Blockentity.GetBehavior<WearAndTearSailBehavior>()?.DropSails();
    }
}