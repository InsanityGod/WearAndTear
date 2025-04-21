using System;

namespace WearAndTear.Code.Extensions
{
    public static class NumberExtensions
    {
        public static string ToPercentageString(this float percentage) => $"{(int)(Math.Round(percentage, 2) * 100)}%";
    }
}