using HarmonyLib;
using InDappledGroves.Util.RecipeTools;
using InsanityLib.Util.SpanUtil;
using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace WearAndTear.Code.HarmonyPatches.indappledgroves;

[HarmonyPatchCategory("indappledgroves")]
[HarmonyPatch]
public static class CreateSawDust
{
    [HarmonyPatch("InDappledGroves.Util.Handlers.RecipeHandler", "SpawnOutput")]
    [HarmonyPostfix]
    public static void AppendSpawnOutput(ItemStack output, EntityAgent byEntity, BlockPos pos)
    {
        if (!output.Collectible.FirstCodePartAsSpan().SequenceEqual("plank")) return;

        SpawnOutput(byEntity.World, new ItemStack(byEntity.World.GetItem(new AssetLocation("wearandtear","sawdust")), 1), pos);
    }

    [HarmonyPatch("InDappledGroves.CollectibleBehaviors.BehaviorIDGTool", "SpawnOutput")]
    [HarmonyPostfix]
    public static void AppendSpawnOutputManual(object recipe, BlockPos pos, ICoreAPI ___api)
    {
        if (recipe is not IDGRecipeNames.GroundRecipe parsedRecipe || !parsedRecipe.Output.ResolvedItemstack.Collectible.FirstCodePartAsSpan().SequenceEqual("plank")) return;

        SpawnOutput(___api.World, new ItemStack(___api.World.GetItem(new AssetLocation("wearandtear","sawdust")), 1), pos);
    }

    private static void SpawnOutput(IWorldAccessor world, ItemStack stack, BlockPos pos)
    {
        for (int i = stack.StackSize; i > 0; i--)
        {
            var singleItem = stack.Clone();
            singleItem.StackSize = 1;

            world.SpawnItemEntity(stack, pos.ToVec3d(), new Vec3d(0.05000000074505806, 0.10000000149011612, 0.05000000074505806));
        }
    }
}