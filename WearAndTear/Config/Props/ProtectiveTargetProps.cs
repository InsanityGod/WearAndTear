using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WearAndTear.Config.Props
{
    public class ProtectiveTargetProps
    {
        /// <summary>
        /// Parts with this code will gain this protection
        /// </summary>
        public string Code { get; set; }

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

        public bool IsEffectiveFor(PartProps props)
        {
            if (Code != null && props.Code != Code) return false;
            if (RepairType != null && RepairType != props.RepairType) return false;
            return true;
        }
    }
}