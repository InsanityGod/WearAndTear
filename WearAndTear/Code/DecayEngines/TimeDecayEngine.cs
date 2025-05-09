using Vintagestory.API.Common;
using WearAndTear.Code.Behaviours;
using WearAndTear.Code.Interfaces;
using WearAndTear.Config.Props;
using WearAndTear.Config.Server;

namespace WearAndTear.Code.DecayEngines
{
    public class TimeDecayEngine : IDecayEngine
    {
        public float GetDecayLoss(ICoreAPI api, Part part, DecayProps decayProps, double daysPassed)
        {
            double degradationRate = 1 / (part.Props.AvgLifeSpanInYears * api.World.Calendar.DaysPerYear);

            if (part.Controller.IsSheltered) degradationRate *= .5;

            return (float)(degradationRate * daysPassed) * DecayModifiersConfig.Instance.Time;
        }
    }
}