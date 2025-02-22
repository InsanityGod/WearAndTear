using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using WearAndTear.Code.Enums;
using WearAndTear.Code.XLib;

namespace WearAndTear.Code.Extensions
{
    public static class EnumExtensions
    {
        public static bool IsRoughEstimateEnabled(this EOptionalWithXLib optional, ICoreAPI api, IPlayer player)
        {
            var enabled = optional == EOptionalWithXLib.Enabled || (optional == EOptionalWithXLib.OnlyWithXLib && WearAndTearModSystem.XlibEnabled);

            if(enabled && WearAndTearModSystem.XlibEnabled) return !SkillsAndAbilities.HasPreciseMeasurementsSkill(api, player);
            return enabled;
        }
    }
}
