using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace WearAndTear.Code.HarmonyPatches.Temporary;

[HarmonyPatch]
public static class BEBehaviorRightClickConstructableMissingNullCheckFix
{
    [HarmonyPatch(typeof(BEBehaviorRightClickConstructable), nameof(BEBehaviorRightClickConstructable.OnBlockBroken))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions, generator);

        matcher.MatchStartForward(
            new CodeMatch(OpCodes.Ldarg_1),
            CodeMatch.Calls(AccessTools.PropertyGetter(typeof(IPlayer), nameof(IPlayer.WorldData))),
            CodeMatch.Calls(AccessTools.PropertyGetter(typeof(IWorldPlayerData), nameof(IWorldPlayerData.CurrentGameMode))),
            new CodeMatch(OpCodes.Ldc_I4_2),
            CodeMatch.Branches()
        );

        if (matcher.IsValid)
        {
            matcher.DefineLabel(out var jumpToOriginalCode);
            matcher.DefineLabel(out var jumpAfterCoalesce);
            matcher.InsertAfterAndAdvance(
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Brtrue_S, jumpToOriginalCode),
                new CodeInstruction(OpCodes.Ldc_I4_2),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Br_S, jumpAfterCoalesce)
            );

            matcher.MatchStartForward(CodeMatch.Calls(AccessTools.PropertyGetter(typeof(IPlayer), nameof(IPlayer.WorldData))));
            matcher.Labels.Add(jumpToOriginalCode);
            
            matcher.MatchStartForward(new CodeMatch(OpCodes.Ldc_I4_2));
            matcher.Labels.Add(jumpAfterCoalesce);
        }

        return matcher.InstructionEnumeration();
    }
}
