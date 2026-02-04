using Vintagestory.GameContent;
using WearAndTear.Code.Enums;

namespace WearAndTear.Config;

//TODO expand and move to InsanityLib
public class TraitConfig : Trait
{
    public string[] AppendToClasses { get; set; }

    public bool OnlyWithTraitRequirementEnabled { get; set; }

    public EXLibPrescenceRequirement XLibPresenceRequirement { get; set; } = EXLibPrescenceRequirement.Irrelevant;
}