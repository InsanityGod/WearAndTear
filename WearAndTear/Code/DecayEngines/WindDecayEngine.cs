using Vintagestory.API.Common;
using WearAndTear.Code.Behaviours;
using WearAndTear.Code.Interfaces;
using WearAndTear.Config.Props;
using WearAndTear.Config.Server;

namespace WearAndTear.Code.DecayEngines;

public class WindDecayEngine : IDecayEngine
{
    public float GetDecayLoss(ICoreAPI api, Part part, DecayProps decayProps, double daysPassed)
    {
        //No wind decay inside
        if (part.Controller.IsSheltered) return 0;

        var avgWindSpeed = .5;

        if (daysPassed < 1)
        {
            //HACK: haven't been able to figure out how to get the average windspeed over the passed period
            //(this is to prevent massive damage when returning from a long trip while it's very windy)

            avgWindSpeed = api.World.BlockAccessor.GetWindSpeedAt(part.Pos).Length();
        }

        //recentering base point
        var degradationFactor = avgWindSpeed / .5;

        // Calculate degradation factor (adjusted to daily wear rate)
        double degradationRate = degradationFactor / (part.Props.AvgLifeSpanInYears * api.World.Calendar.DaysPerYear);

        return (float)(degradationRate * daysPassed) * DecayModifiersConfig.Instance.Wind;
    }
}