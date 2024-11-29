using System;
using Vintagestory.API.Common;
using WearAndTear.Behaviours;
using WearAndTear.Config.Props;
using WearAndTear.Interfaces;

namespace WearAndTear.DecayEngines
{
    public class RainDecayEngine : IDecayEngine
    {
        public float GetDecayLoss(ICoreAPI api, WearAndTearPartBehavior part, WearAndTearDecayProps decayProps, double daysPassed)
        {
            //TODO actually implement this
            throw new NotImplementedException();
        }
    }
}