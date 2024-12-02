using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Config.Props;
using WearAndTear.Interfaces;

namespace WearAndTear.Behaviours
{
    public class WearAndTearBehavior : BlockEntityBehavior, IWearAndTear
    {
        public WearAndTearBehavior(BlockEntity blockentity) : base(blockentity)
        {
        }

        private RoomRegistry RoomRegistry;

        public ItemStack[] ModifyDroppedItemStacks(ItemStack[] itemStacks, IWorldAccessor world, BlockPos pos, IPlayer byPlayer)
        {
            string blockCode = Block.Code.Path.Split('-')[0];
            var normalItem = Array.Find(itemStacks, item => blockCode == item?.Block.Code.Path.Split('-')[0]);
            if(normalItem != null)
            {
                ITreeAttribute tree = new TreeAttribute();
                Blockentity.ToTreeAttributes(tree);
                tree = tree.GetOrAddTreeAttribute("WearAndTear-Durability");
                
                foreach(var part in Parts)
                {
                    if(part is IWearAndTearProtectivePart)
                    {
                        if(part.Durability == 0) tree.RemoveAttribute(part.Props.Name);
                        continue;
                    }

                    if(part.Durability > WearAndTearModSystem.Config.DurabilityLeeway) tree.RemoveAttribute(part.Props.Name);
                }
                if(tree.Count > 0) 
                {
                    normalItem.Attributes
                        .GetOrAddTreeAttribute("WearAndTear-Durability")
                        .MergeTree(tree);
                }
            }

            return itemStacks;
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            Parts = Blockentity.Behaviors.OfType<IWearAndTearPart>().ToList();

            RoomRegistry = Api.ModLoader.GetModSystem<RoomRegistry>(true);
            Blockentity.RegisterGameTickListener(UpdateIsInsideRoom, WearAndTearModSystem.Config.RoomCheckFrequencyInMs);
            UpdateIsInsideRoom(0);

            LastDecayUpdate ??= Api.World.Calendar.TotalDays;
            if (api.Side != EnumAppSide.Server) return;
            Blockentity.RegisterGameTickListener(_ => UpdateDecay(Api.World.Calendar.TotalDays - LastDecayUpdate.Value), WearAndTearModSystem.Config.DurabilityUpdateFrequencyInMs);
        }

        public double? LastDecayUpdate { get; set; }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            LastDecayUpdate = tree.TryGetDouble("LastDecayUpdate") ?? LastDecayUpdate;
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            if (LastDecayUpdate.HasValue) tree.SetDouble("LastDecayUpdate", LastDecayUpdate.Value);
        }

        public List<IWearAndTearPart> Parts { get; private set; }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);
            dsc.AppendLine();
            dsc.AppendLine($"<strong>WearAndTear {(IsInsideRoom ? "(Inside)" : "(Outside)")}</strong>");
            foreach (var part in Parts) part.GetWearAndTearInfo(forPlayer, dsc);

            dsc.AppendLine();
        }

        public bool IsInsideRoom { get; private set; }

        public void UpdateIsInsideRoom(float secondsPassed) => IsInsideRoom = RoomRegistry.GetRoomForPosition(Pos).ExitCount == 0;

        public void UpdateDecay(double daysPassed, bool updateLastUpdatedAt = true)
        {
            if (updateLastUpdatedAt) LastDecayUpdate = Api.World.Calendar.TotalDays;

            foreach (var part in Parts)
                part.UpdateDecay(daysPassed);

            Blockentity.MarkDirty();
        }

        public virtual bool TryMaintenance(WearAndTearRepairItemProps props, ItemSlot slot, EntityAgent byEntity)
        {
            var powerDevice = Blockentity.GetBehavior<IMechanicalPowerDevice>();
            if (powerDevice?.Network != null && powerDevice.Network.Speed > 0.001)
            {
                if (Api is ICoreClientAPI clientApi) clientApi?.TriggerIngameError(this, "wearandtear:failed-maintenance-moving", Lang.Get("wearandtear:failed-maintenance-moving"));
                return false;
            }

            var maintenanceStrength = props.Strength;
            var anyPartRequiredMaintenance = false;
            foreach (var part in Parts)
            {
                if (!part.CanDoMaintenanceWith(props) || part.Durability > WearAndTearModSystem.Config.MinMaintenanceDurability) continue;

                anyPartRequiredMaintenance = true;

                maintenanceStrength = part.DoMaintenanceFor(maintenanceStrength);
                if (maintenanceStrength <= 0) break;
            }

            if (!anyPartRequiredMaintenance && Api is ICoreClientAPI clientApi2)
            {
                clientApi2.TriggerIngameError(this, "wearandtear:failed-maintenance-not-required", Lang.Get("wearandtear:failed-maintenance-not-required"));
            }

            if (maintenanceStrength < props.Strength)
            {
                slot.TakeOut(1);
                slot.MarkDirty();
                return true;
            }

            return false;
        }

    }
}