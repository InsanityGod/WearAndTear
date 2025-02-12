using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using WearAndTear.Code.Behaviours;

namespace WearAndTear.Code.HarmonyPatches.linearpower
{
    [HarmonyPatch]
    [HarmonyPatchCategory("linearpower")]
    public static class LinearPowerPatches
    {
        [HarmonyPatch("sawmill.BlockEntitySawmill", "Cut")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> CreateSawDust(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();

            var methodToFind = AccessTools.Method(typeof(IWorldAccessor), nameof(IWorldAccessor.SpawnItemEntity), new Type[] { typeof(ItemStack), typeof(Vec3d), typeof(Vec3d) });

            for(var i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if (code.opcode == OpCodes.Callvirt && code.operand is MethodInfo info && info == methodToFind)
                {
                    code.opcode = OpCodes.Call;
                    code.operand = AccessTools.Method(typeof(LinearPowerPatches), nameof(SpawnEntityAndSawDust));
                }
            }

            return codes;
        }

        public static Entity SpawnEntityAndSawDust(IWorldAccessor accesor, ItemStack itemstack, Vec3d position, Vec3d velocity = null)
        {
            var normallCall = accesor.SpawnItemEntity(itemstack, position, velocity);

            if(itemstack.StackSize > 0 && itemstack.Collectible.FirstCodePart() == "plank")
            {
                var sawdust = accesor.GetItem(new AssetLocation("wearandtear:sawdust"));
                var sawdustStack = new ItemStack(sawdust, 1);
                accesor.SpawnItemEntity(sawdustStack, position, velocity);
            }

            return normallCall;
        }

        [HarmonyPatch("sawmill.BlockEntitySawmill", nameof(BlockEntity.GetBlockInfo))]
        [HarmonyPostfix]
        public static void FixWearAndTearInfoDisplay(BlockEntity __instance, IPlayer forPlayer, StringBuilder sb) => __instance.GetBehavior<WearAndTearBehavior>()?.GetBlockInfo(forPlayer, sb);
    }
}
