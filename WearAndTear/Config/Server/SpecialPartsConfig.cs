using InsanityLib.Attributes.Auto.Config;
using System.Collections.Generic;
using System.ComponentModel;
using WearAndTear.Config.Props;

namespace WearAndTear.Config.Server
{
    public class SpecialPartsConfig
    {
        [AutoConfig("WearAndTear/Server/SpecialPartsConfig.json", ServerSync = true)]
        public static SpecialPartsConfig Instance { get; private set; }

        /// <summary>
        /// Whether helve hammers will decay
        /// </summary>
        [DefaultValue(true)]
        public bool HelveHammer { get; set; } = true;

        /// <summary>
        /// Helve Hammer durability mapping
        /// </summary>
        public Dictionary<string, int> HelveHammerDurability { get; set; } = new Dictionary<string, int>()
        {
              {"*-tinbronze",       1500},
              {"*-bismuthbronze",   1800},
              {"*-blackbronze",     2200},
              {"*-iron",            3600},
              {"*-meteoriciron",    4200},
              {"*-steel",           9000}
        };

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
        /// Helve Axe durability mapping
        /// </summary>
        public Dictionary<string, int> HelveAxeDurability { get; set; } = new Dictionary<string, int>()
        {
            {"*-tinbronze",         600 },
            {"*-bismuthbronze",     750 },
            {"*-blackbronze",       900 },
            {"*-iron",              1350},
            {"*-meteoriciron",      1800},
            {"*-steel",             2700}
        };

        /// <summary>
        /// Wether pounders have durability
        /// </summary>
        [DefaultValue(true)]
        public bool Pounder { get; set; } = true;

        /// <summary>
        /// PounderCap durability mapping
        /// </summary>
        public Dictionary<string, int> PounderCapDurability { get; set; } = new Dictionary<string, int>()
        {
            {"*-tinbronze",         750 },
            {"*-bismuthbronze",     900 },
            {"*-blackbronze",       1100},
            {"*-iron",              1800},
            {"*-meteoriciron",      2100},
            {"*-steel",             4500}
        };

        /// <summary>
        /// Wether molds have durability
        /// </summary>
        [DefaultValue(true)]
        public bool Molds { get; set; } = true;

        /// <summary>
        /// The default clutch part
        /// BEWARE: These settings don't actually do anything other then decide wether it's included in AutoPartRegistry for now.
        /// </summary>
        public PartProps Clutch { get; set; } = new()
        {
            Code = "wearandtear:clutch"
        };

        /// <summary>
        /// The default windmill sail part
        /// (You simple set this part to `null` if you want to disable the decay of windmill sails altogether)
        /// </summary>
        public PartProps WindmillSails { get; set; } = new()
        {
            Code = "wearandtear:sail",
            RepairType = "cloth",
            DurabilityEfficiencyRatio = 1,
            Decay = new DecayProps[]
            {
                new()
                {
                    Type = "wind"
                }
            }
        };
    }
}