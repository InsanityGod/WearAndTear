using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WearAndTear.Config
{
    public class ModConfig
    {
        /// <summary>
        /// The lowest durability objects should ever drop to.
        /// (Setting this higher means the objects will never fully break but only decrease in efficiency)
        /// </summary>
        public float MinDurability { get; set; } = 0;

        /// <summary>
        /// Until which point you don't actually lose any items if you where to break pick it up without repairing.
        /// </summary>
        public float DurabilityLeeway { get; set; } = .98f;

        public int DurabilityUpdateFrequencyInMs { get; set; } = 2500;
    }
}