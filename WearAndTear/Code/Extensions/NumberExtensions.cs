using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WearAndTear.Code.Extensions
{
    public static class NumberExtensions
    {
        public static string ToPercentageString(this float percentage) => $"{(int)(percentage * 100)}%";
    }
}
