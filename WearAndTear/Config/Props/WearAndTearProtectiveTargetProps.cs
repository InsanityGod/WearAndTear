using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WearAndTear.Config.Props
{
    public class WearAndTearProtectiveTargetProps
    {
        public string Name { get; set; }

        public string RepairType { get; set; }

        public float DecayMultiplier { get; set; } = .5f;

        public bool IsEffectiveFor(WearAndTearPartProps props) => props.Name == Name || props.RepairType == RepairType;
    }
}
