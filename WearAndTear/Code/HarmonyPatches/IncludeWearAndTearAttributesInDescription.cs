using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using static HarmonyLib.Code;

namespace WearAndTear.HarmonyPatches
{
    [HarmonyPatch(typeof(CollectibleObject), nameof(CollectibleObject.GetHeldItemInfo))]
    public static class IncludeWearAndTearAttributesInDescription
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool found = false;
            var appendWearAndTearInfo = AccessTools.Method(
                typeof(IncludeWearAndTearAttributesInDescription),
                nameof(AppendWearAndTearInfo),
                new Type[] { typeof(ItemSlot), typeof(StringBuilder) }
            );

            foreach (var instruction in instructions)
            {
                // Check if this is the call to GetMaxDurability
                if (!found && instruction.opcode == OpCodes.Callvirt &&
                    instruction.operand is MethodInfo method &&
                    method.Name == nameof(CollectibleObject.GetMaxDurability))
                {
                    // Inject the call to AppendWearAndTearInfo before GetMaxDurability
                    yield return new CodeInstruction(OpCodes.Ldarg_1); // Load `ItemSlot` (arg 1 of GetHeldItemInfo)
                    yield return new CodeInstruction(OpCodes.Ldarg_2); // Load `StringBuilder` (arg 2 of GetHeldItemInfo)
                    yield return new CodeInstruction(OpCodes.Call, appendWearAndTearInfo); // Call AppendWearAndTearInfo

                    found = true;
                }

                yield return instruction; // Emit the original instruction
            }

            if (!found)
                throw new InvalidOperationException("Transpiler failed to find GetMaxDurability call to inject code before.");
        }

        public static void AppendWearAndTearInfo(ItemSlot inSlot, StringBuilder dsc)
        {
            ITreeAttribute tree = inSlot.Itemstack?.Attributes?.GetTreeAttribute("WearAndTear-Durability");
            if (tree != null)
            {
                dsc.AppendLine();
                foreach (var attr in tree.Where(attr => !attr.Key.Contains('_')))
                {
                    var str = $"{Lang.Get(attr.Key)}: {(int)((float)attr.Value.GetValue() * 100)}%";
                    var repaired = tree.TryGetFloat($"{attr.Key}_Repaired");
                    if (repaired != null)
                    {
                        str = $"{str} ({Lang.Get("wearandtear:repaired")}: {(int)((float)repaired * 100)}%)";
                    }
                    dsc.AppendLine(str);
                }
            }
        }
    }
}