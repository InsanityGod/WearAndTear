using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using WearAndTear.Config.Props;
using WearAndTear.Interfaces;

namespace WearAndTear.DecayEngines
{
    //TODO look into wood decay texture
    public class TimeDecayEngine : IDecayEngine
    {
        public float GetDecayLoss(ICoreAPI api, IWearAndTearPart part, WearAndTearDecayProps decayProps, double daysPassed)
        {
            double degradationRate = 1 / (part.Props.AvgLifeSpanInYears * api.World.Calendar.DaysPerYear);

            if(part.WearAndTear.IsSheltered) degradationRate *= .5;

            return (float)(degradationRate * daysPassed) * WearAndTearModSystem.Config.DecayModifier.Time;
        }
    }
}
