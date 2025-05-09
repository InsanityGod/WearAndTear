using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            var codes = instructions.ToList();
            var leftField = AccessTools.Field(typeof(BlockEntityIngotMold), nameof(BlockEntityIngotMold.ShatteredLeft));
            var rightField = AccessTools.Field(typeof(BlockEntityIngotMold), nameof(BlockEntityIngotMold.ShatteredRight));

            for (var i = 0; i < codes.Count; i++)
            {
                var currentCode = codes[i];
                if (currentCode.opcode == OpCodes.Ldfld && currentCode.operand is FieldInfo field && codes[i + 1].opcode == OpCodes.Brtrue)
                {
                    if (field == leftField)
                    {
                        codes.InsertRange(i + 2, new CodeInstruction[] {
                            new(OpCodes.Ldarg_0),
                            new(OpCodes.Ldc_I4_1),
                            new(OpCodes.Ldarg_1),
                            new(OpCodes.Call, AccessTools.Method(typeof(IngotMoldPatches), nameof(DoDamageToIngotMold)))
                        });
                        i += 4;
                    }
                    else if (field == rightField)
                    {
                        codes.InsertRange(i + 2, new CodeInstruction[] {
                            new(OpCodes.Ldarg_0),
                            new(OpCodes.Ldc_I4_2),
                            new(OpCodes.Ldarg_1),
                            new(OpCodes.Call, AccessTools.Method(typeof(IngotMoldPatches), nameof(DoDamageToIngotMold)))
                        });
                        i += 4;
                    }
                }
            }
            return codes;
        }

        public static void DoDamageToIngotMold(BlockEntityIngotMold instance, EIngotMoldSide side, IPlayer byPlayer) => instance.Behaviors.OfType<IngotMoldPart>().FirstOrDefault(x => x.Side == side)?.Damage(byPlayer);

        [HarmonyPatch(typeof(BlockEntityIngotMold), "TryTakeMold")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FixIngotMoldRightClickToPickup(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var method = AccessTools.Method(typeof(IngotMoldPatches), nameof(FixItemstack));
            var constructor = AccessTools.Constructor(typeof(ItemStack), new Type[] { typeof(Block), typeof(int) });

            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if (code.opcode == OpCodes.Newobj && code.operand is ConstructorInfo constructorInfo && constructorInfo == constructor)
                {
                    codes.InsertRange(i + 1, new CodeInstruction[]
                    {
                        CodeInstruction.LoadArgument(0),
                        CodeInstruction.LoadArgument(1),
                        new(OpCodes.Call, method),
                    });
                    break;
                }
            }
            return codes;
        }

        public static ItemStack FixItemstack(ItemStack stack, BlockEntityIngotMold instance, IPlayer byPlayer)
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
        public static void Postfix(Block __instance, IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref ItemStack[] __result)
        {
            BlockEntityIngotMold entity = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityIngotMold;
            if (entity == null) return;

            var ingotMoldWearAndTear = entity.Behaviors.OfType<IngotMoldPart>().Where(x => x.IsPresent).ToList();
            if (!ingotMoldWearAndTear.Any()) return;

            string blockCode = __instance.Code.Path.Split('-')[0];
            var normalItem = Array.Find(__result, item => item.Block != null && blockCode == item.Block.Code.Path.Split('-')[0]);

            if (normalItem == null) return;

            var secondItem = normalItem.StackSize > 1 ? normalItem.Clone() : null;
            if (secondItem != null)
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

            if (secondItem != null)
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
            var codes = instructions.ToList();
            var fieldToFind = AccessTools.Field(typeof(BlockEntityIngotMold), nameof(BlockEntityIngotMold.QuantityMolds));

            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if (code.opcode == OpCodes.Stfld && code.operand is FieldInfo field && field == fieldToFind)
                {
                    codes.InsertRange(i + 1, new CodeInstruction[]
                    {
                        new(OpCodes.Ldarg_0),
                        new(OpCodes.Ldarg_1, 1),
                        new(OpCodes.Call, AccessTools.Method(typeof(IngotMoldPatches), nameof(SetDurability)))
                    });
                    break;
                }
            }

            return codes;
        }

        public static void SetDurability(BlockEntityIngotMold instance, IPlayer byPlayer)
        {
            var item = byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack;
            if (item == null) return;
            var durabilityTree = item.Attributes.GetOrAddTreeAttribute(Constants.DurabilityTreeName);

            var durability = durabilityTree.GetFloat("Mold", 1);

            var ingotMoldWearAndTear = instance.Behaviors.OfType<IngotMoldPart>().LastOrDefault(x => x.IsPresent);
            if (ingotMoldWearAndTear == null) return;
            ingotMoldWearAndTear.Durability = durability;
        }
    }
}