using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.GameContent.Mechanics;

namespace WearAndTear.HarmonyPatches
{


    [HarmonyPatch(typeof(BEPulverizer), nameof(BEPulverizer.GetBlockInfo))]
    public static class AddBaseCallToBEPulverizerGetBlockInfo
    {
        //TODO modify so it checks if base method is called and abort if that is the case
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var baseMethod = AccessTools.Method(typeof(BlockEntity), nameof(BlockEntity.GetBlockInfo));
    
            var instructionList = new List<CodeInstruction>(instructions);
            var insertIndex = 0; // Insert at the start; change this if needed to insert elsewhere
    
            // Insert instructions to call the base method
            var newInstructions = new List<CodeInstruction>
            {
                new(OpCodes.Ldarg_0), // Load `this`
                new(OpCodes.Ldarg_1), // Load `forPlayer`
                new(OpCodes.Ldarg_2), // Load `sb`
                new(OpCodes.Call, baseMethod) // Call the base method
            };
    
            instructionList.InsertRange(insertIndex, newInstructions);
    
            return instructionList;
        }
    }
}
