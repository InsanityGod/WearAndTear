using HarmonyLib;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Interfaces;

namespace WearAndTear.HarmonyPatches
{
    [HarmonyPatch(typeof(BEBehaviorWindmillRotor))]
    public static class IncludeWearAndTearEfficiencyInTorqueFactorAndTargetSpeed
    {

        [HarmonyPostfix]
        [HarmonyPatch("TorqueFactor", MethodType.Getter)]
        public static void TorqueFactorPostfix(BEBehaviorMPRotor __instance, ref float __result)
        {
            var wearAndTearBehaviour = __instance.Blockentity.GetBehavior<IWearAndTear>();
            if (wearAndTearBehaviour == null) return;
            __result *= wearAndTearBehaviour.AvgEfficiencyModifier;
        }

        [HarmonyPostfix]
        [HarmonyPatch("TargetSpeed", MethodType.Getter)]
        public static void TargetSpeedPostfix(BEBehaviorMPRotor __instance, ref float __result)
        {
            var wearAndTearBehaviour = __instance.Blockentity.GetBehavior<IWearAndTear>();
            if (wearAndTearBehaviour == null) return;
            __result *= wearAndTearBehaviour.AvgEfficiencyModifier;
        }
    }
}