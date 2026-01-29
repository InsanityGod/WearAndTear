using Vintagestory.API.Common;
using WearAndTear.Code.Behaviours;
using WearAndTear.Config.Props;

namespace WearAndTear.Code.Interfaces;

public interface IDecayEngine
{
    public float GetDecayLoss(ICoreAPI api, Part part, DecayProps decayProps, double daysPassed);
}