using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using Vintagestory.API.Common;

namespace WearAndTear.Code.HarmonyPatches.Temporary;

[HarmonyPatch("ExpandedMolds.code.ExpandedMoldsModSystem", nameof(ModSystem.AssetsFinalize))]
[HarmonyPatchCategory("expandedmolds")]
public static class ExpandedMoldsFix
{

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
    {
        foreach(var instruction in instructions)
        {
            if (instruction.opcode == OpCodes.Ldstr && instruction.operand is string StrOperand)
            {
                if (StrOperand == "WearAndTear")
                {
                    instruction.operand = "wearandtear:PartController";
                }
                else if (StrOperand == "WearAndTearMold")
                {
                    instruction.operand = "wearandtear:MoldPart";
                }
            }
            yield return instruction;
        }
    }
}
