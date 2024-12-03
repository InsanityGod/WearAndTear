namespace WearAndTear.Config
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
        /// If set to false, the helve hammer will only be damaged if there is actually something on the anvil
        /// </summary>
        public bool DamageHelveHammerEvenIfNothingOnAnvil { get; set; } = true;

        public FeatureConfig Features { get; set; } = new();
    }
}