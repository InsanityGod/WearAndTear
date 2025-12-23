using InsanityLib.Auto.Config;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WearAndTear.Config.Server
{
    public class DecayModifiersConfig
    {
        [AutoConfig("WearAndTear/Server/DecayModifierConfig.json", ServerSync = true)]
        public static DecayModifiersConfig Instance { get; private set; }

        /// <summary>
        /// Multiplier on decay (damage) caused by wind
        /// </summary>
        [DefaultValue(1)]
        [Range(0d, double.PositiveInfinity)]
        public float Wind { get; set; } = 1f;

        /// <summary>
        /// Multiplier on decay (damage) caused by humidity (rainfall)
        /// </summary>
        [DefaultValue(1)]
        [Range(0d, double.PositiveInfinity)]
        public float Humidity { get; set; } = 1f;

        /// <summary>
        /// Multiplier on decay (damage) caused by the passing of time (Time decay is usually a secondary decay type)
        /// </summary>
        [DefaultValue(1)]
        [Range(0d, double.PositiveInfinity)]
        public float Time { get; set; } = 1f;

        /// <summary>
        /// Multiplier on decay of molds
        /// </summary>
        [DefaultValue(1)]
        [Range(0d, double.PositiveInfinity)]
        public float Mold { get; set; } = 1f;
        
        /// <summary>
        /// Wether decay should happen while the chunk is unloaded.
        /// </summary>
        [DefaultValue(true)]
        public bool DecayWhileUnloaded { get; set; } = true;
    }
}