using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent.Mechanics;

namespace WearAndTear.HarmonyPatches
{
    [HarmonyPatch]
    public static class FixSetBlockItemstackParameter
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            // Find all TryPlaceBlock methods in types derived from BlockMPBase
            var blockMPBaseType = typeof(BlockMPBase);
            var assembly = blockMPBaseType.Assembly;

            return assembly.GetTypes()
                .Where(t => t.IsSubclassOf(blockMPBaseType))
                .SelectMany(t => t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                .Where(m => m.Name == "TryPlaceBlock");
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                // Look for the SetBlock call
                if (instruction.opcode == OpCodes.Callvirt &&
                    instruction.operand is MethodInfo method &&
                    method.Name == nameof(IBlockAccessor.SetBlock) &&
                    method.GetParameters().Length == 2) // Match the overload with 2 parameters
                {
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
        }
    }
}