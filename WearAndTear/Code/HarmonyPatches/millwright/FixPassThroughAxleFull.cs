using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace WearAndTear.Code.HarmonyPatches.millwright;

[HarmonyPatchCategory("millwright")]
[HarmonyPatch]
public static class FixPassThroughAxleFull
{
    [HarmonyPatch("Millwright.ModSystem.BlockAxlePassthroughFull", "TryPlaceBlock")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> FixPlacement(IEnumerable<CodeInstruction> instructions)
    {
        var codes = instructions.ToList();

        var methodToFind = AccessTools.Method(typeof(IBlockAccessor), nameof(IBlockAccessor.SetBlock), parameters: new Type[]
        {
            typeof(int),
            typeof(BlockPos)
        });

        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].Calls(methodToFind))
            {
                codes[i].operand = AccessTools.Method(typeof(IBlockAccessor), nameof(IBlockAccessor.SetBlock), parameters: new Type[]
                {
                    typeof(int),
                    typeof(BlockPos),
                    typeof(ItemStack)
                });
                codes.Insert(i, new(OpCodes.Ldarg_3));
                break;
            }
        }

        return codes;
    }
}