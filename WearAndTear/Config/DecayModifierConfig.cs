using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WearAndTear.Config
{
    public class DecayModifierConfig
    {
        /// <summary>
        /// Multiplier on decay (damage) caused by wind
        /// </summary>
        [DefaultValue(1)]
        [Range(0, float.PositiveInfinity)]
        public float Wind { get; set; } = 1;

        /// <summary>
        /// Multiplier on decay (damage) caused by humidity (rainfall)
        /// </summary>
        [DefaultValue(1)]
        [Range(0, float.PositiveInfinity)]
        public float Humidity { get; set; } = 1;

        /// <summary>
        /// Multiplier on decay (damage) caused by the passing of time
        /// (Time decay is usually a secondary decay type)
        /// </summary>
        [DefaultValue(1)]
        [Range(0, float.PositiveInfinity)]
        public float Time { get; set; } = 1;
    }
}