using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.GameContent;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Behaviours;

namespace WearAndTear.HarmonyPatches
{
    [HarmonyPatch(typeof(BEBehaviorWindmillRotor), "TorqueFactor", MethodType.Getter)]
    [HarmonyPatch(typeof(BEBehaviorWindmillRotor), "TargetSpeed", MethodType.Getter)]
    public static class IncludeWearAndTearEfficiencyInTorqueFactorAndTargetSpeed
    {
        public static void Postfix(BEBehaviorMPRotor __instance, ref float __result)
        {
            var wearAndTearBehaviour = __instance.Blockentity.GetBehavior<WearAndTearBlockEntityBehavior>();
            if (wearAndTearBehaviour == null) return;
            __result *= wearAndTearBehaviour.Efficiency;
        }
    }
}