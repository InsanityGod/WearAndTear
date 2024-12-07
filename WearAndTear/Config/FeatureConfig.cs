using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WearAndTear.Behaviours.Parts.Item;

namespace WearAndTear.Config
{
    public class FeatureConfig
    {
        /// <summary>
        /// Whether helve hammers will decay
        /// </summary>
        public bool HelveHammer { get; set; } = true;
        
        /// <summary>
        /// Whether helve axes will decay
        /// </summary>
        public bool HelveAxe { get; set; } = true;

        /// <summary>
        /// Wether pounders have durability
        /// </summary>
        public bool Pounder { get; set; } = true;

        /// <summary>
        /// Whether windmills will decay
        /// </summary>
        public bool WindmillRotor { get; set; } = true;

        /// <summary>
        /// Wether decayed windmills will have decayed models (look like they have are torn sails)
        /// </summary>
        public bool WindmillRotoDecayedAppearance { get; set;} = true;

        /// <summary>
        /// Wether missing shapes (for showing decay in model if `WindmillRotoDecayedAppearance` is set to true) should be auto generated
        /// (Will still attempt to remove cloth if fully broken)
        /// </summary>
        public bool WindmillRotorDecayAutoGenShapes { get; set;} = true;

        public string[] GenericBlacklist { get; set; }

        //TODO maybe include too friction being able to cause friction?
    }
}
