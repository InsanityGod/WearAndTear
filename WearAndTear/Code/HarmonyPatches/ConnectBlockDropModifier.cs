using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code.Interfaces;

namespace WearAndTear.HarmonyPatches
{
    [HarmonyPatch]
    public static class ConnectBlockDropModifier
    {
        public static void Postfix(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref ItemStack[] __result)
        {
            var wearAndTear = world.BlockAccessor.GetBlockEntity(pos)?.GetBehavior<IWearAndTear>();
            if (wearAndTear != null)
            {
                __result = wearAndTear.ModifyDroppedItemStacks(__result, world, pos, byPlayer);
            }
        }

        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return typeof(Block).GetMethod("GetDrops");
            yield return typeof(BlockPulverizer).GetMethod("GetDrops");

            var sawmillDropMethod = AccessTools.TypeByName("sawmill.BlockSawmill")?.GetMethod("GetDrops");
            if(sawmillDropMethod != null) yield return sawmillDropMethod;
        }
    }
}