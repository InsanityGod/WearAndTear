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
        /// The minimum drop in durability is required before repairing is allowed
        /// </summary>
        public float MinRepairDurability { get; set; } = .95f;

        /// <summary>
        /// How often the durability update method runs
        /// </summary>
        public int DurabilityUpdateFrequencyInMs { get; set; } = 2500;

        /// <summary>
        /// If set to false, the helve hammer will only be damaged if there is actually something on the anvil
        /// </summary>
        public bool DamageHelveHammerEvenIfNothingOnAnvil { get; set; } = true;
    }
}