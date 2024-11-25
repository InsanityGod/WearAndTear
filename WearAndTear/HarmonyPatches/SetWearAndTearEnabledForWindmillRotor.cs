using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Behaviours;

namespace WearAndTear.HarmonyPatches
{
    //ToTreeAttributes instead of FromTreeAttributes since it needs to be run server side
    [HarmonyPatch(typeof(BEBehaviorWindmillRotor), nameof(BEBehaviorWindmillRotor.ToTreeAttributes))]
    public static class SetWearAndTearEnabledForWindmillRotor
    {
        public static void Postfix(BlockEntityBehavior __instance, ITreeAttribute tree)
        {
            var wearAndTearBehaviour = __instance.Blockentity.GetBehavior<WearAndTearBlockEntityBehavior>();
            if (wearAndTearBehaviour == null) return;
            wearAndTearBehaviour.Enabled = tree.GetInt("sailLength", 0) > 0;
        }
    }
}