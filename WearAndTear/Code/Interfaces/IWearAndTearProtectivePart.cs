using WearAndTear.Config.Props;

namespace WearAndTear.Code.Interfaces
{
    public interface IWearAndTearProtectivePart : IWearAndTearPart
    {
        //By default protective parts don't affect efficiency
        //TODO maybe have lubrication as a protective part and allow for it to increase efficiency / reduce resistance
        float? IWearAndTearPart.EfficiencyModifier => null;

        WearAndTearProtectivePartProps ProtectiveProps { get; }
    }
}