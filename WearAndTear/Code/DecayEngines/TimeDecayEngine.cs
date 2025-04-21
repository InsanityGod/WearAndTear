using Vintagestory.API.Common;
using WearAndTear.Code.Interfaces;
using WearAndTear.Config.Props;
using WearAndTear.Config.Server;

namespace WearAndTear.Code.DecayEngines
{
    public class TimeDecayEngine : IDecayEngine
    {
        public float GetDecayLoss(ICoreAPI api, IWearAndTearPart part, WearAndTearDecayProps decayProps, double daysPassed)
        {
            double degradationRate = 1 / (part.Props.AvgLifeSpanInYears * api.World.Calendar.DaysPerYear);

            if (part.WearAndTear.IsSheltered) degradationRate *= .5;

            return (float)(degradationRate * daysPassed) * WearAndTearServerConfig.Instance.DecayModifier.Time;
        }
    }
}