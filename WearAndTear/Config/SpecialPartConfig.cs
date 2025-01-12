using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.MathTools;
using WearAndTear.Behaviours.Parts.Item;
using WearAndTear.Config.Props;

namespace WearAndTear.Config
{
    public class SpecialPartConfig
    {
        /// <summary>
        /// Whether helve hammers will decay
        /// </summary>
        [DefaultValue(true)]
        public bool HelveHammer { get; set; } = true;

        /// <summary>
        /// If set to false, the helve hammer will only be damaged if there is actually something on the anvil
        /// </summary>
        [DefaultValue(true)]
        public bool DamageHelveHammerEvenIfNothingOnAnvil { get; set; } = true;
        
        /// <summary>
        /// Whether helve axes will decay
        /// </summary>
        [DefaultValue(true)]
        public bool HelveAxe { get; set; } = true;

        /// <summary>
        /// Wether pounders have durability
        /// </summary>
        [DefaultValue(true)] 
        public bool Pounder { get; set; } = true;

        /// <summary>
        /// The default clutch part
        /// BEWARE: Setting this doesn't currently do anything other making it be included in AutoPartRegistry
        /// TODO: Planning on having a custom damage type from engaging at very high speeds
        /// </summary>
        public WearAndTearPartProps Clutch { get; set; } = new();

        /// <summary>
        /// The default windmill sail part
        /// (You simple set this part to `null` if you want to disable the decay of windmill sails altogether)
        /// </summary>
        public WearAndTearPartProps WindmillSails { get; set; } = new()
        {
            Name = "Sail",
            RepairType = "cloth",
            DurabilityEfficiencyRatio = 1,
            Decay = new WearAndTearDecayProps[]
            {
                new()
                {
                    Type = "wind"
                }
            }
        };

        /// <summary>
        /// Wether decayed windmills will have decayed models (look like they have are torn sails)
        /// </summary>
        [DefaultValue(true)]
        public bool WindmillRotoDecayedAppearance { get; set;} = true;

        /// <summary>
        /// Wether missing shapes (for showing decay in model if `WindmillRotoDecayedAppearance` is set to true) should be auto generated
        /// (Will still attempt to remove cloth if fully broken)
        /// </summary>
        [DefaultValue(true)]
        public bool WindmillRotorDecayAutoGenShapes { get; set;} = true;

        //TODO maybe include too friction being able to cause fire?
    }
}
