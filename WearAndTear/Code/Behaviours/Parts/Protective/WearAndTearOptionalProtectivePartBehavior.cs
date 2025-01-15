using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using WearAndTear.Code.Behaviours.Parts.Abstract;
using WearAndTear.Code.Interfaces;
using WearAndTear.Config.Props;

namespace WearAndTear.Code.Behaviours.Parts.Protective
{
    public class WearAndTearOptionalProtectivePartBehavior : WearAndTearOptionalPartBehavior, IWearAndTearProtectivePart
    {
        public WearAndTearOptionalProtectivePartBehavior(BlockEntity blockentity) : base(blockentity)
        {
            //Optional Protective parts don't start with any durability
            Durability = 0;
        }

        public WearAndTearProtectivePartProps ProtectiveProps { get; private set; }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            ProtectiveProps = properties.AsObject<WearAndTearProtectivePartProps>() ?? new();
        }
    }
}