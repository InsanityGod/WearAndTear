using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.GameContent;
using WearAndTear.Code.Enums;

namespace WearAndTear.Config
{
    public class TraitConfig : Trait
    {
        public string[] AppendToClasses { get; set; }

        public bool OnlyWithTraitRequirementEnabled { get; set; }

        public EXLibPrescenceRequirement XLibPresenceRequirement { get; set; } = EXLibPrescenceRequirement.Irrelevant;
    }
}
