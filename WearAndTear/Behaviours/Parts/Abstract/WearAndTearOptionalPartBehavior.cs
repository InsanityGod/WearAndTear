using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using WearAndTear.Config.Props;
using WearAndTear.Interfaces;

namespace WearAndTear.Behaviours.Parts.Abstract
{
    public abstract class WearAndTearOptionalPartBehavior : WearAndTearPartBehavior
    {
        protected WearAndTearOptionalPartBehavior(BlockEntity blockentity) : base(blockentity)
        {
        }

        public virtual bool IsPresent => Durability != 0;

        public override void GetWearAndTearInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            if (!IsPresent) return;
            base.GetWearAndTearInfo(forPlayer, dsc);
        }

        public override void UpdateDecay(double daysPassed)
        {
            if(!IsPresent) return;
            base.UpdateDecay(daysPassed);
        }
    }
}
