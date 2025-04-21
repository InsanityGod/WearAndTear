using System;
using WearAndTear.Code.XLib.Containers;
using WearAndTear.Config.Props;

namespace WearAndTear.Code.Interfaces
{
    public interface IWearAndTearProtectivePart : IWearAndTearPart
    {
        //By default protective parts don't affect efficiency
        //TODO maybe have lubrication as a protective part and allow for it to increase efficiency / reduce resistance
        float? IWearAndTearPart.EfficiencyModifier => null;

        WearAndTearProtectivePartProps ProtectiveProps { get; }

        public float GetDecayMultiplierFor(WearAndTearPartProps props)
        {
            var protection = Array.Find(ProtectiveProps.EffectiveFor, target => target.IsEffectiveFor(props));
            if (protection != null)
            {
                var mult = protection.DecayMultiplier;

                if (PartBonuses != null) mult = 1 + ((mult - 1) * PartBonuses.ProtectionModifier);

                return mult;
            }
            return 1f;
        }
    }
}