using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using WearAndTear.Config.Props;
using WearAndTear.Interfaces;

namespace WearAndTear.Behaviours
{
    public class WearAndTearRepairItemBehavior : CollectibleBehavior
    {
        public WearAndTearRepairItemBehavior(CollectibleObject collObj) : base(collObj)
        {
        }

        public WearAndTearRepairItemProps Props { get; private set; }

        public override void Initialize(JsonObject properties)
        {
            base.Initialize(properties);
            Props = properties.AsObject<WearAndTearRepairItemProps>() ?? new();
            //TODO maybe a consumeDurability mode?
        }

        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandling handling)
        {
            if (secondsUsed < .1f)
            {
                handling = EnumHandling.PreventDefault;
                return true;
            }

            return false;
        }

        public override bool OnHeldInteractCancel(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, EnumItemUseCancelReason cancelReason, ref EnumHandling handled)
        {
            var wearAndTear = slot.Inventory.Api.World.BlockAccessor.GetBlockEntity(blockSel.Position)?.GetBehavior<IWearAndTear>();

            if (wearAndTear != null && wearAndTear.IsRepairableWith(Props))
            {
                handled = EnumHandling.PreventDefault;
                return false;
            }
            return true;
        }

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
        {
            if (blockSel == null) return;

            var wearAndTear = slot.Inventory.Api.World.BlockAccessor.GetBlockEntity(blockSel.Position)?.GetBehavior<IWearAndTear>();

            if (wearAndTear != null && wearAndTear.IsRepairableWith(Props))
            {
                handHandling = EnumHandHandling.PreventDefault;
            }
        }

        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandling handling)
        {
            if (blockSel == null) return;

            var wearAndTear = slot.Inventory.Api.World.BlockAccessor.GetBlockEntity(blockSel.Position)?.GetBehavior<IWearAndTear>();

            if (wearAndTear != null && wearAndTear.TryMaintenance(Props, slot, byEntity))
            {
                handling = EnumHandling.Handled;
            }
        }

        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot, ref EnumHandling handling)
        {
            handling = EnumHandling.PassThrough;

            return new WorldInteraction[] 
            {
                new() {
                    ActionLangCode = "wearandtear:maintenance",
                    MouseButton = EnumMouseButton.Right
                }
            };
        }
    }
}