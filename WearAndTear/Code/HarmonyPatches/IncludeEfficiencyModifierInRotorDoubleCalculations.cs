using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code;
using WearAndTear.Code.Interfaces;

namespace WearAndTear.HarmonyPatches
{
    [HarmonyPatch]
    public static class IncludeEfficiencyModifierInRotorDoubleCalculations
    {
        public static void Postfix(BEBehaviorMPRotor __instance, ref double __result)
        {
            var wearAndTearBehaviour = __instance.Blockentity.GetBehavior<IWearAndTear>();
            if (wearAndTearBehaviour == null) return;
            __result *= wearAndTearBehaviour.AvgEfficiencyModifier;
        }

        public static IEnumerable<MethodBase> TargetMethods()
        {
            var baseType = typeof(BEBehaviorMPRotor);

            // Find all derived classes, including the base class itself
            var derivedTypes = AccessTools.AllTypes().Where(type => type != baseType && baseType.IsAssignableFrom(type));

            foreach (var type in derivedTypes)
            {
                var property = type.GetProperty("AccelerationFactor", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (property?.GetMethod != null)
                {
                    yield return property.GetMethod;
                }
            }
        }
    }
}