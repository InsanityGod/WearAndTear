using Vintagestory.API.Common;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code.Behaviours.Parts.Abstract;

namespace WearAndTear.Code.Behaviours.Parts.Item
{
    public class PulverizerItemPart : ItemPart
    {
        public readonly BEPulverizer Pulverizer;

        public PulverizerItemPart(BlockEntity blockentity) : base(blockentity)
        {
            Pulverizer = (BEPulverizer)Blockentity;
        }

        public override void OnGeneraterRubble(ref ItemStack[] drops)
        {
            if (Blockentity is BEPulverizer pulverizer)
            {
                pulverizer.hasLPounder = false;
                pulverizer.hasRPounder = false;
                pulverizer.hasAxle = false;
            }

            base.OnGeneraterRubble(ref drops);
        }

        public override ItemSlot ItemSlot => Pulverizer.Inventory[2];
    }
}