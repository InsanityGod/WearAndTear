using HarmonyLib;
using InsanityLib.Util;
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
using WearAndTear.Code.XLib;
using WearAndTear.Config.Client;
using WearAndTear.Config.Props;
using WearAndTear.Config.Props.rubble;
using WearAndTear.Config.Server;

namespace WearAndTear.Code.Behaviours
{
    public class PartController : BlockEntityBehavior
    {
        public PartController(BlockEntity blockentity) : base(blockentity) { }

        public List<Part> Parts { get; private set; }

        public virtual float Durability => Parts.Average(p => p.Durability);

        private RoomRegistry RoomRegistry;

        public virtual bool CanRepairWith(RepairItemProps props) => Parts.Exists(part => part.CanRepairWith(props));

        public virtual float AvgEfficiencyModifier
        {
            get
            {
                var totalParts = 0;
                var totalModifier = 0f;

                foreach (var part in Parts)
                {
                    var modifier = part.EfficiencyModifier;
                    if (modifier == null) continue;
                    totalModifier += modifier.Value;
                    totalParts++;
                }

                if (totalParts == 0) return 1;
                return totalModifier / totalParts;
            }
        }

        public virtual ItemStack[] ModifyDroppedItemStacks(ItemStack[] itemStacks, IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
        {
            var block = Block.GetPlacedByItem(world.Api);

            string blockCode = block.Code.Path.Split('-')[0];
            var normalItem = Array.Find(itemStacks, item => item.Block != null && blockCode == item.Block.Code.Path.Split('-')[0]);
            bool isBlockDestroyed = false;
            if (normalItem != null)
            {
                ITreeAttribute tree = new TreeAttribute();
                Blockentity.ToTreeAttributes(tree);
                tree = tree.GetOrAddTreeAttribute(Constants.DurabilityTreeName);

                foreach (var part in Parts)
                {
                    if (part.Props.IsCritical && part.Durability <= 0)
                    {
                        itemStacks = itemStacks.Remove(normalItem);
                        normalItem = null;
                        isBlockDestroyed = true;
                        break;
                    }

                    if (part is IOptionalPart optionalPart)
                    {
                        if (!optionalPart.IsPresent) tree.RemoveAttribute(part.Props.Code);
                        continue;
                    }

                    if (part.Durability > WearAndTearServerConfig.Instance.DurabilityLeeway || float.IsNaN(part.Durability)) tree.RemoveAttribute(part.Props.Code);
                }

                if (normalItem != null)
                {
                    //Remove all unnecary variables
                    foreach (var item in tree.Where(item => item.Key.EndsWith(Constants.RepairedPrefix) && (float)item.Value.GetValue() == 0).ToList())
                    {
                        tree.RemoveAttribute(item.Key);
                    }

                    if (tree.Count > 0)
                    {
                        normalItem.Attributes
                            .GetOrAddTreeAttribute(Constants.DurabilityTreeName)
                            .MergeTree(tree);
                    }
                }
            }

            foreach (var part in Parts)
            {
                itemStacks = part.ModifyDroppedItemStacks(itemStacks, world, pos, byPlayer, dropQuantityMultiplier, isBlockDestroyed);
            }

            return itemStacks;
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            Parts = Blockentity.Behaviors.OfType<Part>().ToList();

            RoomRegistry = Api.ModLoader.GetModSystem<RoomRegistry>(true);
            Blockentity.RegisterGameTickListener(UpdateIsSheltered, WearAndTearServerConfig.Instance.RoomCheckFrequencyInMs);
            UpdateIsSheltered(0);

            LastDecayUpdate ??= Api.World.Calendar.TotalDays;
            if (api.Side == EnumAppSide.Client) QueueDecalUpdate();
            if (api.Side != EnumAppSide.Server) return;
            if (!Parts.Exists(part => part.RequiresUpdateDecay)) return;

            //TODO maybe create a manager for this to reduce the ammount of GameTickListeners (and allow for more gradual ticking)
            Blockentity.RegisterGameTickListener(_ => UpdateDecay(Api.World.Calendar.TotalDays - LastDecayUpdate.Value), WearAndTearServerConfig.Instance.DurabilityUpdateFrequencyInMs, Api.World.Rand.Next(0, WearAndTearServerConfig.Instance.DurabilityUpdateFrequencyInMs));
        }

        public double? LastDecayUpdate { get; set; }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            LastDecayUpdate = tree.TryGetDouble("LastDecayUpdate") ?? LastDecayUpdate;

            if (Api?.Side == EnumAppSide.Client) QueueDecalUpdate();
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            if (LastDecayUpdate.HasValue) tree.SetDouble("LastDecayUpdate", LastDecayUpdate.Value);
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);
            dsc.AppendLine();
            dsc.AppendLine($"<strong>WearAndTear {(IsSheltered ? "(Sheltered)" : "(Unsheltered)")}</strong>");
            foreach (var part in Parts) part.GetWearAndTearInfo(forPlayer, dsc);

