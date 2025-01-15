using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Vintagestory.GameContent;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code.Behaviours.Parts.Item;

namespace WearAndTear.HarmonyPatches
{
    [HarmonyPatch(typeof(BEHelveHammer), "onEvery25ms")]
    public static class ConsumeDurabilityOnHelveHammerHit
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool found = false;
            var onHelveHammerHit = AccessTools.Method(typeof(BlockEntityAnvil), nameof(BlockEntityAnvil.OnHelveHammerHit));
            var consumeDurability = AccessTools.Method(typeof(ConsumeDurabilityOnHelveHammerHit), nameof(ConsumeDurability), new Type[] { typeof(BEHelveHammer) });
            foreach (var instruction in instructions)
            {
                if (instruction.Calls(onHelveHammerHit))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, consumeDurability);
                    found = true;
                }
                yield return instruction;
            }
            if (!found) throw new InvalidOperationException("Transpiler failed to find OnHelveHammerHit call to inject code after");
        }

        public static void ConsumeDurability(BEHelveHammer instance) => instance.GetBehavior<WearAndTearHelveItemBehavior>()?.DamageItem();
    }
}