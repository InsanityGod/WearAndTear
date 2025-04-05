using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code.Behaviours.Parts;

namespace WearAndTear.Code.HarmonyPatches
{
    [HarmonyPatch]
    public static class FixObstructedItemDrop
    {
        [HarmonyPostfix]
        public static void Postfix(BEBehaviorMPRotor __instance, int len, ref bool __result)
        {
            if (__instance.Api.Side != EnumAppSide.Server || !__result) return;
            var wearAndTearBehaviour = __instance.Blockentity.GetBehavior<WearAndTearSailBehavior>();
            if (wearAndTearBehaviour == null) return;

            if (len == wearAndTearBehaviour.SailLength + 1)
            {
                __instance.Api.World.PlaySoundAt(new AssetLocation("game:sounds/effect/toolbreak"), __instance.Position.X + 0.5, __instance.Position.Y + 0.5, __instance.Position.Z + 0.5, null, false, 20f, 1f);

                wearAndTearBehaviour.DropSails();

                var manager = __instance.Api.ModLoader.GetModSystem<MechanicalPowerMod>();
                __instance.Network.updateNetwork(manager.getTickNumber());
            }
        }

        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            var baseType = typeof(BEBehaviorMPRotor);
            var derivedTypes = AccessTools.AllTypes().Where(type => type != baseType && baseType.IsAssignableFrom(type) && type.Name.StartsWith(nameof(BEBehaviorWindmillRotor)));
            foreach (var type in derivedTypes)
            {
                var method = type.GetMethod("obstructed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (method != null)
                {
                    yield return method;
                }
            }
        }
    }
}