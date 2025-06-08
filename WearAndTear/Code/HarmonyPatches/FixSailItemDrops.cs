using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code.Behaviours.Parts;

namespace WearAndTear.Code.HarmonyPatches
{
    [HarmonyPatch]
    public static class FixSailItemDrops
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> DoDrops(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = instructions.ToList();
            var sailLengthLocal = generator.DeclareLocal(typeof(int));

            //Save and delete sail length
            codes.InsertRange(0, new CodeInstruction[]
            {
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, AccessTools.Method(typeof(FixSailItemDrops), nameof(ReturnAndDeleteSailLength))),
                new(OpCodes.Stloc_S, sailLengthLocal.LocalIndex),
            });

            for(var i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if(code.opcode == OpCodes.Call && code.operand is MethodInfo method && method.Name == "OnBlockBroken")
                {
                    //Restore sail length
                    codes.InsertRange(i - 2, new CodeInstruction[]
                    {
                        new(OpCodes.Ldarg_0),
                        new(OpCodes.Ldloc_S, sailLengthLocal.LocalIndex),
                        new(OpCodes.Call, AccessTools.Method(typeof(FixSailItemDrops), nameof(SetSailLength))),
                    });

                    break;
                }
            }

            return codes;
        }

        public static int ReturnAndDeleteSailLength(BEBehaviorMPRotor instance)
        {
            int result = 0;
            var beh = instance.Blockentity.GetBehavior<WindmillSailPart>();

            if(beh != null)
            {
                result = beh.SailLength;
                beh.SailLength = 0;
            }

            return result;
        }

        public static void SetSailLength(BEBehaviorMPRotor instance, int sailLength)
        {
            var beh = instance.Blockentity.GetBehavior<WindmillSailPart>();
            if(beh != null) beh.SailLength = sailLength;
        }


        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            foreach(var type in Helpers.WindmillRotorBehaviorTypes())
            {
                var method = type.GetMethod(nameof(BEBehaviorWindmillRotor.OnBlockBroken), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (method != null)
                {
                    yield return method;
                }
            }

        }
    }
}