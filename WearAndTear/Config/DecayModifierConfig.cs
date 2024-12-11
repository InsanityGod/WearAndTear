using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WearAndTear.Config
{
    public class DecayModifierConfig
    {
        /// <summary>
        /// Decay caused by wind
        /// </summary>
        public float Wind { get; set; } = 1;
        
        /// <summary>
        /// Decay caused by humidity (rainfall)
        /// </summary>
        public float Humidity { get; set; } = 1;

        /// <summary>
        /// Decay caused by the passing of time 
        /// (Time decay is usually a secondary decay type)
        /// </summary>
        public float Time { get; set; } = 1;
    }
}
