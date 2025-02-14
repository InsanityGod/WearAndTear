using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WearAndTear.Config
{
    public class CompatibilityConfig
    {
        /// <summary>
        /// Multiplier on the lifespan of encased parts.
        /// (Keep in mind that you can't do maintenance on encased parts due to their inaccessibility)
        /// </summary>
        [Category("Axle in Blocks")]
        [DefaultValue(1.5f)]
        [Range(0.1f, float.PositiveInfinity)]
        public float EncasedPartLifeSpanMultiplier { get; set; } = 1.5f;
    }
}