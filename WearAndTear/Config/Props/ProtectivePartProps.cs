using System;

namespace WearAndTear.Config.Props
{
    public class ProtectivePartProps
    {
        /// <summary>
        /// Contains information on what parts will gain protection against which type of decay (and how much)
        /// </summary>
        public ProtectiveTargetProps[] EffectiveFor { get; set; } = Array.Empty<ProtectiveTargetProps>();
    }
}