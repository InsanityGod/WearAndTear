using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using WearAndTear.Code.Enums;
using WearAndTear.Code.XLib;

namespace WearAndTear.Code.Extensions
{
    public static class EnumExtensions
    {
        public static bool IsFullfilled(this EXLibPrescenceRequirement requirement) => requirement switch
        {
            EXLibPrescenceRequirement.Disabled => false,
            EXLibPrescenceRequirement.OnlyWithXLib => WearAndTearModSystem.XlibEnabled,
            EXLibPrescenceRequirement.OnlyWithoutXLib => !WearAndTearModSystem.XlibEnabled,
            EXLibPrescenceRequirement.Irrelevant => true,
            _ => false,
        };
    }
}
