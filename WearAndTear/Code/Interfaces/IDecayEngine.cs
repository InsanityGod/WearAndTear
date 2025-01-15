using Vintagestory.API.Common;
using WearAndTear.Config.Props;

namespace WearAndTear.Code.Interfaces
{
    public interface IDecayEngine
    {
        public float GetDecayLoss(ICoreAPI api, IWearAndTearPart part, WearAndTearDecayProps decayProps, double daysPassed);
    }
}