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
using Vintagestory.API.Util;
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
                    if (part.Props.IsCritical && part.Durability == 0)
                    {
                        return itemStacks.Remove(normalItem);
                    }
                    
                    if(part is IWearAndTearOptionalPart optionalPart)
                    {
                        if(!optionalPart.IsPresent) tree.RemoveAttribute(part.Props.Name);
                        continue;
                    }

                    if(part.Durability > WearAndTearModSystem.Config.DurabilityLeeway) tree.RemoveAttribute(part.Props.Name);
                }

                //Remove all unnecary variables
                foreach(var item in tree.Where(item => item.Key.EndsWith("_Repaired") && (float)item.Value.GetValue() == 0).ToList())
                {
                    tree.RemoveAttribute(item.Key);
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
            Blockentity.RegisterGameTickListener(UpdateIsSheltered, WearAndTearModSystem.Config.RoomCheckFrequencyInMs);
            UpdateIsSheltered(0);

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
            dsc.AppendLine($"<strong>WearAndTear {(IsSheltered ? "(Sheltered)" : "(Unsheltered)")}</strong>");
            foreach (var part in Parts) part.GetWearAndTearInfo(forPlayer, dsc);

            dsc.AppendLine();
        }

        public bool IsSheltered { get; private set; }

        public void UpdateIsSheltered(float secondsPassed) => IsSheltered = RoomRegistry.GetRoomForPosition(Pos).ExitCount <= WearAndTearModSystem.Config.RoomExitCountLeeway;

        public void UpdateDecay(double daysPassed, bool updateLastUpdatedAt = true)
        {
            if (updateLastUpdatedAt) LastDecayUpdate = Api.World.Calendar.TotalDays;

            foreach (var part in Parts)
                part.UpdateDecay(daysPassed);

            if(Parts.Exists(part => part.Props.IsCritical && part.Durability <= 0))
            {
                Api.World.BlockAccessor.BreakBlock(Pos, null, 0);
                //TODO Maybe allow for parts to drop stuff when this happens?
            }

            Blockentity.MarkDirty();
        }

        public virtual bool TryMaintenance(WearAndTearRepairItemProps props, ItemSlot slot, EntityAgent byEntity)
        {
            if(props.RequiredTool != null && !WildcardUtil.Match(props.RequiredTool, byEntity.LeftHandItemSlot?.Itemstack?.Collectible?.Code.Path ?? string.Empty))
            {
                if(Api is ICoreClientAPI clientApi)
                {
                    clientApi.TriggerIngameError(
                        this,
                        "wearandtear:failed-maintenance-missing-tool",
                        Lang.Get(string.IsNullOrEmpty(props.MissingToolLangCode) ? props.RequiredTool : props.MissingToolLangCode)
                    );
                }

                return false;
            }
            var maintenanceStrength = props.Strength;
            var anyPartRequiredMaintenance = false;
            var anyPartMaintenanceLimitReached = false;
            var anyPartActive = false;
            foreach (var part in Parts)
            {
                if (!part.CanDoMaintenanceWith(props) || part.Durability > WearAndTearModSystem.Config.MinMaintenanceDurability) continue;
                anyPartRequiredMaintenance = true;

                if (WearAndTearModSystem.Config.MaintenanceRequiresInactivePart && part.IsActive)
                {
                    anyPartActive = true;
                    continue;
                }

                var remainingMaintenanceStrength = part.DoMaintenanceFor(maintenanceStrength);


                if(!WearAndTearModSystem.Config.AllowForInfiniteMaintenance && remainingMaintenanceStrength == maintenanceStrength)
                {
                    anyPartMaintenanceLimitReached = true;
                }

                maintenanceStrength = remainingMaintenanceStrength;
                if (maintenanceStrength <= 0) break;
            }

            //If any maintenance was done
            if (maintenanceStrength < props.Strength)
            {
                slot.TakeOut(1);
                slot.MarkDirty();
                Blockentity.MarkDirty();

                if (props.RequiredTool != null && props.ToolDurabilityCost > 0)
                {
                    byEntity.LeftHandItemSlot.Itemstack.Collectible.DamageItem(Api.World, byEntity, byEntity.LeftHandItemSlot, props.ToolDurabilityCost);
                }

                return true;
            }
            else if(Api is ICoreClientAPI clientApi2)
            {
                if (!anyPartRequiredMaintenance)
                {
                    clientApi2.TriggerIngameError(this, "wearandtear:failed-maintenance-not-required", Lang.Get("wearandtear:failed-maintenance-not-required"));
                }
                else if (anyPartActive)
                {
                    clientApi2.TriggerIngameError(this, "wearandtear:failed-maintenance-active", Lang.Get("wearandtear:failed-maintenance-active"));
                }
                else if (anyPartMaintenanceLimitReached)
                {
                    clientApi2.TriggerIngameError(this, "wearandtear:failed-maintenance-limit-reached", Lang.Get("wearandtear:failed-maintenance-limit-reached"));
                }
            }

            return false;
        }

    }
}