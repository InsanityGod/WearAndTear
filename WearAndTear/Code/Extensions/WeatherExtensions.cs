using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace WearAndTear.Code.Extensions
{
    public static class WeatherExtensions
    {
        public static ClimateCondition GetPastAverageClimateCondition(this IWorldAccessor world, BlockPos pos, double timePassed, double pollInterval)
        {
            var now = world.Calendar.TotalDays;
            if (timePassed <= 0) return world.BlockAccessor.GetClimateAt(pos, EnumGetClimateMode.ForSuppliedDate_TemperatureRainfallOnly, now);
            var pollCount = 0;
            var totalRainfall = 0f;
            var totalTemperature = 0f;

            while (timePassed > 0)
            {
                var poll = world.BlockAccessor.GetClimateAt(pos, EnumGetClimateMode.ForSuppliedDate_TemperatureRainfallOnly, now - timePassed);

                if(!float.IsNaN(poll.Rainfall) && !float.IsNaN(poll.Temperature))
                {
                    totalRainfall += poll.Rainfall;
                    totalTemperature += poll.Temperature;
                    pollCount++;
                }
                else if(WearAndTearModSystem.Config.EnableDebugLogging)
                {
                    world.Logger.Warning($"WearAndTear: Invalid climate data at {pos} for totalDays {now - timePassed} (skipping poll)");
                }
                
                timePassed -= pollInterval;
            }

            if (pollCount == 0)
            {
                if(WearAndTearModSystem.Config.EnableDebugLogging) world.Logger.Warning("WearAndTear: totally failed to poll climate data (returning default template)");

                return new ClimateCondition
                {
                    Rainfall = 0.5f,
                    Temperature = 15f,
                };
            }

            return new ClimateCondition
            {
                Rainfall = totalRainfall / pollCount,
                Temperature = totalTemperature / pollCount,
            };
        }
    }
}