using HarmonyLib;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using WearAndTear.Code;
using WearAndTear.Config.Props;

namespace WearAndTear.HarmonyPatches
{
    [HarmonyPatch(typeof(CollectibleObject), nameof(CollectibleObject.GetHeldItemInfo))]
    public static class IncludeWearAndTearAttributesInDescription
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool found = false;

            foreach (var instruction in instructions)
            {
                // Check if this is the call to GetMaxDurability
                if (!found && instruction.opcode == OpCodes.Callvirt &&
                    instruction.operand is MethodInfo method &&
                    method.Name == nameof(CollectibleObject.GetMaxDurability))
                {
                    // Inject the call to AppendWearAndTearInfo before GetMaxDurability
                    yield return new CodeInstruction(OpCodes.Ldarg_3); // Load `world` (arg 3 of GetHeldItemInfo)
                    yield return new CodeInstruction(OpCodes.Ldarg_1); // Load `ItemSlot` (arg 1 of GetHeldItemInfo)
                    yield return new CodeInstruction(OpCodes.Ldarg_2); // Load `StringBuilder` (arg 2 of GetHeldItemInfo)
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(IncludeWearAndTearAttributesInDescription), nameof(AppendWearAndTearInfo))); // Call AppendWearAndTearInfo

                    found = true;
                }

                yield return instruction; // Emit the original instruction
            }

            if (!found)
                throw new InvalidOperationException("Transpiler failed to find GetMaxDurability call to inject code before.");
        }

        public static void AppendWearAndTearInfo(IWorldAccessor world, ItemSlot inSlot, StringBuilder dsc)
        {
            if (world.Api is not ICoreClientAPI api) return;
            ITreeAttribute tree = inSlot.Itemstack?.Attributes?.GetTreeAttribute(Constants.DurabilityTreeName);
            if (tree == null) return;

            var entityBehaviors = inSlot.Itemstack.Block?.GetPlacedBlock(world.Api)?.BlockEntityBehaviors;
            if (entityBehaviors == null) return;

            dsc.AppendLine();
            foreach (var attr in tree.Where(attr => !attr.Key.EndsWith(Constants.RepairedPrefix)))
            {
                var beh = Array.Find(entityBehaviors, item => item.properties != null && item.properties[nameof(PartProps.Code)].AsString() == attr.Key);

                dsc.AppendLine(PartProps.GetDurabilityStringForPlayer(api, api.World.Player, beh?.properties.AsObject<PartProps>().GetDisplayName() ?? attr.Key, (float)attr.Value.GetValue()));
            }
        }
    }
}