using HarmonyLib;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code.Behaviours.Parts;

namespace WearAndTear.HarmonyPatches
{
    [HarmonyPatch(typeof(BEBehaviorWindmillRotor), nameof(BEBehaviorWindmillRotor.OnBlockBroken))]
    public static class FixSailItemDrops
    {
        public static void Prefix(BEBehaviorMPRotor __instance)
        {
            var beh = __instance.Blockentity.GetBehavior<WearAndTearSailBehavior>();
            if (beh == null) return;
            beh.SailLength = 0;
            beh.Durability = 1;
        }
    }
}