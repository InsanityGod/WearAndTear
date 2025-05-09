using System;
using WearAndTear.Code.XLib.Containers;
using WearAndTear.Config.Props;

namespace WearAndTear.Code.Interfaces
{
    public interface IProtectivePart
    {
        float GetDecayMultiplierFor(PartProps props);
    }
}