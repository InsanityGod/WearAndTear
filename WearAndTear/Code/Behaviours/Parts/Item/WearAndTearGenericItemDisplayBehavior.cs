using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;
using WearAndTear.Code.Behaviours.Parts.Abstract;
using WearAndTear.Config.Props;

namespace WearAndTear.Code.Behaviours.Parts.Item
{
    public class WearAndTearGenericItemDisplayBehavior : WearAndTearItemPartBehavior
    {
        public readonly InventoryBase Inventory;

        public WearAndTearGenericItemDisplayProps displayProps { get; set; }

        public WearAndTearGenericItemDisplayBehavior(BlockEntity blockentity) : base(blockentity)
        {
            if (blockentity is BlockEntityContainer container)
            {
                Inventory = container.Inventory;
            }
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            displayProps = properties.AsObject<WearAndTearGenericItemDisplayProps>();
        }

        public override ItemSlot ItemSlot => Inventory[displayProps.ItemSlotIndex];
    }
}