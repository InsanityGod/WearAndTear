using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code.Behaviours.Parts;
using WearAndTear.Config.Client;

namespace WearAndTear.Code.HarmonyPatches
{
    [HarmonyPatch]
    public static class FixWindmillShape
    {
        [HarmonyPostfix]
        public static void FixUpdateShape(BEBehaviorMPRotor __instance)
        {
            if (!WearAndTearClientConfig.Instance.WindmillRotoDecayedAppearance) return;
            var wearAndTearBehaviour = __instance.Blockentity.GetBehavior<WindmillSailPart>();
            if (wearAndTearBehaviour == null || wearAndTearBehaviour.SailLength == 0) return;
            wearAndTearBehaviour.UpdateShape(__instance);
        }

        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            foreach(var type in Helpers.WindmillRotorBehaviorTypes())
            {
                var method = type.GetMethod("updateShape", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (method != null)
                {
                    yield return method;
                }
            }
        }
    }
}