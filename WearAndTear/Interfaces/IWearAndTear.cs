using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using WearAndTear.Config.Props;

namespace WearAndTear.Interfaces
{
    public interface IWearAndTear
    {
        float Durability => Parts.Average(p => p.Durability);

        List<IWearAndTearPart> Parts { get; }

        public bool IsInsideRoom => false;

        public ItemStack[] ModifyDroppedItemStacks(ItemStack[] itemStacks, IWorldAccessor world, BlockPos pos, IPlayer byPlayer);

        bool IsRepairableWith(WearAndTearRepairItemProps props) => Parts.Exists(part => part.CanDoMaintenanceWith(props));

        float AvgEfficiencyModifier => Parts.Average(part => part.EfficiencyModifier);

        void UpdateDecay(double daysPassed, bool updateLastUpdatedAt = true);

        bool TryMaintenance(WearAndTearRepairItemProps props, ItemSlot slot, EntityAgent byEntity);
    }
}