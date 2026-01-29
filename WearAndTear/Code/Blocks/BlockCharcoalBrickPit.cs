using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using WearAndTear.Code.BlockEntities;

namespace WearAndTear.Code.Blocks;

public class BlockCharcoalBrickPit : Block, IIgnitable
{
    WorldInteraction[] interactions;

    public override void OnLoaded(ICoreAPI api)
    {
        base.OnLoaded(api);

        interactions = ObjectCacheUtil.GetOrCreate(api, "charcoalbrickpitInteractions", () =>
        {
            List<ItemStack> canIgniteStacks = BlockBehaviorCanIgnite.CanIgniteStacks(api, true);

            return new WorldInteraction[]
            {
                new()
                {
                    ActionLangCode = "blockhelp-firepit-ignite",
                    MouseButton = EnumMouseButton.Right,
                    HotKeyCode = "shift",
                    Itemstacks = canIgniteStacks.ToArray(),
                    GetMatchingStacks = (wi, bs, es) => {
                        BlockEntityCharcoalBrickPit becp = api.World.BlockAccessor.GetBlockEntity(bs.Position) as BlockEntityCharcoalBrickPit;
                        return becp?.Lit == false ? wi.Itemstacks : null;
                    }
                }
            };
        });
    }

    EnumIgniteState IIgnitable.OnTryIgniteStack(EntityAgent byEntity, BlockPos pos, ItemSlot slot, float secondsIgniting)
    {
        if (api.World.BlockAccessor.GetBlockEntity(pos) is BlockEntityCharcoalBrickPit becp && becp.Lit) return secondsIgniting > 2 ? EnumIgniteState.IgniteNow : EnumIgniteState.Ignitable;

        return EnumIgniteState.NotIgnitable;
    }

    public EnumIgniteState OnTryIgniteBlock(EntityAgent byEntity, BlockPos pos, float secondsIgniting)
    {
        if (api.World.BlockAccessor.GetBlockEntity(pos) is not BlockEntityCharcoalBrickPit becp || becp.Lit) return EnumIgniteState.NotIgnitablePreventDefault;

        return secondsIgniting > 3 ? EnumIgniteState.IgniteNow : EnumIgniteState.Ignitable;
    }

    public void OnTryIgniteBlockOver(EntityAgent byEntity, BlockPos pos, float secondsIgniting, ref EnumHandling handling)
    {
        if (api.World.BlockAccessor.GetBlockEntity(pos) is BlockEntityCharcoalBrickPit becp && !becp.Lit) becp.IgniteNow();

        handling = EnumHandling.PreventDefault;
    }

    public override bool ShouldReceiveClientParticleTicks(IWorldAccessor world, IPlayer player, BlockPos pos, out bool isWindAffected)
    {
        bool val = base.ShouldReceiveClientParticleTicks(world, player, pos, out _);
        isWindAffected = true;

        return val;
    }

    public static bool IsDrySawdustBrickPile(IWorldAccessor world, BlockPos pos)
    {
        BlockEntityGroundStorage blockEntity = world.BlockAccessor.GetBlockEntity<BlockEntityGroundStorage>(pos);
        
        return blockEntity != null && blockEntity.Inventory[0]?.Itemstack?.Collectible.Code == "wearandtear:sawdustbrick-dry";
    }

    public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer) => interactions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));

    public static void ConvertPile(IWorldAccessor world, BlockPos pos)
    {
        BlockEntityGroundStorage blockEntity = world.BlockAccessor.GetBlockEntity<BlockEntityGroundStorage>(pos);
        if(blockEntity == null) return;
        var burnedBrick = world.GetItem("wearandtear:sawdustbrick-burned");

        foreach(var slot in blockEntity.Inventory)
        {
            if(slot.Empty || slot.Itemstack.Collectible.Code != "wearandtear:sawdustbrick-dry") continue;

            slot.Itemstack = new ItemStack(burnedBrick, slot.Itemstack.StackSize);
            blockEntity.MarkDirty();
        }
    }
}
