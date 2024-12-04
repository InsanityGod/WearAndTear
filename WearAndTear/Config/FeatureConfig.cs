using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WearAndTear.Config
{
    public class FeatureConfig
    {
        public bool HelveHammer { get; set; } = true;
        
        public bool HelveAxe { get; set; } = true;

        public bool AngledGear { get; set; } = true;

        public bool LargeGear { get; set; } = true;

        public bool Toggle { get; set; } = true;

        //TODO the two bellow should have some form of penalty from trying to engage them at high speeds
        public bool Transmission { get; set; } = true;

        public bool Clutch { get; set; } = true;

        public bool WindmillRotor { get; set; } = true;

        //TODO maybe include too friction being able to cause friction?
    }
}
