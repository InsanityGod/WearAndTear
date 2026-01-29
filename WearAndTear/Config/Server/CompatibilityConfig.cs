using InsanityLib.Auto.Config;
using InsanityLib.Generators.Attributes;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WearAndTear.Code.Enums;

namespace WearAndTear.Config.Server;

public class CompatibilityConfig
{
    [AutoConfig("WearAndTear/Server/CompatibilityConfig.json", ServerSync = true)]
    public static CompatibilityConfig Instance { get; internal set; }

    /// <summary>
    /// Multiplier on the lifespan of encased parts.
    /// (Keep in mind that you can't do maintenance on encased parts due to their inaccessibility)
    /// </summary>
    [Category("Axle in Blocks")]
    [DefaultValue(1.5f)]
    [Range(0.1d, double.PositiveInfinity)]
    public float EncasedPartLifeSpanMultiplier { get; set; } = 1.5f;

    /// <summary>
    /// The ratio of durability to XP gained when repairing blocks.
    /// (100% durability would mean translate into 10 xp)
    /// </summary>
    [Category("XLib / XSkills")]
    [DefaultValue(10f)]
    [DisplayName("Durability to XP ratio")]
    public float DurabilityToXPRatio { get; set; } = 10f;

    /// <summary>
    /// When enabled you will see rough estimates rather then exact percentages
    /// (if you have XLib, you need to get a skill to see exact values otherwise you will need the "wearandtear-precisemeasurements" trait)
    /// </summary>
    [Category("XLib / XSkills")]
    [DefaultValue(EXLibPrescenceRequirement.Irrelevant)]
    public EXLibPrescenceRequirement RoughDurabilityEstimate { get; set; } = EXLibPrescenceRequirement.Irrelevant;

    /// <summary>
    /// Enables/Disabled extra code for handling old WearAndTear save data
    ///
    /// Disabling this will potentially increase performance but at the cost of causing old save data to be ignored
    /// </summary>
    [DefaultValue(true)]
    public bool LegacyCompatibility { get; set; } = true;
}