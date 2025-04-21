using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using WearAndTear.Code.XLib;
using WearAndTear.Code.XLib.Containers;
using WearAndTear.Config.Props;
using WearAndTear.Config.Server;

namespace WearAndTear.Code.Interfaces
{
    public interface IWearAndTearPart
    {
        public bool RequiresUpdateDecay => true;
        public IWearAndTear WearAndTear { get; }

        public WearAndTearPartProps Props { get; }

        void GetWearAndTearInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            //empty default
        }

        public bool CanDoMaintenanceWith(WearAndTearRepairItemProps props)
        {
            if (props.RequiredMaterialVariant != null && props.RequiredMaterialVariant != Props.MaterialVariant) return false;
            return props.RepairType == Props.RepairType;
        }

        public float? EfficiencyModifier => Props.DurabilityEfficiencyRatio == 0 ? null : 1 - (1f - Durability) * Props.DurabilityEfficiencyRatio;

        float Durability { get; set; }

        void UpdateDecay(double daysPassed);

        public BlockPos Pos { get; }

        bool IsActive { get; }

        bool HasMaintenanceLimit => !WearAndTearServerConfig.Instance.AllowForInfiniteMaintenance && Props.MaintenanceLimit != null;

        /// <summary>
        /// Repairs item and returns remaining repair strength
        /// </summary>
        /// <param name="maintenanceStrength">How much can be repaired</param>
        /// <returns>How much can still be repaired with this item (on other parts that is)</returns>
        float DoMaintenanceFor(float maintenanceStrength, EntityPlayer player)
        {
            var allowedMaintenanceStrength = maintenanceStrength;
            if (HasMaintenanceLimit)
            {
                var limit = Props.MaintenanceLimit.Value;

                if (WearAndTearModSystem.XlibEnabled) limit = SkillsAndAbilities.ApplyLimitBreakerBonus(player.Api, player.Player, limit);

                allowedMaintenanceStrength = GameMath.Clamp(maintenanceStrength, 0, limit - RepairedDurability);
            }

            Durability += allowedMaintenanceStrength;
            var leftOverMaintenanceStrength = maintenanceStrength - allowedMaintenanceStrength + Math.Max(Durability - 1, 0);

            if (HasMaintenanceLimit) RepairedDurability += allowedMaintenanceStrength - Math.Max(Durability - 1, 0);

            Durability = GameMath.Clamp(Durability, WearAndTearServerConfig.Instance.MinDurability, 1);

            return Math.Max(leftOverMaintenanceStrength, 0);
        }

        /// <summary>
        /// Code that can run/override breaking behavior
        /// </summary>
        /// <returns>true if default behavior should run</returns>
        public bool OnBreak() => Props.IsCritical;

        public ItemStack[] ModifyDroppedItemStacks(ItemStack[] itemStacks, IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier, bool isBlockDestroyed) => itemStacks;

        public float RepairedDurability { get; set; }

        public PartBonuses PartBonuses { get; }
    }
}