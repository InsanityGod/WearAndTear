using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
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

        public override ItemSlot ItemSlot => Pulverizer.Inventory[2];
    }
}