            dsc.AppendLine();
        }

        public bool IsSheltered { get; private set; }

        public void UpdateIsSheltered(float secondsPassed) => IsSheltered = RoomRegistry.GetRoomForPosition(Pos).ExitCount <= WearAndTearServerConfig.Instance.RoomExitCountLeeway;

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
                if (part.Durability <= 0 && part.OnBreak())
                {
                    if (RubbleConfig.Instance.RubbleBlock)
                    {
                        GenerateRubbleBlock();
                    }
                    else Api.World.BlockAccessor.BreakBlock(Pos, null, 0);
                }
            }

            Blockentity.MarkDirty();
        }

        public void GenerateRubbleBlock()
        {
            var stabilityVariant = Block.Attributes[WearAndTearRubbleProps.Key][nameof(WearAndTearRubbleProps.Unstable)].AsBool(true) ? "unstable" : "stable";

            var rubbleBlock = Api.World.GetBlock($"wearandtear:rubble-{stabilityVariant}");
            if (rubbleBlock == null)
            {
                Api.Logger.Error("[WearAndTear] failed to generate rubble (block not found)");
                return;
            }

            var stack = new ItemStack(Block);
            foreach (var part in Parts)
            {
                (part as BlockEntityBehavior)?.ToTreeAttributes(stack.Attributes);
            }
            ToTreeAttributes(stack.Attributes);

            try
            {
                var drops = Block.GetDrops(Api.World, Pos, null);

                //TODO Container drops (helve hammer) should be part of rubble pile

                if (drops.Length == 0)
                {
                    //No point in creating rubble if it doesn't contain anything
                    Api.World.BlockAccessor.BreakBlock(Pos, null);
                    return;
                }

                var normalDropsTree = stack.Attributes.GetOrAddTreeAttribute("rubble-normal-drops");

                for (var i = 0; i < drops.Length; i++)
                {
                    normalDropsTree.SetItemstack(i.ToString(), drops[i]);
                }
            }
            catch (Exception ex)
            {
                Api.Logger.Error("[WearAndTear] Failed to get normal drops for rubble: {0}", ex);
            }

            Block.OnBlockBroken(Api.World, Pos, null, 0);

            Api.World.BlockAccessor.SetBlock(rubbleBlock.BlockId, Pos, stack);
        }

        private object decal;
        private Traverse DecalCreator;
        private Traverse DecalCache;
        private Traverse DecalUpdator;
        private Traverse<int> DecalId;

        public void QueueDecalUpdate(int delay = 1)
        {
            if (Api == null || WearAndTearClientConfig.Instance.VisualTearingMinDurability == 0 || (WearAndTearClientConfig.Instance.DisableVisualTearingOnMPBlocks && Block is BlockMPBase) || Blockentity is BlockEntityIngotMold) return;
            Blockentity.RegisterDelayedCallback(_ => UpdateDecal(), delay);
        }

        public void UpdateDecal()
        {
            if (Api is not ICoreClientAPI clientApi || Parts == null) return;

            if (DecalCreator == null)
            {
                var clientMain = Traverse.Create(clientApi).Field<ClientMain>("gamemain").Value;
                var decalSystem = clientMain.clientSystems.OfType<SystemRenderDecals>().First();
                DecalCreator = Traverse.Create(decalSystem).Method("AddBlockBreakDecal", new Type[] { typeof(BlockPos), typeof(int) });
            }

            if (DecalCache == null)
            {
                var clientMain = Traverse.Create(clientApi).Field<ClientMain>("gamemain").Value;
                var decalSystem = clientMain.clientSystems.OfType<SystemRenderDecals>().First();
                DecalCache = Traverse.Create(decalSystem).Field("decals");
            }

            var criticalparts = Parts.Where(part => part.Props != null && part.Props.IsCritical).ToArray();
            var durability = criticalparts.Length > 0 ? criticalparts.Min(part => part.Durability) : 1;

            if (durability > WearAndTearClientConfig.Instance.VisualTearingMinDurability)
            {
                if (decal != null) DecalCache.GetValue<IDictionary>().Remove(DecalId.Value);
                return;
            }
            var stage = 10 - (int)Math.Max(1, durability / WearAndTearClientConfig.Instance.VisualTearingMinDurability * 10);

            decal ??= DecalCreator.GetValue(Pos, stage);

            if (decal == null) return; //Just in case

            var DecalStage = Traverse.Create(decal).Field<int>("AnimationStage");
            DecalId = Traverse.Create(decal).Field<int>("DecalId");

            if (!DecalCache.GetValue<IDictionary>().Contains(DecalId.Value))
            {
                decal = DecalCreator.GetValue(Pos, stage);
                if (decal == null) return; //Just in case
            }

            DecalStage.Value = stage;

            if (DecalUpdator == null)
            {
                var clientMain = Traverse.Create(clientApi).Field<ClientMain>("gamemain").Value;
                var decalSystem = clientMain.clientSystems.OfType<SystemRenderDecals>().First();
                DecalUpdator = Traverse.Create(decalSystem).Method("UpdateDecal", new Type[] { decal.GetType() });
            }

            DecalUpdator.GetValue(decal);
        }

        public virtual bool TryMaintenance(RepairItemProps props, ItemSlot slot, EntityAgent byEntity)
        {
            if (byEntity is not EntityPlayer player) return false;
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

            if (WearAndTearServerConfig.Instance.TraitRequirements && props.RequiredTraits != null)
            {
                var characterSystem = Api.ModLoader.GetModSystem<CharacterSystem>();

                var missingTraits = props.RequiredTraits.Where(trait => !characterSystem.HasTrait(player.Player, trait)).ToList();
                if (missingTraits.Any())
                {
                    if (Api is ICoreClientAPI clientApi)
                    {
                        clientApi.TriggerIngameError(
                            this,
                            "wearandtear:failed-maintenance-missing-traits",
                            Lang.Get("wearandtear:failed-maintenance-missing-traits", string.Join(", ", missingTraits.Select(trait => Lang.Get($"trait-{trait}"))))
                        );
                    }
                    return false;
                }
            }

            var maintenanceStrength = props.Strength;
            if (WearAndTearModSystem.XlibEnabled) maintenanceStrength = SkillsAndAbilities.ApplyHandyManBonus(Api, player.Player, maintenanceStrength);
            var originalMaintenanceStrength = maintenanceStrength;

            var anyPartRequiredMaintenance = false;
            var anyPartMaintenanceLimitReached = false;
            var anyPartActive = false;
            foreach (var part in Parts)
            {
                if (!part.CanRepairWith(props) || part.Durability > WearAndTearServerConfig.Instance.MinMaintenanceDurability) continue;
                anyPartRequiredMaintenance = true;

                if (WearAndTearServerConfig.Instance.MaintenanceRequiresInactivePart && part.IsActive)
                {
                    anyPartActive = true;
                    continue;
                }

                var remainingMaintenanceStrength = part.DoMaintenanceFor(maintenanceStrength, player);
                if (WearAndTearModSystem.XlibEnabled) part.Bonuses?.UpdateForRepair(part, Api, player.Player);

                if (!WearAndTearServerConfig.Instance.AllowForInfiniteMaintenance && remainingMaintenanceStrength == maintenanceStrength)
                {
                    anyPartMaintenanceLimitReached = true;
                }

                maintenanceStrength = remainingMaintenanceStrength;
                if (maintenanceStrength <= 0) break;
            }

            //If any maintenance was done
            if (maintenanceStrength < originalMaintenanceStrength)
            {
                slot.TakeOut(1);
                slot.MarkDirty();
                Blockentity.MarkDirty();

                if (props.RequiredTool != null && props.ToolDurabilityCost > 0)
                {
                    byEntity.LeftHandItemSlot.Itemstack.Collectible.DamageItem(Api.World, byEntity, byEntity.LeftHandItemSlot, props.ToolDurabilityCost);
                }

                if (byEntity is EntityPlayer player2 && WearAndTearModSystem.XlibEnabled) SkillsAndAbilities.GiveMechanicExp(player2.Api, player2.Player, (originalMaintenanceStrength - maintenanceStrength) * CompatibilityConfig.Instance.DurabilityToXPRatio);
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