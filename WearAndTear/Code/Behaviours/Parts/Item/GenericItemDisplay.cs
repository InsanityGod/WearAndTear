using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;
using WearAndTear.Code.Behaviours.Parts.Abstract;
using WearAndTear.Config.Props;

namespace WearAndTear.Code.Behaviours.Parts.Item
{
    public class GenericItemDisplay : ItemPart
    {
        public GenericItemDisplay(BlockEntity blockentity) : base(blockentity)
        {
            if (Blockentity is BlockEntityContainer container)
            {
                Inventory = container.Inventory;
            }
        }
        
        public readonly InventoryBase Inventory;

        public GenericItemDisplayProps ItemDisplayProps { get; set; }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            ItemDisplayProps = properties.AsObject<GenericItemDisplayProps>();
        }

        public override ItemSlot ItemSlot => Inventory?[ItemDisplayProps.ItemSlotIndex];
    }
}