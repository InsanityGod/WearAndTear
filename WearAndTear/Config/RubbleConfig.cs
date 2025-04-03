using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace WearAndTear.Config
{
    /// <summary>
    /// Configuration for rubble and scrap that will be generated when the block breaks
    /// </summary>
    public class RubbleConfig
    {
        /// <summary>
        /// Wether a rubble block should be generated when the block breaks
        /// (For this to work propperly you should also enable GenerateScrap)
        /// </summary>
        [DefaultValue(true)]
        public bool RubbleBlock { get; set; } = true;

        /// <summary>
        /// Wether scrap should be generated
        /// </summary>
        [DefaultValue(true)]
        public bool GenerateScrap { get; set; } = true;

        /// <summary>
        /// The fixed percentage of content level that will always drop
        /// (Drop = ContentLevel * ((Durability * DurabilityDropPercentage) + FixedDropPercentage)
        /// </summary>
        [DefaultValue(.2f)]
        public float FixedDropPercentage { get; set;} = .2f;
        
        /// <summary>
        /// The percentage of content level that will drop based on durability
        /// (Drop = ContentLevel * ((Durability * DurabilityDropPercentage) + FixedDropPercentage)
        /// </summary>
        [DefaultValue(.8f)]
        public float DurabilityDropPercentage { get; set;} = .8f;
        
        /// <summary>
        /// How much damage you take from sprinting into rubble
        /// </summary>
        [DefaultValue(2f)]
        public float SprintIntoDamage { get; set; } = 2f;

        /// <summary>
        /// How much damage you take from falling onto rubble
        /// </summary>
        [DefaultValue(4f)]
        public float FallIntoDamageMul { get; set; } = 4f;
    }
}
