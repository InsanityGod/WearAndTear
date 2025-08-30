using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using Vintagestory.GameContent.Mechanics;

namespace WearAndTear.Code.HarmonyPatches;

[HarmonyPatch]
public static class FixStupidBaseGameClassesNotCallingBaseMethods
{
    [HarmonyTargetMethods]
    public static IEnumerable<MethodBase> TargetMethods()
    {
        var method = AccessTools.Method(typeof(BlockEntityToolMold), nameof(BlockEntity.GetBlockInfo));
        if(method is not null) yield return method;

        method = AccessTools.Method(typeof(BlockEntityIngotMold), nameof(BlockEntity.GetBlockInfo));
        if(method is not null) yield return method;

        method = AccessTools.Method(typeof(BEPulverizer), nameof(BlockEntity.GetBlockInfo));
        if(method is not null) yield return method;
    }

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> AddBaseCall(IEnumerable<CodeInstruction> instructions)
    {
        var baseMethod = AccessTools.Method(typeof(BlockEntity), nameof(BlockEntity.GetBlockInfo));
        if(instructions.Any(instruction => instruction.Calls(baseMethod))) return instructions; //Base game is already called

        var matcher = new CodeMatcher(instructions);

        matcher.MatchStartForward(new CodeMatch(OpCodes.Ret));
        matcher.Repeat(match =>
        {
            match.InsertAndAdvance(
                new(OpCodes.Ldarg_0), // Load `this`
                new(OpCodes.Ldarg_1), // Load `forPlayer`
                new(OpCodes.Ldarg_2), // Load `dsc`
                new(OpCodes.Call, baseMethod) // Call the base method
            );
            match.Advance(1);
        });

        return matcher.InstructionEnumeration();
    }
}
