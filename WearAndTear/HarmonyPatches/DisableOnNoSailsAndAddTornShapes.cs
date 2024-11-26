using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Behaviours;

namespace WearAndTear.HarmonyPatches
{
    //TODO register on millwright stuff once the shapes have been made
    [HarmonyPatch(typeof(BEBehaviorWindmillRotor), "updateShape")]
    public static class DisableOnNoSailsAndAddTornShapes
    {
        public static void Postfix(BEBehaviorMPRotor __instance)
        {
            var wearAndTearBehaviour = __instance.Blockentity.GetBehavior<WearAndTearSailBlockEntityBehavior>();
            if (wearAndTearBehaviour == null) return;
            if (wearAndTearBehaviour.SailLength == 0) wearAndTearBehaviour.Enabled = false;
            wearAndTearBehaviour.UpdateShape(__instance, "torn");
        }
    }
}