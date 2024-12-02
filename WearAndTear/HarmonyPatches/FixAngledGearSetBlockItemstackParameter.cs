using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Behaviours.Parts;

namespace WearAndTear.HarmonyPatches
{
    [HarmonyPatch(typeof(BlockAngledGears), "TryPlaceBlock")]
    public static class FixAngledGearSetBlockItemstackParameter
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool found = false;

            foreach (var instruction in instructions)
            {
                // Look for the SetBlock call
                if (instruction.opcode == OpCodes.Callvirt &&
                    instruction.operand is MethodInfo method &&
                    method.Name == nameof(IBlockAccessor.SetBlock) &&
                    method.GetParameters().Length == 2) // Match the overload with 2 parameters
                {
                    found = true;

                    // Insert the 'ldarg.3' instruction to load the `itemstack` argument onto the stack
                    yield return new CodeInstruction(OpCodes.Ldarg_3); // Load the `itemstack` argument

                    // Update the operand to point to the overload with ItemStack
                    instruction.operand = typeof(IBlockAccessor).GetMethod(
                        nameof(IBlockAccessor.SetBlock),
                        new[] { typeof(int), typeof(BlockPos), typeof(ItemStack) }
                    );
                }

                yield return instruction; // Emit the original instruction
            }

            if (!found)
            {
                throw new InvalidOperationException("Transpiler failed to find SetBlock call to fix");
            }
        }
    }
}