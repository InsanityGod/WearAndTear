using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using WearAndTear.Code.Interfaces;

namespace WearAndTear.Code.Behaviours.Parts.Abstract;

public abstract class ItemPart : OptionalPart, IWearAndTearItemPart
{
    protected ItemPart(BlockEntity blockentity) : base(blockentity) { }
    
    //TODO this should never be critical
    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
    {
        //Empty on purpose
    }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        //Empty on purpose
    }

    public override void UpdateDecay(double daysPassed)
    {
        //Empty on purpose
    }

    public virtual ItemStack ItemStack
    {
        get => ItemSlot?.Itemstack;
        set
        {
            if (ItemSlot == null) return;
            ItemSlot.Itemstack = value;
        }
    }

    public abstract ItemSlot ItemSlot { get; }

    public bool ItemCanBeDamaged => (ItemStack?.Collectible?.GetMaxDurability(ItemStack) ?? 0) > 0;

    public override float Durability
    {
        get
        {
            if (ItemStack == null) return 0;
            var maxDurability = ItemStack.Collectible.GetMaxDurability(ItemStack);
            var durability = ItemStack.Attributes.GetInt("durability", maxDurability);
            return durability / (float)maxDurability;
        }
        set
        {
            var item = ItemStack?.Collectible;
            if (item == null) return;
            var maxDurability = item.GetMaxDurability(ItemStack);
            ItemStack.Attributes.SetInt("durability", (int)Math.Ceiling(maxDurability * value));
            ItemSlot?.MarkDirty();
        }
    }

    public override void GetWearAndTearInfo(IPlayer forPlayer, StringBuilder dsc)
    {
        if (!ItemCanBeDamaged || ItemStack == null) return;

        dsc.AppendLine($"{ItemStack.GetName()} {Lang.Get("Durability: {0} / {1}",
            ItemStack.Collectible.GetRemainingDurability(ItemStack),
            ItemStack.Collectible.GetMaxDurability(ItemStack)
        )}");
    }

    public virtual void DamageItem(int amount = 1)
    {
        if (!ItemCanBeDamaged || ItemStack == null) return;

        var entity = this as BlockEntityBehavior;

        var durability = ItemStack.Attributes.GetInt("durability", ItemStack.Collectible.GetMaxDurability(ItemStack));

        if (durability <= amount)
        {
            entity.Api.World.PlaySoundAt(new("sounds/effect/toolbreak"), Pos.X, Pos.Y, Pos.Z, null, true, 32f, 1f);
            ItemStack = null;
        }
        else ItemStack.Attributes.SetInt("durability", durability - amount);

        ItemSlot?.MarkDirty();
        entity.Blockentity.MarkDirty();
    }
}