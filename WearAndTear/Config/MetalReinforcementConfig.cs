using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WearAndTear.Config
{
    public class MetalReinforcementConfig
    {
        /// <summary>
        /// This is used to fill the multiplier of the protective part of the Metal Reinforcement Template
        /// </summary>
        [DefaultValue(.95f)]
        [Range(0, 1)]
        [DisplayFormat(DataFormatString = "P")]
        public float DecayMultiplier { get; set; } = .95f;

        /// <summary>
        /// This is used to fill the average life span of the Metal Reinforcement Template
        /// </summary>
        [DefaultValue(2)]
        [Range(0, float.PositiveInfinity)]
        public float AvgLifeSpanInYears { get; set; } = 2;
    }
}
