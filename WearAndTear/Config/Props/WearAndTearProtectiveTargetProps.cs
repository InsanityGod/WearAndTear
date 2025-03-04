using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WearAndTear.Config.Props
{
    public class WearAndTearProtectiveTargetProps
    {
        /// <summary>
        /// Parts with this name will gain this protection
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Parts of this repair type will gain this protection
        /// </summary>
        public string RepairType { get; set; }

        /// <summary>
        /// Multiplier on how much damage is done
        /// </summary>
        [DefaultValue(0.5f)]
        [Range(0, 1)]
        public float DecayMultiplier { get; set; } = .5f;

        public bool IsEffectiveFor(WearAndTearPartProps props) => props.Name == Name || props.RepairType == RepairType;
    }
}