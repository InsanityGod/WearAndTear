using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using WearAndTear.Config.Props;

namespace WearAndTear.Code.Interfaces
{
    public interface IWearAndTear
    {
        float Durability => Parts.Average(p => p.Durability);

        List<IWearAndTearPart> Parts { get; }

        public bool IsSheltered => false;

        public ItemStack[] ModifyDroppedItemStacks(ItemStack[] itemStacks, IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f);

        bool IsRepairableWith(WearAndTearRepairItemProps props) => Parts.Exists(part => part.CanDoMaintenanceWith(props));

        float AvgEfficiencyModifier
        {
            get
            {
                var totalParts = 0;
                var totalModifier = 0f;

                foreach (var part in Parts)
                {
                    var modifier = part.EfficiencyModifier;
                    if (modifier == null) continue;
                    totalModifier += modifier.Value;
                    totalParts++;
                }

                if (totalParts == 0) return 1;
                return totalModifier / totalParts;
            }
        }

        void UpdateDecay(double daysPassed, bool updateLastUpdatedAt = true);

        bool TryMaintenance(WearAndTearRepairItemProps props, ItemSlot slot, EntityAgent byEntity);
    }
}