using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent.Mechanics;

namespace WearAndTear.Behaviours
{
    public class WearAndTearHelveHammerBlockEntityBehavior : WearAndTearBlockEntityBehavior
    {
        protected override bool RunUpdateMethod => false;

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            //Empty on purpose
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            //Empty on purpose
        }

        public readonly BEHelveHammer HelveHammerBase;

        public WearAndTearHelveHammerBlockEntityBehavior(BlockEntity blockentity) : base(blockentity)
        {
            HelveHammerBase = (BEHelveHammer)blockentity;
        }

        public override float Durability
        {
            get
            {
                if (HelveHammerBase.HammerStack == null) return 1;
                var maxDurability = HelveHammerBase.HammerStack.Collectible.GetMaxDurability(HelveHammerBase.HammerStack);
                var durability = HelveHammerBase.HammerStack.Attributes.GetInt("durability", maxDurability);
                return durability / (float)maxDurability;
            }
            set
            {
                var item = HelveHammerBase.HammerStack?.Collectible;
                if (item == null) return;
                var maxDurability = item.GetMaxDurability(HelveHammerBase.HammerStack);
                HelveHammerBase.HammerStack.Attributes.SetInt("durability", (int)Math.Ceiling(maxDurability * value));
            }
        }

        public bool ItemCanBeDamaged => (HelveHammerBase.HammerStack?.Collectible?.GetMaxDurability(HelveHammerBase.HammerStack) ?? 0) > 0;

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            if (!ItemCanBeDamaged) return;
            base.GetBlockInfo(forPlayer, dsc);
        }
    }
}