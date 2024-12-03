using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Interfaces;
using System.Linq;

namespace WearAndTear.HarmonyPatches
{
    [HarmonyPatch]
    public static class IncludeEfficiencyModifierInRotorFloatCalculations
    {


        public static void Postfix(BEBehaviorMPRotor __instance, ref float __result)
        {
            var wearAndTearBehaviour = __instance.Blockentity.GetBehavior<IWearAndTear>();
            if (wearAndTearBehaviour == null) return;
            __result *= wearAndTearBehaviour.AvgEfficiencyModifier;
        }

        public static IEnumerable<MethodBase> TargetMethods()
        {
            var baseType = typeof(BEBehaviorMPRotor);

            // Find all derived classes, including the base class itself
            var derivedTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type != baseType && baseType.IsAssignableFrom(type));

            foreach (var type in derivedTypes)
            {
                var property = type.GetProperty("TargetSpeed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (property?.GetMethod != null)
                {
                    yield return property.GetMethod;
                }
                
                property = type.GetProperty("TorqueFactor", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (property?.GetMethod != null)
                {
                    yield return property.GetMethod;
                }
            }
        }
    }
}