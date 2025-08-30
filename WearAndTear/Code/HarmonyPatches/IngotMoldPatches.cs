using HarmonyLib;
using InsanityLib.Util.SpanUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using WearAndTear.Code.Behaviours.Parts;
using WearAndTear.Code.Enums;

namespace WearAndTear.Code.HarmonyPatches
{
    [HarmonyPatch]
    public static class IngotMoldPatches
    {
        [HarmonyPatch(typeof(BlockEntityIngotMold), "TryTakeIngot")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ConsumeIngotMoldDurability(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchEndForward(new CodeMatch(OpCodes.Call, AccessTools.PropertySetter(typeof(BlockEntityIngotMold), nameof(BlockEntityIngotMold.SelectedFillLevel))));
            matcher.InsertAfter(
                CodeInstruction.LoadArgument(0),
                CodeInstruction.LoadArgument(1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(IngotMoldPatches), nameof(DoDamageToIngotMold)))
            );

            return matcher.InstructionEnumeration();
        }

        public static void DoDamageToIngotMold(BlockEntityIngotMold instance, IPlayer byPlayer)
        {
            if(instance.Api.Side != EnumAppSide.Server) return;
            
            EIngotMoldSide side = instance.IsRightSideSelected ? EIngotMoldSide.Right : EIngotMoldSide.Left;
            instance.Behaviors.OfType<IngotMoldPart>().FirstOrDefault(x => x.Side == side)?.Damage(byPlayer);
        }

        [HarmonyPatch(typeof(BlockEntityIngotMold), "TryTakeMold")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FixIngotMoldRightClickToPickup(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchEndForward(new CodeMatch(OpCodes.Newobj, AccessTools.Constructor(typeof(ItemStack), [typeof(Block), typeof(int)]))); //TODO test
            matcher.InsertAfter(
                CodeInstruction.LoadArgument(0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(IngotMoldPatches), nameof(FixItemstack)))
            );

            return matcher.InstructionEnumeration();
        }

        public static ItemStack FixItemstack(ItemStack stack, BlockEntityIngotMold instance)
        {
            var ingotMoldWearAndTear = instance.Behaviors.OfType<IngotMoldPart>().OrderByDescending(x => x.Side).FirstOrDefault(x => x.IsPresent);
            if (ingotMoldWearAndTear != null && ingotMoldWearAndTear.Durability < 1)
            {
                var durabilityTree = stack.Attributes.GetOrAddTreeAttribute(Constants.DurabilityTreeName);
                durabilityTree.SetFloat("Mold", ingotMoldWearAndTear.Durability);
            }
            return stack;
        }

        [HarmonyPatch(typeof(BlockIngotMold), nameof(BlockIngotMold.GetDrops))]
        public static void Postfix(Block __instance, IWorldAccessor world, BlockPos pos, ref ItemStack[] __result)
        {
            if (world.BlockAccessor.GetBlockEntity(pos) is not BlockEntityIngotMold entity) return;

            var ingotMoldWearAndTear = entity.Behaviors.OfType<IngotMoldPart>().Where(x => x.IsPresent).ToList();
            if (ingotMoldWearAndTear.Count == 0) return;

            var blockCode = __instance.FirstCodePartAsSpan().ToString();
            var normalItem = Array.Find(__result, item => item.Collectible is not null && blockCode == item.Collectible.FirstCodePartAsSpan());

            if (normalItem is null) return;

            var secondItem = normalItem.StackSize > 1 ? normalItem.Clone() : null;
            if (secondItem is not null)
            {
                normalItem.StackSize = 1;
                secondItem.StackSize = 1;
            }

            var wearAndTear = ingotMoldWearAndTear[0];
            if (wearAndTear.Durability < 1)
            {
                var durabilityTree = normalItem.Attributes.GetOrAddTreeAttribute(Constants.DurabilityTreeName);
                durabilityTree.SetFloat("Mold", wearAndTear.Durability);
            }

            if (secondItem is not null)
            {
                wearAndTear = ingotMoldWearAndTear[1];
                if (wearAndTear.Durability < 1)
                {
                    var durabilityTree = secondItem.Attributes.GetOrAddTreeAttribute(Constants.DurabilityTreeName);
                    durabilityTree.SetFloat("Mold", wearAndTear.Durability);
                }
                __result = __result.Append(secondItem);
            }
        }

        [HarmonyPatch(typeof(BlockEntityIngotMold), "TryPutMold")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SetDurabilityOnMoldAdd(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchEndForward(
                new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(BlockEntityIngotMold), nameof(BlockEntityIngotMold.QuantityMolds)))
            );

            matcher.InsertAfter(
                CodeInstruction.LoadArgument(0),
                CodeInstruction.LoadArgument(1),
                new(OpCodes.Call, AccessTools.Method(typeof(IngotMoldPatches), nameof(SetDurability)))
            );

            return matcher.InstructionEnumeration();
        }

        public static void SetDurability(BlockEntityIngotMold instance, IPlayer byPlayer)
        {
            var item = byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack;
            if (item is null) return;
            var durabilityTree = item.Attributes.GetTreeAttribute(Constants.DurabilityTreeName);

            var durability = durabilityTree?.GetFloat("Mold", 1) ?? 1;

            var ingotMoldWearAndTear = instance.Behaviors.OfType<IngotMoldPart>().LastOrDefault(x => x.IsPresent);
            if (ingotMoldWearAndTear == null) return;
            ingotMoldWearAndTear.Durability = durability;
        }
    }
}