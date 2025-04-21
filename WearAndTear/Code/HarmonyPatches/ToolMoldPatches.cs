using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using WearAndTear.Code.Behaviours.Parts;
using WearAndTear.Code.Interfaces;

namespace WearAndTear.Code.HarmonyPatches
{
    [HarmonyPatch]
    public static class ToolMoldPatches
    {
        [HarmonyPatch(typeof(BlockEntityToolMold), "TryTakeContents")]
        [HarmonyPostfix]
        public static void ConsumeToolMoldDurability(BlockEntityToolMold __instance, IPlayer byPlayer, ref bool __result)
        {
            if (!__result) return;
            __instance.GetBehavior<WearAndTearMoldPartBehavior>()?.Damage(byPlayer);
        }

        [HarmonyPatch(typeof(BlockEntityToolMold), nameof(BlockEntityToolMold.OnPlayerInteract))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FixToolMoldRightClickToPickup(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var method = AccessTools.Method(typeof(ToolMoldPatches), nameof(FixItemStack));
            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if (code.opcode == OpCodes.Stloc_2)
                {
                    codes.InsertRange(i, new CodeInstruction[]
                    {
                        CodeInstruction.LoadArgument(0),
                        CodeInstruction.LoadArgument(1),
                        new(OpCodes.Call, method),
                    });
                    break;
                }
            }
            return codes;
        }

        public static ItemStack FixItemStack(ItemStack stack, BlockEntityToolMold instance, IPlayer byPlayer)
        {
            var wearAndTear = instance.Api.World.BlockAccessor.GetBlockEntity(instance.Pos)?.GetBehavior<IWearAndTear>();
            var fixedStacks = wearAndTear?.ModifyDroppedItemStacks(new ItemStack[] { stack }, instance.Api.World, instance.Pos, byPlayer);
            if (fixedStacks?.Length == 1) return fixedStacks[0];
            return stack;
        }
    }
}