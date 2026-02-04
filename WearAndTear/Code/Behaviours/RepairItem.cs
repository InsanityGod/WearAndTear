using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using WearAndTear.Config.Props;

namespace WearAndTear.Code.Behaviours;

public class RepairItem : CollectibleBehavior
{
    public RepairItem(CollectibleObject collObj) : base(collObj) { }

    public RepairItemProps Props { get; private set; }

    public override void Initialize(JsonObject properties)
    {
        base.Initialize(properties);
        Props = properties.AsObject<RepairItemProps>() ?? new();
        //TODO maybe a consumeDurability mode?
        //TODO maybe have repair strength be relative to content level?
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

    public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
    {
        if (blockSel == null) return;

        var wearAndTear = slot.Inventory.Api.World.BlockAccessor.GetBlockEntity(blockSel.Position)?.GetBehavior<PartController>();

        if (wearAndTear != null && wearAndTear.CanRepairWith(Props))
        {
            handHandling = EnumHandHandling.PreventDefault;
        }
    }

    public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandling handling)
    {
        if (blockSel == null) return;

        var wearAndTear = slot.Inventory.Api.World.BlockAccessor.GetBlockEntity(blockSel.Position)?.GetBehavior<PartController>();

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