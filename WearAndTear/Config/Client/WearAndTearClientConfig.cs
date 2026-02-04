using InsanityLib.Auto.Config;
using InsanityLib.Generators.Attributes;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WearAndTear.Config.Client;

public class WearAndTearClientConfig
{
    [AutoConfig("WearAndTear/Client/MainConfig.json")]
    public static WearAndTearClientConfig Instance { get; internal set; }

    //Actual Config

    /// <summary>
    /// If the durability drops bellow this amount of durability you will see visual tearing
    /// (set to 0 to disable entirely)
    /// </summary>
    [Category("Appearance")]
    [DefaultValue(0.6f)]
    [Range(0d, 1d)]
    [DisplayFormat(DataFormatString = "P")]
    public float VisualTearingMinDurability { get; set; } = 0.6f;

    /// <summary>
    /// Wether visual tearing should be disabled on MP blocks
    /// (MP blocks generally move/turn and since the tearing doesn't it will end up looking weirdly)
    /// </summary>
    [Category("Appearance")]
    [DefaultValue(true)]
    [DisplayName("Disable Visual Tearing on MP Blocks")]
    public bool DisableVisualTearingOnMPBlocks { get; set; } = true;

    /// <summary>
    /// Wether decayed windmills will have decayed models (look like they have are torn sails)
    /// </summary>
    [Category("Appearance")]
    [DefaultValue(true)]
    public bool WindmillRotoDecayedAppearance { get; set; } = true;

    /// <summary>
    /// Wether missing shapes (for showing decay in model if `WindmillRotoDecayedAppearance` is set to true) should be auto generated
    /// (Will still attempt to remove cloth if fully broken)
    /// </summary>
    [Category("Appearance")]
    [DefaultValue(true)]
    public bool WindmillRotorDecayAutoGenShapes { get; set; } = true;

    /// <summary>
    /// If enabled, questionable events are logged (like when temperature data is invalid and creates NaN as a value)
    /// </summary>
    [Category("Debug")]
    [DefaultValue(false)]
    public bool EnableDebugLogging { get; set; } = false;
}