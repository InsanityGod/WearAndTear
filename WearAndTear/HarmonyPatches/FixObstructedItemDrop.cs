using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Behaviours.Parts;

namespace WearAndTear.HarmonyPatches
{
    [HarmonyPatch(typeof(BEBehaviorWindmillRotor), "obstructed")]
    public static class FixObstructedItemDrop
    {
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
    }
}