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

        public bool WindmillRotor { get; set; } = true;

        public string[] GenericBlacklist { get; set; }

        //TODO maybe include too friction being able to cause friction?
    }
}
