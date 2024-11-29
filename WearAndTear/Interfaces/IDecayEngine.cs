using Vintagestory.API.Common;
using WearAndTear.Behaviours;
using WearAndTear.Config.Props;

namespace WearAndTear.Interfaces
{
    public interface IDecayEngine
    {
        public float GetDecayLoss(ICoreAPI api, WearAndTearPartBehavior part, WearAndTearDecayProps decayProps, double daysPassed);
    }
}