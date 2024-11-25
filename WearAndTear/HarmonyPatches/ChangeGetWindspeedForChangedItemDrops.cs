using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Behaviours;

namespace WearAndTear.HarmonyPatches
{
    [HarmonyPatch(typeof(BEBehaviorWindmillRotor), "CheckWindSpeed")]
    public static class ChangeGetWindspeedForChangedItemDrops
    {
        //TODO: register behavior on millwright stuff
        //TODO: Register on millwright stuff
        private static void Prefix(BEBehaviorMPRotor __instance)
        {
            if (__instance.Api.Side != EnumAppSide.Server) return;
            var wearAndTearBehaviour = __instance.Blockentity.GetBehavior<WearAndTearSailBlockEntityBehavior>();
            if (wearAndTearBehaviour == null) return;

            var tranverse = new Traverse(__instance);
            var sailLength = wearAndTearBehaviour.SailLength;
            if (sailLength > 0 && __instance.Api.World.Rand.NextDouble() < 0.2 && tranverse.Method("obstructed", sailLength + 1).GetValue<bool>())
            {
                //Do custom drops
                __instance.Api.World.PlaySoundAt(new AssetLocation("game:sounds/effect/toolbreak"), __instance.Position.X + 0.5, __instance.Position.Y + 0.5, __instance.Position.Z + 0.5, null, false, 20f, 1f);

                //TODO check if dropping millwright sails will allow other sails to be placed onto it afterwards
                WearAndTearSailBlockEntityBehavior.DropSails(__instance.Api, wearAndTearBehaviour, __instance.Pos);

                wearAndTearBehaviour.Enabled = false;
                wearAndTearBehaviour.Durability = 1;
                wearAndTearBehaviour.SailLength = 0;
                __instance.Blockentity.MarkDirty(true, null);

                var manager = __instance.Api.ModLoader.GetModSystem<MechanicalPowerMod>();
                __instance.Network.updateNetwork(manager.getTickNumber());
            }
        }
    }
}