using Vintagestory.API.Common;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code.Behaviours.Parts.Abstract;

namespace WearAndTear.Code.Behaviours.Parts.Item
{
    public class WearAndTearPulverizerItemBehavior : WearAndTearItemPartBehavior
    {
        public readonly BEPulverizer pulverizer;

        public WearAndTearPulverizerItemBehavior(BlockEntity blockentity) : base(blockentity)
        {
            pulverizer = (BEPulverizer)blockentity;
        }

        public override ItemSlot ItemSlot => pulverizer.Inventory[2];
    }
}