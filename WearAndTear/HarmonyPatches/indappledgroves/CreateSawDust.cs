using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace WearAndTear.HarmonyPatches.indappledgroves
{
    public static class CreateSawDust
    {
        public static void SpawnSawDustSawBuck(BlockEntity __instance)
        {
            var sawdust = __instance.Api.World.GetItem(new AssetLocation("wearandtear:sawdust"));
            __instance.Api.World.SpawnItemEntity(new ItemStack(sawdust, 1), __instance.Pos.ToVec3d(), new Vec3d(0.05000000074505806, 0.10000000149011612, 0.05000000074505806));
        }

        public static void SpawnSawDustGroundRecipe(CollectibleBehavior __instance, object recipe, BlockPos pos)
        {
            var mode = Traverse.Create(recipe).Field<string>("ToolMode").Value;
            if(mode != "sawing") return;
            var api = Traverse.Create(__instance).Field<ICoreAPI>("api").Value;
            var sawdust = api.World.GetItem(new AssetLocation("wearandtear:sawdust"));
            api.World.SpawnItemEntity(new ItemStack(sawdust, 1), pos.ToVec3d(), new Vec3d(0.05000000074505806, 0.10000000149011612, 0.05000000074505806));
        }
    }
}
