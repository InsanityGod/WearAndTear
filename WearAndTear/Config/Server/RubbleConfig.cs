using InsanityLib.Attributes.Auto.Config;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WearAndTear.Config.Server
{
    /// <summary>
    /// Configuration for rubble and scrap that will be generated when the block breaks
    /// </summary>
    public class RubbleConfig
    {
        [AutoConfig("WearAndTear/Server/RubbleConfig.json", ServerSync = true)]
        public static RubbleConfig Instance { get; private set; }

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
        [Range(0d, 1d)]
        [DisplayFormat(DataFormatString = "P")]
        public float FixedDropPercentage { get; set; } = .2f;

        /// <summary>
        /// The percentage of content level that will drop based on durability
        /// (Drop = ContentLevel * ((Durability * DurabilityDropPercentage) + FixedDropPercentage)
        /// </summary>
        [DefaultValue(.8f)]
        [Range(0d, 1d)]
        [DisplayFormat(DataFormatString = "P")]
        public float DurabilityDropPercentage { get; set; } = .6f;

        /// <summary>
        /// How much damage you take from sprinting into rubble
        /// </summary>
        [DefaultValue(2f)]
        [Range(0d, double.PositiveInfinity)]
        public float SprintIntoDamage { get; set; } = 2f;

        /// <summary>
        /// How much damage you take from falling onto rubble multiplied by the fall speed
        /// </summary>
        [DefaultValue(4f)]
        [Range(0d, double.PositiveInfinity)]
        public float FallIntoDamageMul { get; set; } = 4f;
    }
}