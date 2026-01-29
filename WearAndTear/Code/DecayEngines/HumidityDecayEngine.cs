using Vintagestory.API.Common;
using WearAndTear.Code.Behaviours;
using WearAndTear.Code.Extensions;
using WearAndTear.Code.Interfaces;
using WearAndTear.Config.Props;
using WearAndTear.Config.Server;

namespace WearAndTear.Code.DecayEngines;

public class HumidityDecayEngine : IDecayEngine
{
    public float GetDecayLoss(ICoreAPI api, Part part, DecayProps decayProps, double daysPassed)
    {
        var climate = api.World.GetPastAverageClimateCondition(part.Pos, daysPassed, WearAndTearServerConfig.Instance.PollIntervalInDays);

        //Relative to the average AvgLifeSpanInYears expects
        var degradationFactor = climate.Rainfall / .25;

        // Calculate degradation factor (adjusted to daily wear rate)
        double degradationRate = degradationFactor / (part.Props.AvgLifeSpanInYears * api.World.Calendar.DaysPerYear);

        if (part.Controller.IsSheltered) degradationRate *= .5;

        return (float)(degradationRate * daysPassed) * DecayModifiersConfig.Instance.Humidity;
    }
}