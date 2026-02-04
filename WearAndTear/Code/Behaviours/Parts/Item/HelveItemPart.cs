using HarmonyLib;
using MechanicalWoodSplitter.Code.FakeStuff;
using MechanicalWoodSplitter.Code.Items;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code.Behaviours.Parts.Abstract;
using WearAndTear.Config.Server;

namespace WearAndTear.Code.Behaviours.Parts.Item;

public class HelveItemPart : ItemPart
{
    public readonly BEHelveHammer HelveHammerBase;

    public HelveItemPart(BlockEntity blockentity) : base(blockentity)
    {
        HelveHammerBase = (BEHelveHammer)blockentity;
    }

    public override ItemStack ItemStack
    {
        get => HelveHammerBase.HammerStack;
        set => HelveHammerBase.HammerStack = value;
    }

    public override ItemSlot ItemSlot => null;

    public override void DamageItem(int amount = 1)
    {
        var anvil = Traverse.Create(HelveHammerBase).Field("targetAnvil").GetValue<BlockEntityAnvil>();
        if (WearAndTearModSystem.HelveAxeModLoaded && IsHelveAxe)
        {
            if (!ShoulDamageHelveAxe(anvil)) return;
        }
        else if (!SpecialPartsConfig.Instance.HelveHammer || (!SpecialPartsConfig.Instance.DamageHelveHammerEvenIfNothingOnAnvil && anvil.WorkItemStack == null)) return;

        base.DamageItem(amount);
    }

    public override ItemStack[] ModifyDroppedItemStacks(ItemStack[] itemStacks, IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier, bool isBlockDestroyed)
    {
        if (isBlockDestroyed && ItemStack is not null)
        {
            itemStacks = itemStacks.Append(ItemStack);
            ItemStack = null;
        }

        return itemStacks;
    }

    public bool IsHelveAxe => ItemStack?.Collectible is HelveAxe;

    public bool ShoulDamageHelveAxe(object anvil) => SpecialPartsConfig.Instance.HelveAxe && anvil is FakeBlockEntityAnvil fakeAnvil && (fakeAnvil.ChoppingBlock?.recipecomplete ?? false);

    public void ManualDamageIem(int amount = 1) => base.DamageItem(amount);
}