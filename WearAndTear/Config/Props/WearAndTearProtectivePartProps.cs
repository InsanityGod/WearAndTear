using System;

namespace WearAndTear.Config.Props
{
    public class WearAndTearProtectivePartProps
    {
        /// <summary>
        /// Contains information on what parts will gain protection against which type of decay (and how much)
        /// </summary>
        public WearAndTearProtectiveTargetProps[] EffectiveFor { get; set; } = Array.Empty<WearAndTearProtectiveTargetProps>();
    }
}