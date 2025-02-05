using HarmonyLib;
using InDappledGroves.Util.Handlers;
using InDappledGroves.Util.RecipeTools;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using WearAndTear.Code;

namespace WearAndTear.HarmonyPatches.indappledgroves
{
    [HarmonyPatchCategory("indappledgroves")]
    [HarmonyPatch]
    public static class CreateSawDust
    {
        [HarmonyPatch("InDappledGroves.Util.Handlers.RecipeHandler", "SpawnOutput")]
        [HarmonyPostfix]
        public static void AppendSpawnOutput(object __instance, ItemStack output, EntityAgent byEntity, BlockPos pos)
        {
            if(output.Collectible.Code.FirstCodePart() == "plank")
            {
                var sawdust = byEntity.World.GetItem(new AssetLocation("wearandtear:sawdust"));
                (__instance as RecipeHandler).SpawnOutput(new ItemStack(sawdust, 1), byEntity, pos);
            }
        }

        [HarmonyPatch("InDappledGroves.CollectibleBehaviors.BehaviorIDGTool", "SpawnOutput")]
        [HarmonyPostfix]
        public static void AppendSpawnOutputManual(CollectibleBehavior __instance, object recipe, BlockPos pos)
        {
            var parsedRecipe = recipe as IDGRecipeNames.GroundRecipe;

            if(parsedRecipe.Output.ResolvedItemstack.Collectible.FirstCodePart() == "plank")
            {
                var api = Traverse.Create(__instance).Field<ICoreAPI>("api").Value;
                var sawdust = api.World.GetItem(new AssetLocation("wearandtear:sawdust"));
                api.World.SpawnItemEntity(new ItemStack(sawdust, 1), pos.ToVec3d(), new Vec3d(0.05000000074505806, 0.10000000149011612, 0.05000000074505806));
            }
        }

    }
}