using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using WearAndTear.Config.Props;

namespace WearAndTear.Interfaces
{
    public interface IWearAndTear
    {
        float Durability => Parts.Average(p => p.Durability);

        List<IWearAndTearPart> Parts { get; }

        bool IsRepairableWith(WearAndTearRepairItemProps props) => Parts.Exists(part => part.IsRepairableWith(props));

        float AvgEfficiencyModifier => Parts.Average(part => part.EfficiencyModifier);

        void UpdateDecay(double daysPassed, bool updateLastUpdatedAt = true);

        bool TryRepair(WearAndTearRepairItemProps props, ItemSlot slot, EntityAgent byEntity);
    }
}