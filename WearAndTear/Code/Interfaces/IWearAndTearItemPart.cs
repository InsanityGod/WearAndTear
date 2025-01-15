using Vintagestory.API.Common;

namespace WearAndTear.Code.Interfaces
{
    public interface IWearAndTearItemPart : IWearAndTearOptionalPart
    {
        ItemStack ItemStack { get; set; }

        ItemSlot ItemSlot { get; }

        bool ItemCanBeDamaged { get; }

        void DamageItem(int amount = 1);
    }
}