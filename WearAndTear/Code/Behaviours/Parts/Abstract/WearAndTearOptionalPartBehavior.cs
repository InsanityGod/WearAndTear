using System.Text;
using Vintagestory.API.Common;
using WearAndTear.Code.Interfaces;

namespace WearAndTear.Code.Behaviours.Parts.Abstract
{
    public abstract class WearAndTearOptionalPartBehavior : WearAndTearPartBehavior, IWearAndTearOptionalPart
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
            if (!IsPresent) return;
            base.UpdateDecay(daysPassed);
        }
    }
}