using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using WearAndTear.Code.Behaviours.Parts.Abstract;
using WearAndTear.Code.Interfaces;
using WearAndTear.Config.Props;

namespace WearAndTear.Code.Behaviours.Parts.Protective
{
    public class OptionalProtectivePart : ProtectivePart, IOptionalPart
    {
        public OptionalProtectivePart(BlockEntity blockentity) : base(blockentity) { }

        public virtual bool IsPresent => Durability != 0;

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            Durability = Props.IsCritical ? 1 : 0; //Only present by default if IsCritical
        }

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