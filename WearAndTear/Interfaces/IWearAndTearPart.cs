using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using WearAndTear.Config.Props;

namespace WearAndTear.Interfaces
{
    public interface IWearAndTearPart
    {
        public IWearAndTear WearAndTear { get; }

        public WearAndTearPartProps Props { get; }

        void GetWearAndTearInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            //empty default
        }

        public bool CanDoMaintenanceWith(WearAndTearRepairItemProps props) => props.RepairType == Props.RepairType;

        public float EfficiencyModifier => 1 - ((1f - Durability) * Props.DurabilityEfficiencyRatio);

        float Durability { get; set; }

        void UpdateDecay(double daysPassed);

        public BlockPos Pos { get; }

        /// <summary>
        /// Repairs item and returns remaining repair strength
        /// </summary>
        /// <param name="repairStrength">How much can be repaired</param>
        /// <returns>How much can still be repaired with this item (on other parts that is)</returns>
        float DoMaintenanceFor(float repairStrength)
        {
            Durability += repairStrength;
            var leftOver = Durability - 1;

            Durability = Math.Min(Durability, 1);
            return Math.Min(leftOver, 0);
        }
    }
}