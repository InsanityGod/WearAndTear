using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace WearAndTear.Code.Extensions
{
    public static class WeatherExtensions
    {
        public static ClimateCondition GetPastAverageClimateCondition(this IWorldAccessor world, BlockPos pos, double timePassed, double pollInterval)
        {
            var now = world.Calendar.TotalDays;
            if (timePassed == 0) return world.BlockAccessor.GetClimateAt(pos, EnumGetClimateMode.ForSuppliedDate_TemperatureRainfallOnly, now);
            var pollCount = 0;
            var totalRainfall = 0f;
            var totalTemperature = 0f;

            while (timePassed > 0)
            {
                var poll = world.BlockAccessor.GetClimateAt(pos, EnumGetClimateMode.ForSuppliedDate_TemperatureRainfallOnly, now - timePassed);

                totalRainfall += poll.Rainfall;
                totalTemperature += poll.Temperature;

                pollCount++;
                timePassed -= pollInterval;
            }

            return new ClimateCondition
            {
                Rainfall = totalRainfall / pollCount,
                Temperature = totalTemperature / pollCount,
            };
        }
    }
}