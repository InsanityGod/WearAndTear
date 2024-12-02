using HarmonyLib;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Behaviours.Parts;

namespace WearAndTear.HarmonyPatches
{
    //TODO register on millwright stuff once the shapes have been made
    [HarmonyPatch(typeof(BEBehaviorWindmillRotor), "updateShape")]
    public static class DisableOnNoSailsAndAddTornShapes
    {
        public static void Postfix(BEBehaviorMPRotor __instance)
        {
            var wearAndTearBehaviour = __instance.Blockentity.GetBehavior<WearAndTearSailBehavior>();
            if (wearAndTearBehaviour == null || wearAndTearBehaviour.SailLength == 0) return;
            wearAndTearBehaviour.UpdateShape(__instance, "torn");
        }
    }
}