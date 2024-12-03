using System;

namespace WearAndTear.Config.Props
{
    public class WearAndTearPartProps
    {
        /// <summary>
        /// Name of the part
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of repair tool will be required to repair this part
        /// </summary>
        public string RepairType { get; set; }

        /// <summary>
        /// How long the object should last on average
        /// </summary>
        public float AvgLifeSpanInYears { get; set; } = 1;

        /// <summary>
        /// How the missing durability translates itself into loss in efficiency
        /// (0 meaning no loss even when fully broken, 1 means it will stop working altogether when fully broken)
        /// </summary>
        public float DurabilityEfficiencyRatio { get; set; } = 1;

        /// <summary>
        /// Whether this part is critical to the object.
        /// If this is set to true, the entire object will fall apart when
        /// </summary>
        public bool IsCritical { get; set; } = false;

        /// <summary>
        /// The decay affecting this part (this are used to select the DecayEngines)
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