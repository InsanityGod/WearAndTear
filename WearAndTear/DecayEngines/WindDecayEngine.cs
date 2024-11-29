using Vintagestory.API.Common;
using WearAndTear.Behaviours;
using WearAndTear.Config.Props;
using WearAndTear.Interfaces;

namespace WearAndTear.DecayEngines
{
    public class WindDecayEngine : IDecayEngine
    {
        public float GetDecayLoss(ICoreAPI api, WearAndTearPartBehavior part, WearAndTearDecayProps decayProps, double daysPassed)
        {
            var avgWindSpeed = .5;

            if (daysPassed < 1)
            {
                //HACK: haven't been able to figure out how to get the average windspeed over the passed period
                //(this is to prevent massive damage when returning from a long trip while it's very windy)

                //TODO test
                avgWindSpeed = api.World.BlockAccessor.GetWindSpeedAt(part.Pos).Length();
            }

            var degradationFactor = avgWindSpeed / 0.5;

            // Calculate degradation factor (adjusted to daily wear rate)
            double degradationRate = degradationFactor / (part.Props.AvgLifeSpanInYears * api.World.Calendar.DaysPerYear);

            return (float)(degradationRate * daysPassed);
        }
    }
}