using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace WearAndTear.Behaviours
{
    public class WearAndTearRepairToolCollectibleBehavior : CollectibleBehavior
    {
        public WearAndTearRepairToolCollectibleBehavior(CollectibleObject collObj) : base(collObj)
        {
        }

        public virtual string RepairType { get; private set; }

        public virtual float RepairStrength { get; private set; } = .5f;

        public virtual bool ConsumeItem { get; private set; } = true;

        public override void Initialize(JsonObject properties)
        {
            base.Initialize(properties);

            RepairType = properties["repairType"].AsString();
            RepairStrength = properties["repairStrength"].AsFloat(RepairStrength);
            ConsumeItem = properties["consumeItem"].AsBool(ConsumeItem);
            //TODO maybe a consumeDurability mode
        }

        public virtual bool TryRepair(WearAndTearBlockEntityBehavior wearAndTearBehavior, ItemSlot slot)
        {
            if (!wearAndTearBehavior.TryRepair(RepairStrength)) return false;

            if (slot.Inventory.Api.Side == EnumAppSide.Server)
            {
                if (ConsumeItem)
                {
                    slot.TakeOut(1);
                    slot.MarkDirty();
                }
            }

            return true;
        }

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
        {
            if (blockSel == null) return;
            var wearAndTearBehavior = slot.Inventory.Api.World.BlockAccessor.GetBlockEntity(blockSel.Position)?.GetBehavior<WearAndTearBlockEntityBehavior>();
            if (wearAndTearBehavior != null && wearAndTearBehavior.IsRepairableWith(this))
            {
                handHandling = EnumHandHandling.Handled;
                handling = EnumHandling.Handled;
            }
        }

        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandling handling)
        {
            if (blockSel == null) return;
            var wearAndTearBehavior = slot.Inventory.Api.World.BlockAccessor.GetBlockEntity(blockSel.Position)?.GetBehavior<WearAndTearBlockEntityBehavior>();
            if (wearAndTearBehavior != null && wearAndTearBehavior.IsRepairableWith(this) && TryRepair(wearAndTearBehavior, slot))
            {
                handling = EnumHandling.Handled;
            }
        }
    }
}