using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using WearAndTear.Interfaces;

namespace WearAndTear.HarmonyPatches
{
    [HarmonyPatch(typeof(Block), nameof(Block.GetDrops))]
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
    }
}
