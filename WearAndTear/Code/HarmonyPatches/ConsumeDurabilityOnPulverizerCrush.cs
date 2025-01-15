using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code.Behaviours.Parts.Item;

namespace WearAndTear.HarmonyPatches
{
    [HarmonyPatch(typeof(BEPulverizer), "Crush")]
    public static class ConsumeDurabilityOnPulverizerCrush
    {
        public static void Postfix(BlockEntity __instance) => __instance.GetBehavior<WearAndTearPulverizerItemBehavior>()?.DamageItem();
    }
}