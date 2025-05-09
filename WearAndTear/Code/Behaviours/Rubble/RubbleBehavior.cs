using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace WearAndTear.Code.Behaviours.Rubble
{
    public class RubbleBehavior : BlockBehaviorUnstableFalling
    {
        public RubbleBehavior(Block block) : base(block)
        {
        }

        public void DelayedOnBlockPlaced(IWorldAccessor world, BlockPos blockPos)
        {
            var handling = EnumHandling.PassThrough;
            base.OnBlockPlaced(world, blockPos, ref handling);
        }

        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ref EnumHandling handling)
        {
            //Purposefully empty
        }
    }
}