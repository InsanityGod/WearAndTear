using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Vintagestory.API.Common;
using WearAndTear.Code.Enums;

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

        /// <summary>
        /// The ratio of durability to XP gained when repairing blocks.
        /// (100% durability would mean translate into 10 xp)
        /// </summary>
        [Category("XLib / XSkills")]
        [DefaultValue(10f)]
        [DisplayName("Durability to XP ratio")]
        public float DurabilityToXPRatio { get; set; } = 10f;

        /// <summary>
        /// When enabled you will see rough estimates rather then exact percentages
        /// (if you have XLib, you need to get a skill to see exact values)
        /// </summary>
        [Category("XLib / XSkills")]
        [DefaultValue(EOptionalWithXLib.OnlyWithXLib)]
        public EOptionalWithXLib RoughDurabilityEstimate { get; set; } = EOptionalWithXLib.OnlyWithXLib;
    }
}