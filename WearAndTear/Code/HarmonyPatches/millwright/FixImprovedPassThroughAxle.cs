using HarmonyLib;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Vintagestory.API.Common;
using WearAndTear.Code.Behaviours;

namespace WearAndTear.Code.HarmonyPatches.millwright;

[HarmonyPatchCategory("millwright")]
[HarmonyPatch]
public static class FixImprovedPassThroughAxle
{
    [HarmonyPatch("Millwright.ModSystem.ImprovedBlockAxlePassthrough", "OnBlockBroken")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> FixOnBlockBroken(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions, generator);

        matcher.MatchEndForward(
            CodeMatch.Calls(AccessTools.PropertyGetter(typeof(ItemSlot), nameof(ItemSlot.Itemstack)))
        );
        
        var skipEarlyReturn = generator.DefineLabel();

        //Early return if the Itemstack is null (prevents crash when WearAndTear has already stolen inventory content)
        matcher.InsertAfterAndAdvance(
            new CodeInstruction(OpCodes.Dup),
            new CodeInstruction(OpCodes.Brtrue_S, skipEarlyReturn),
            new CodeInstruction(OpCodes.Pop),
            new CodeInstruction(OpCodes.Ret)
        );

        matcher.Advance(1);
        matcher.Instruction.labels.Add(skipEarlyReturn);

        return matcher.InstructionEnumeration();
    }

    [HarmonyPatch("Millwright.ModSystem.ImprovedBEAxlePassThrough", "GetBlockInfo")]
    [HarmonyPostfix]
    public static void AppendWearAndTearInfo(BlockEntity __instance, IPlayer forPlayer, StringBuilder sb) => __instance.GetBehavior<PartController>()?.GetBlockInfo(forPlayer, sb);
}
