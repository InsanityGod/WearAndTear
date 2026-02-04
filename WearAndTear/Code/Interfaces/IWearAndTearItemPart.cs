using Vintagestory.API.Common;

namespace WearAndTear.Code.Interfaces;

public interface IWearAndTearItemPart
{
    ItemStack ItemStack { get; }

    ItemSlot ItemSlot { get; }

    bool ItemCanBeDamaged { get; }

    void DamageItem(int amount = 1);
}