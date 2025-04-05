using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code.Behaviours.Parts;

namespace WearAndTear.Code.HarmonyPatches
{
    [HarmonyPatch]
    public static class FixSailItemDrops
    {
        [HarmonyPrefix]
        public static void Prefix(BEBehaviorMPRotor __instance)
        {
            var beh = __instance.Blockentity.GetBehavior<WearAndTearSailBehavior>();
            if (beh == null) return;
            beh.SailLength = 0;
            beh.Durability = 1;
        }

        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            var baseType = typeof(BEBehaviorMPRotor);
            var derivedTypes = AccessTools.AllTypes().Where(type => type != baseType && baseType.IsAssignableFrom(type) && type.Name.StartsWith(nameof(BEBehaviorWindmillRotor)));
            foreach (var type in derivedTypes)
            {
                var method = type.GetMethod(nameof(BEBehaviorWindmillRotor.OnBlockBroken), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (method != null)
                {
                    yield return method;
                }
            }
        }
    }
}