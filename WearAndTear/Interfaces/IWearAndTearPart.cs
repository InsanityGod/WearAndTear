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

        public float? EfficiencyModifier => Props.DurabilityEfficiencyRatio == 0 ? null : 1 - ((1f - Durability) * Props.DurabilityEfficiencyRatio);

        float Durability { get; set; }

        void UpdateDecay(double daysPassed);

        public BlockPos Pos { get; }

        bool IsActive { get; }

        bool HasMaintenanceLimit => !WearAndTearModSystem.Config.AllowForInfiniteMaintenance && Props.MaintenanceLimit != null;

        /// <summary>
        /// Repairs item and returns remaining repair strength
        /// </summary>
        /// <param name="maintenanceStrength">How much can be repaired</param>
        /// <returns>How much can still be repaired with this item (on other parts that is)</returns>
        float DoMaintenanceFor(float maintenanceStrength)
        {
            var allowedMaintenanceStrength = HasMaintenanceLimit ? 
                GameMath.Clamp(maintenanceStrength, 0, Props.MaintenanceLimit.Value - RepairedDurability) :
                maintenanceStrength;

            Durability += allowedMaintenanceStrength;
            var leftOverMaintenanceStrength = (maintenanceStrength - allowedMaintenanceStrength) + Math.Max(Durability - 1, 0);

            if(HasMaintenanceLimit) RepairedDurability += allowedMaintenanceStrength - Math.Max(Durability - 1, 0);

            Durability = GameMath.Clamp(Durability, WearAndTearModSystem.Config.MinDurability, 1);
            return Math.Max(leftOverMaintenanceStrength, 0);
        }

        public float RepairedDurability { get; set; }
    }
}