using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WearAndTear.Config.Props
{
    public class WearAndTearPartProps
    {
        /// <summary>
        /// Name of the part
        /// </summary>
        public string Name { get; set; }
        //TODO migrate to use language strings instead (should probably wait until major update as it will break existing parts)

        /// <summary>
        /// The type of repair tool that will be required to repair this part
        /// </summary>
        public string RepairType { get; set; }

        /// <summary>
        /// How long the object should last on average
        /// </summary>
        [DefaultValue(1f)]
        [Range(0, float.PositiveInfinity)]
        public float AvgLifeSpanInYears { get; set; } = 1;

        /// <summary>
        /// How the missing durability translates itself into loss in efficiency
        /// (0 meaning no loss even when fully broken, 1 means it will stop working altogether when fully broken)
        /// </summary>
        [Range(0, 2)]
        [DefaultValue(0)]
        public float DurabilityEfficiencyRatio { get; set; } = 0;

        /// <summary>
        /// Whether this part is critical to the object.
        /// If this is set to true, the entire object will fall apart when durability reaches 0%
        /// </summary>
        public bool IsCritical { get; set; }

        /// <summary>
        /// The maximum ammount of durability that can be repaired before the item has to be fully replaced
        /// </summary>
        [Range(0, float.PositiveInfinity)]
        public float? MaintenanceLimit { get; set; }

        /// <summary>
        /// The decay affecting this part (these are used to select the DecayEngines)
        /// </summary>
        public WearAndTearDecayProps[] Decay { get; set; } = new WearAndTearDecayProps[]
        {
            new()
            {
                Type = "time"
            }
        };
    }
}