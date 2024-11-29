using HarmonyLib;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Interfaces;

namespace WearAndTear.HarmonyPatches
{
    [HarmonyPatch(typeof(BEBehaviorWindmillRotor), "AccelerationFactor", MethodType.Getter)]
    public static class IncludeWearAndTearEfficiencyInAccelerationFactor
    {
        public static void Postfix(BEBehaviorMPRotor __instance, ref double __result)
        {
            var wearAndTearBehaviour = __instance.Blockentity.GetBehavior<IWearAndTear>();
            if (wearAndTearBehaviour == null) return;
            __result *= wearAndTearBehaviour.AvgEfficiencyModifier;
        }
    }
}