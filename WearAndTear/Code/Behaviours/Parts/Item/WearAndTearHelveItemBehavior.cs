using HarmonyLib;
using MechanicalWoodSplitter.Code.FakeStuff;
using MechanicalWoodSplitter.Code.Items;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code.Behaviours.Parts.Abstract;

namespace WearAndTear.Code.Behaviours.Parts.Item
{
    public class WearAndTearHelveItemBehavior : WearAndTearItemPartBehavior
    {
        public readonly BEHelveHammer HelveHammerBase;

        public WearAndTearHelveItemBehavior(BlockEntity blockentity) : base(blockentity)
        {
            HelveHammerBase = (BEHelveHammer)blockentity;
        }

        public override ItemStack ItemStack
        {
            get => HelveHammerBase.HammerStack;
            set => HelveHammerBase.HammerStack = value;
        }

        public override ItemSlot ItemSlot => null;

        public override void DamageItem(int amount = 1)
        {
            var anvil = Traverse.Create(HelveHammerBase).Field("targetAnvil").GetValue<BlockEntityAnvil>();
            if (WearAndTearModSystem.HelveAxeModLoaded && IsHelveAxe)
            {
                if(!ShoulDamageHelveAxe(anvil)) return;
            }
            else if (!WearAndTearModSystem.Config.SpecialParts.DamageHelveHammerEvenIfNothingOnAnvil && anvil.WorkItemStack == null) return;

            base.DamageItem(amount);
        }

        public bool IsHelveAxe => ItemStack?.Collectible is HelveAxe;

        public bool ShoulDamageHelveAxe(object anvil) => anvil is FakeBlockEntityAnvil fakeAnvil && (fakeAnvil.ChoppingBlock?.recipecomplete ?? false);

        public void ManualDamageIem(int amount = 1) => base.DamageItem(amount);
    }
}