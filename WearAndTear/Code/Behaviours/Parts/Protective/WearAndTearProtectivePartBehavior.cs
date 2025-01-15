using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using WearAndTear.Code.Interfaces;
using WearAndTear.Config.Props;

namespace WearAndTear.Code.Behaviours.Parts.Protective
{
    public class WearAndTearProtectivePartBehavior : WearAndTearPartBehavior, IWearAndTearProtectivePart
    {
        public WearAndTearProtectivePartBehavior(BlockEntity blockentity) : base(blockentity)
        {
        }

        public WearAndTearProtectivePartProps ProtectiveProps { get; private set; }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            ProtectiveProps = properties.AsObject<WearAndTearProtectivePartProps>() ?? new();
        }
    }
}