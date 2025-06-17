using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Vintagestory.API.Common;
using WearAndTear.HarmonyPatches;

namespace WearAndTear.Code.HarmonyPatches.AutoRegistry
{
    [HarmonyPatch]
    public static class AutoRegistryPatches
    {
        //TODO should probably come up with a cleaner way of doing this (though really people should remember to call base classes themself -_-)

        public static void EnsureBaseMethodCall(ICoreAPI api, Harmony harmony, MethodInfo method)
        {
            if (!method.IsVirtual || harmony.GetPatchedMethods().Contains(method)) return;

            try
            {
                harmony.Patch(method, transpiler: new HarmonyMethod(typeof(AutoRegistryPatches), nameof(AddCallToBaseClassTranspiler)));
            }
            catch
            {
                //Could not or did not need to patch
            }
        }

        public static IEnumerable<CodeInstruction> AddCallToBaseClassTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
        {
            var codes = instructions.ToList();
            var baseMethod = ((MethodInfo)__originalMethod).GetBaseDefinition();
            for (var i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if (code.opcode == OpCodes.Call && code.operand is MethodInfo info && info == baseMethod) throw new InvalidOperationException("Already calls base class");
                if (code.opcode == OpCodes.Ret)
                {
                    var newCodes = new List<CodeInstruction>
                    {
                        CodeInstruction.LoadArgument(0)
                    };

                    foreach (var param in baseMethod.GetParameters())
                    {
                        newCodes.Add(CodeInstruction.LoadArgument(param.Position + 1));
                    }
                    var baseCall = new CodeInstruction(OpCodes.Call, baseMethod)
                    {
                        labels = code.labels
                    };
                    code.labels = new();
                    newCodes.Add(baseCall);
                    codes.InsertRange(i, newCodes);
                    i += newCodes.Count;
                }
            }
            return codes;
        }

        public static void EnsureBlockDropsConnected(ICoreAPI api, Harmony harmony, Block block)
        {
            var method = block.GetType().GetMethod(nameof(Block.GetDrops));
            if (harmony.GetPatchedMethods().Contains(method)) return;
            try
            {
                harmony.Patch(method, postfix: new HarmonyMethod(typeof(ConnectBlockDropModifier), nameof(ConnectBlockDropModifier.Postfix)));
            }
            catch
            {
                //Empty stump
            }
        }
    }
}