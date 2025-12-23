using InsanityLib.Auto.Config;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WearAndTear.Config.Server
{
    public class WearAndTearServerConfig
    {
        [AutoConfig("WearAndTear/Server/MainConfig.json", ServerSync = true)]
        public static WearAndTearServerConfig Instance { get; private set; }

        /// <summary>
        /// If enabled certain actions will require class traits
        /// </summary>
        [DefaultValue(false)]
        public bool TraitRequirements { get; set; } = false;

        /// <summary>
        /// The lowest durability objects should ever drop to.
        /// (Setting this higher means the objects will never fully break but only decrease in efficiency, this does not affect items such as helvehammers)
        /// </summary>
        [Category("Maintenance")]
        [DefaultValue(0)]
        [Range(0d, 1d)]
        [DisplayFormat(DataFormatString = "P")]
        public float MinDurability { get; set; } = 0;

        /// <summary>
        /// Until which point the item/block is still considered to be fully repaired when broken
        /// (If the actual durability is above the specified threshold then it won't affect the drops)
        /// </summary>
        [Category("Maintenance")]
        [DefaultValue(.95f)]
        [Range(0d, 1d)]
        [DisplayFormat(DataFormatString = "P")]
        public float DurabilityLeeway { get; set; } = .95f;

        /// <summary>
        /// The minimum durability is required before maintenance is allowed
        /// </summary>
        [Category("Maintenance")]
        [DefaultValue(.95f)]
        [Range(0d, 1d)]
        [DisplayFormat(DataFormatString = "P")]
        public float MinMaintenanceDurability { get; set; } = .95f;

        /// <summary>
        /// How often the durability update method runs
        /// </summary>
        [Category("Maintenance")]
        [DefaultValue(15000)]
        [Range(1, int.MaxValue)]
        public int DurabilityUpdateFrequencyInMs { get; set; } = 15000;

        /// <summary>
        /// Whether objects can only be repaired while they are not active
        /// </summary>
        [Category("Maintenance")]
        [DefaultValue(true)]
        public bool MaintenanceRequiresInactivePart { get; set; } = true;

        /// <summary>
        /// When calculating decay for stuff that was unloaded for a long time this decides the ammount of dates the rainfall/temperature is collected from to get an average
        /// </summary>
        [Category("Maintenance")]
        [DefaultValue(0.1)]
        [Range(0.01d, double.PositiveInfinity)]
        public double PollIntervalInDays { get; set; } = 0.1;

        /// <summary>
        /// Whether you can infinitely repair stuff
        /// (When set true you won't ever have to replace stuff but can just continue to repair it)
        /// </summary>
        [Category("Maintenance")]
        [DefaultValue(false)]
        public bool AllowForInfiniteMaintenance { get; set; } = false;

        /// <summary>
        /// How often the check if entity is inside room is done
        /// </summary>
        [Category("Shelter")]
        [DefaultValue(30000)]
        [Range(1, int.MaxValue)]
        public int RoomCheckFrequencyInMs { get; set; } = 30000;

        /// <summary>
        /// Leeway for considering something as Sheltered.
        /// Setting this higher means that you can build larger tunnels and still having it be considered sheltered.
        /// Setting this to -1 will make everything be considered outside
        /// </summary>
        [Category("Shelter")]
        [DefaultValue(18)]
        [Range(-1, int.MaxValue)]
        public int RoomExitCountLeeway { get; set; } = 18;
    }
}