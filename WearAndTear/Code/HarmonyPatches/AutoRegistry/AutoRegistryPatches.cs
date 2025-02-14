using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using WearAndTear.Code.Interfaces;
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
            catch (Exception ex)
            {
            }
        }

        [HarmonyPatch(typeof(BlockEntityToolMold), nameof(BlockEntityToolMold.OnPlayerInteract))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FixRightClickToPickup(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var method = AccessTools.Method(typeof(AutoRegistryPatches), nameof(FixItemStack));
            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if (code.opcode == OpCodes.Stloc_2)
                {
                    codes.InsertRange(i, new CodeInstruction[]
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

        public static ItemStack FixItemStack(ItemStack stack, BlockEntityToolMold instance, IPlayer byPlayer)
        {
            var wearAndTear = instance.Api.World.BlockAccessor.GetBlockEntity(instance.Pos)?.GetBehavior<IWearAndTear>();
            var fixedStacks = wearAndTear?.ModifyDroppedItemStacks(new ItemStack[] { stack }, instance.Api.World, instance.Pos, byPlayer);
            if (fixedStacks?.Length == 1) return fixedStacks[0];
            return stack;
        }

        //[HarmonyPatch(typeof(BlockBehaviorRightClickPickup), nameof(BlockBehaviorRightClickPickup.OnBlockInteractStart))]
        //[HarmonyTranspiler]
        //public static IEnumerable<CodeInstruction> FixRightClickToPickup(IEnumerable<CodeInstruction> instructions)
        //{
        //    var codes = instructions.ToList();
        //    var method = AccessTools.Method(typeof(AutoRegistryPatches), nameof(FixItemStacks));
        //    for(int i = 0; i < codes.Count; i++)
        //    {
        //        var code = codes[i];
        //        if(code.opcode == OpCodes.Stloc_0)
        //        {
        //            codes.InsertRange(i, new CodeInstruction[]
        //            {
        //                CodeInstruction.LoadArgument(1),
        //                CodeInstruction.LoadArgument(2),
        //                CodeInstruction.LoadArgument(3),
        //                new(OpCodes.Call, method),
        //            });
        //            break;
        //        }
        //    }
        //    return codes;
        //}

        //public static ItemStack[] FixItemStacks(ItemStack[] stacks, IWorldAccessor world, IPlayer byPlayer, BlockPos pos)
        //{
        //    var wearAndTear = world.BlockAccessor.GetBlockEntity(pos)?.GetBehavior<IWearAndTear>();
        //
        //    return wearAndTear?.ModifyDroppedItemStacks(stacks, world, pos, byPlayer) ?? stacks;
        //}
    }
}