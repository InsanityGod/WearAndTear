using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WearAndTear.Config.Props.RegistryTemplates
{
    public class ReinforcementTemplate
    {
        /// <summary>
        /// This is used to fill the multiplier of the protective part of the Metal Reinforcement Template
        /// </summary>
        [DefaultValue(.95f)]
        [Range(0d, 1d)]
        [DisplayFormat(DataFormatString = "P")]
        public float DecayMultiplier { get; set; } = .95f;

        /// <summary>
        /// This is used to fill the average life span of the Metal Reinforcement Template
        /// </summary>
        [DefaultValue(2)]
        [Range(0d, double.PositiveInfinity)]
        public float AvgLifeSpanInYears { get; set; } = 2;
    }
}