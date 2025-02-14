using HarmonyLib;
using Vintagestory.GameContent;
using WearAndTear.Code.Behaviours.Parts;

namespace WearAndTear.Code.HarmonyPatches
{
    [HarmonyPatch(typeof(BlockEntityToolMold), "TryTakeContents")]
    public static class ConsumeDurabilityOnCollectingFromMold
    {
        public static void Postfix(BlockEntityToolMold __instance, ref bool __result)
        {
            if (!__result) return;
            __instance.GetBehavior<WearAndTearMoldPartBehavior>()?.Damage();
        }
    }
}