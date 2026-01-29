using System;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using WearAndTear.Code.Interfaces;
using WearAndTear.Config.Props;

namespace WearAndTear.Code.Behaviours.Parts.Protective;

public class ProtectivePart : Part, IProtectivePart
{
    public ProtectivePart(BlockEntity blockentity) : base(blockentity)
    {
    }

    public ProtectivePartProps ProtectiveProps { get; private set; }

    public float GetDecayMultiplierFor(PartProps props)
    {
        var protection = Array.Find(ProtectiveProps.EffectiveFor, target => target.IsEffectiveFor(props));
        if (protection != null)
        {
            var mult = protection.DecayMultiplier;

            if (Bonuses != null) mult = 1 + ((mult - 1) * Bonuses.ProtectionModifier);

            return mult;
        }
        return 1f;
    }

    public override void Initialize(ICoreAPI api, JsonObject properties)
    {
        base.Initialize(api, properties);
        ProtectiveProps = properties.AsObject<ProtectivePartProps>() ?? new();
    }
}