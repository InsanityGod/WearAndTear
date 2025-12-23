using System.Text;
using Vintagestory.API.Common;
using WearAndTear.Code.Interfaces;

namespace WearAndTear.Code.Behaviours.Parts.Abstract
{
    public abstract class OptionalPart : Part, IOptionalPart
    {
        protected OptionalPart(BlockEntity blockentity) : base(blockentity)
        {
            _durability = 0;
        }

        public virtual bool IsPresent => Durability != 0;

        public override void GetWearAndTearInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            if (IsPresent) base.GetWearAndTearInfo(forPlayer, dsc);
        }

        public override void UpdateDecay(double daysPassed)
        {
            if (IsPresent) base.UpdateDecay(daysPassed);
        }
    }
}