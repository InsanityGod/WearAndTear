using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code.Interfaces;
using WearAndTear.Config.Props;

namespace WearAndTear.Code.Behaviours
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
            var normalItem = Array.Find(itemStacks, item => item.Block != null && blockCode == item.Block.Code.Path.Split('-')[0]);
            if (normalItem != null)
            {
                ITreeAttribute tree = new TreeAttribute();
                Blockentity.ToTreeAttributes(tree);
                tree = tree.GetOrAddTreeAttribute("WearAndTear-Durability");

                foreach (var part in Parts)
                {
                    if (part.Props.IsCritical && part.Durability <= 0)
                    {
                        return itemStacks.Remove(normalItem);
                    }

                    if (part is IWearAndTearOptionalPart optionalPart)
                    {
                        if (!optionalPart.IsPresent) tree.RemoveAttribute(part.Props.Name);
                        continue;
                    }

                    if (part.Durability > WearAndTearModSystem.Config.DurabilityLeeway || float.IsNaN(part.Durability)) tree.RemoveAttribute(part.Props.Name);
                }

                //Remove all unnecary variables
                foreach (var item in tree.Where(item => item.Key.EndsWith("_Repaired") && (float)item.Value.GetValue() == 0).ToList())
                {
                    tree.RemoveAttribute(item.Key);
                }

                if (tree.Count > 0)
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
            if(api.Side == EnumAppSide.Client) QueueDecalUpdate();
            if (api.Side != EnumAppSide.Server) return;
            if (!Parts.Exists(part => part.RequiresUpdateDecay)) return;
            //TODO maybe create a manager for this to reduce the ammount of GameTickListeners
            Blockentity.RegisterGameTickListener(_ => UpdateDecay(Api.World.Calendar.TotalDays - LastDecayUpdate.Value), WearAndTearModSystem.Config.DurabilityUpdateFrequencyInMs, Api.World.Rand.Next(0, WearAndTearModSystem.Config.DurabilityUpdateFrequencyInMs));
        }

        public double? LastDecayUpdate { get; set; }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            LastDecayUpdate = tree.TryGetDouble("LastDecayUpdate") ?? LastDecayUpdate;

            if(Api?.Side == EnumAppSide.Client) QueueDecalUpdate();
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
            {
                part.UpdateDecay(daysPassed);
            }
            
            //Seperate loop since we want to ensure all durability is updated for block drop modifications
            foreach (var part in Parts)
            {
                if(part.Durability <= 0)
                {
                    if (part.Props.IsCritical)
                    {
                        if (part.OnBreak())
                        {
                            Api.World.BlockAccessor.BreakBlock(Pos, null, 0);
                        }
                        return;
                    }
                }
            }

            Blockentity.MarkDirty();
        }

        private object decal;
        private Traverse DecalCreator;
        private Traverse DecalCache;
        private Traverse DecalUpdator;
        private Traverse<int> DecalId;

        public void QueueDecalUpdate(int delay = 1)
        {
            if(Api == null || WearAndTearModSystem.Config.VisualTearingMinDurability == 0 || (WearAndTearModSystem.Config.DisableVisualTearingOnMPBlocks && Block is BlockMPBase)) return;
            Blockentity.RegisterDelayedCallback(_ => UpdateDecal(), delay);
        }

        public void UpdateDecal()
        {
            //TODO option to hide this for MechanicalBlocks
            if(Api is not ICoreClientAPI clientApi || Parts == null) return;
            
            if(DecalCreator == null)
            {
                var clientMain = Traverse.Create(clientApi).Field<ClientMain>("gamemain").Value;
                var decalSystem = clientMain.clientSystems.OfType<SystemRenderDecals>().First();
                DecalCreator = Traverse.Create(decalSystem).Method("AddBlockBreakDecal", new Type[] { typeof(BlockPos), typeof(int)});
            }

            if(DecalCache == null)
            {

                var clientMain = Traverse.Create(clientApi).Field<ClientMain>("gamemain").Value;
                var decalSystem = clientMain.clientSystems.OfType<SystemRenderDecals>().First();
                DecalCache = Traverse.Create(decalSystem).Field("decals");
            }

            var criticalparts = Parts.Where(part => part.Props.IsCritical).ToArray();
            var durability = criticalparts.Length > 0 ? criticalparts.Min(part => part.Durability) : 1;

            if(durability > WearAndTearModSystem.Config.VisualTearingMinDurability)
            {
                if(decal != null) DecalCache.GetValue<IDictionary>().Remove(DecalId.Value);
                return;
            }
            var stage = 10 - (int)Math.Max(1, durability / WearAndTearModSystem.Config.VisualTearingMinDurability * 10);

            decal ??= DecalCreator.GetValue(Pos, stage);

            if(decal == null) return; //Just in case

            var DecalStage = Traverse.Create(decal).Field<int>("AnimationStage");
            DecalId = Traverse.Create(decal).Field<int>("DecalId");

            if (!DecalCache.GetValue<IDictionary>().Contains(DecalId.Value))
            {
                decal = DecalCreator.GetValue(Pos, stage);
                if(decal == null) return; //Just in case
            }
            
            DecalStage.Value = stage;

            if(DecalUpdator == null)
            {
                var clientMain = Traverse.Create(clientApi).Field<ClientMain>("gamemain").Value;
                var decalSystem = clientMain.clientSystems.OfType<SystemRenderDecals>().First();
                DecalUpdator = Traverse.Create(decalSystem).Method("UpdateDecal", new Type[] { decal.GetType() });
            }

            DecalUpdator.GetValue(decal);
        }

        public virtual bool TryMaintenance(WearAndTearRepairItemProps props, ItemSlot slot, EntityAgent byEntity)
        {
            if (props.RequiredTool != null && !WildcardUtil.Match(props.RequiredTool, byEntity.LeftHandItemSlot?.Itemstack?.Collectible?.Code.Path ?? string.Empty))
            {
                if (Api is ICoreClientAPI clientApi)
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

                if (!WearAndTearModSystem.Config.AllowForInfiniteMaintenance && remainingMaintenanceStrength == maintenanceStrength)
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
            else if (Api is ICoreClientAPI clientApi2)
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