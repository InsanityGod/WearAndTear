using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Behaviours.Parts.Abstract;

namespace WearAndTear.Behaviours.Parts.Item
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
