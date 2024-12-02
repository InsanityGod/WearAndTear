using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Behaviours.Parts.Abstract;

namespace WearAndTear.Behaviours.Parts
{
    //TODO create a base class for Item parts
    public class WearAndTearHelveItemBehavior : WearAndTearOptionalPartBehavior
    {
        public readonly BEHelveHammer HelveHammerBase;

        public WearAndTearHelveItemBehavior(BlockEntity blockentity) : base(blockentity)
        {
            HelveHammerBase = (BEHelveHammer)blockentity;
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            //Empty on purpose
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            //Empty on purpose
        }

        public override void UpdateDecay(double daysPassed)
        {
            //Empty on purpose
        }

        public override float Durability
        {
            get
            {
                if (HelveHammerBase.HammerStack == null) return 0;
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

        public override void GetWearAndTearInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            if (!ItemCanBeDamaged) return;

            dsc.AppendLine($"{HelveHammerBase.HammerStack.GetName()} {Lang.Get("Durability: {0} / {1}",
                HelveHammerBase.HammerStack.Collectible.GetRemainingDurability(HelveHammerBase.HammerStack),
                HelveHammerBase.HammerStack.Collectible.GetMaxDurability(HelveHammerBase.HammerStack)
            )}");
        }
    }
}