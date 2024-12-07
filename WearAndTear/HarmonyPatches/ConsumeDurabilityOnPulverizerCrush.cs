using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Behaviours.Parts.Item;
using WearAndTear.Interfaces;

namespace WearAndTear.HarmonyPatches
{
    [HarmonyPatch(typeof(BEPulverizer), "Crush")]
    public static class ConsumeDurabilityOnPulverizerCrush
    {
        public static void Postfix(BlockEntity __instance) => __instance.GetBehavior<WearAndTearPulverizerItemBehavior>()?.DamageItem();
    }
}
