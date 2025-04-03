using Vintagestory.API.Common;
using WearAndTear.Code.Extensions;
using WearAndTear.Code.Interfaces;
using WearAndTear.Config.Props;

namespace WearAndTear.Code.DecayEngines
{
    public class HumidityDecayEngine : IDecayEngine
    {
        public float GetDecayLoss(ICoreAPI api, IWearAndTearPart part, WearAndTearDecayProps decayProps, double daysPassed)
        {
            var climate = api.World.GetPastAverageClimateCondition(part.Pos, daysPassed, WearAndTearModSystem.Config.PollIntervalInDays);

            //Relative to the average AvgLifeSpanInYears expects
            var degradationFactor = climate.Rainfall / .25;

            // Calculate degradation factor (adjusted to daily wear rate)
            double degradationRate = degradationFactor / (part.Props.AvgLifeSpanInYears * api.World.Calendar.DaysPerYear);

            if (part.WearAndTear.IsSheltered) degradationRate *= .5;

            return (float)(degradationRate * daysPassed) * WearAndTearModSystem.Config.DecayModifier.Humidity;
        }
    }
}