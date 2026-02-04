using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace WearAndTear.HarmonyPatches;

[HarmonyPatch(typeof(BlockEntityContainer), nameof(BlockEntityContainer.OnBlockPlaced))]
public static class AddBaseCallToEntityContainerOnBlockPlaced
{
    //TODO modify so it checks if base method is called and abort if that is the case
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var baseMethod = AccessTools.Method(typeof(BlockEntity), nameof(BlockEntity.OnBlockPlaced), new[] { typeof(ItemStack) });

        var instructionList = new List<CodeInstruction>(instructions);
        var insertIndex = 0; // Modify as needed to specify where to insert the base method call

        // Insert instructions to call the base method
        var newInstructions = new List<CodeInstruction>
        {
            new(OpCodes.Ldarg_0), // Load `this`
            new(OpCodes.Ldarg_1), // Load `byItemStack`
            new(OpCodes.Call, baseMethod) // Call the base method
        };

        instructionList.InsertRange(insertIndex, newInstructions);

        return instructionList;
    }
}