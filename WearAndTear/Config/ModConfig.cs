﻿namespace WearAndTear.Config
{
    public class ModConfig
    {
        /// <summary>
        /// The lowest durability objects should ever drop to.
        /// (Setting this higher means the objects will never fully break but only decrease in efficiency, this does not affect items such as helvehammers)
        /// </summary>
        public float MinDurability { get; set; } = 0;

        /// <summary>
        /// Until which point you don't actually lose any items if you where to break the block without first repairing.
        /// </summary>
        public float DurabilityLeeway { get; set; } = .95f;

        /// <summary>
        /// The minimum drop in durability is required before maintenance is allowed
        /// </summary>
        public float MinMaintenanceDurability { get; set; } = .95f;

        /// <summary>
        /// How often the durability update method runs
        /// </summary>
        public int DurabilityUpdateFrequencyInMs { get; set; } = 15000;

        /// <summary>
        /// How often the check if entity is inside room is done
        /// </summary>
        public int RoomCheckFrequencyInMs { get; set; } = 30000;

        /// <summary>
        /// Whether objects can only be repaired while they are not active
        /// </summary>
        public bool MaintenanceRequiresInactivePart { get; set; } = true;

        /// <summary>
        /// When calculating decay for stuff that was unloaded for a long time this decides the ammount of dates the rainfall/temperature is collected from to get an average
        /// </summary>
        public double PollIntervalInDays { get; set; } = 0.1;

        /// <summary>
        /// Whether you can infinitely repair stuff
        /// (When set true you won't ever have to replace stuff but can just continue to repair it)
        /// </summary>
        public bool AllowForInfiniteMaintenance { get; set; } = false;
        
        /// <summary>
        /// Leeway for considering something as Sheltered.
        /// Setting this higher means that you can build larger tunnels and still having it be considered sheltered.
        /// Setting this to -1 will make everything be considered outside
        /// </summary>
        public int RoomExitCountLeeway { get; set; } = 18;

        public DecayModifierConfig DecayModifier { get; set; } = new();

        public SpecialPartConfig SpecialParts { get; set; } = new();

        public AutoPartRegistryConfig AutoPartRegistry { get; set; } = new();
    }
}