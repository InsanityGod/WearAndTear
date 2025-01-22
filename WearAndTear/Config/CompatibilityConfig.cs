﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public float EncasedPartLifeSpanMultiplier { get; set; } = 1.5f;
    }
}
