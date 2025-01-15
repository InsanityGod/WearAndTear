using HarmonyLib;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code;
using WearAndTear.Code.Behaviours.Parts;

namespace WearAndTear.HarmonyPatches
{
    [HarmonyPatch(typeof(BEBehaviorWindmillRotor), "updateShape")]
    public static class FixWindmillShape
    {
        public static void Postfix(BEBehaviorMPRotor __instance)
        {
            if (!WearAndTearModSystem.Config.SpecialParts.WindmillRotoDecayedAppearance) return;
            var wearAndTearBehaviour = __instance.Blockentity.GetBehavior<WearAndTearSailBehavior>();
            if (wearAndTearBehaviour == null || wearAndTearBehaviour.SailLength == 0) return;
            wearAndTearBehaviour.UpdateShape(__instance);
        }
    }
}