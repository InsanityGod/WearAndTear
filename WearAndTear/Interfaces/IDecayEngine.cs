using Vintagestory.API.Common;
using WearAndTear.Behaviours;
using WearAndTear.Config.Props;

namespace WearAndTear.Interfaces
{
    public interface IDecayEngine
    {
        public float GetDecayLoss(ICoreAPI api, IWearAndTearPart part, WearAndTearDecayProps decayProps, double daysPassed);
    }
}