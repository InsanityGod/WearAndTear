using System;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using WearAndTear.Behaviours;
using WearAndTear.Config.Props;
using WearAndTear.Extensions;
using WearAndTear.Interfaces;

namespace WearAndTear.DecayEngines
{
    public class HumidityDecayEngine : IDecayEngine
    {
        //TODO wood having issues with low humidity?
        //TODO look at changing humidity
        public float GetDecayLoss(ICoreAPI api, IWearAndTearPart part, WearAndTearDecayProps decayProps, double daysPassed)
        {
            var climate = api.World.GetPastAverageClimateCondition(part.Pos, daysPassed, WearAndTearModSystem.Config.PollIntervalInDays);
            
            //Relative to the average AvgLifeSpanInYears expects
            var degradationFactor = climate.Rainfall / .25;

            // Calculate degradation factor (adjusted to daily wear rate)
            double degradationRate = degradationFactor / (part.Props.AvgLifeSpanInYears * api.World.Calendar.DaysPerYear);

            if(part.WearAndTear.IsSheltered) degradationRate *= .5;

            return (float)(degradationRate * daysPassed) * WearAndTearModSystem.Config.DecayModifier.Humidity;
        }
    }
}