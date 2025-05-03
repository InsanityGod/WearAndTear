using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code.Interfaces;
using WearAndTear.Code.XLib;
using WearAndTear.Code.XLib.Containers;
using WearAndTear.Config.Client;
using WearAndTear.Config.Props;
using WearAndTear.Config.Server;

namespace WearAndTear.Code.Behaviours
{
    public class Part : BlockEntityBehavior
    {
        public Part(BlockEntity blockentity) : base(blockentity) { }
        public PartController Controller { get; private set; }
        public PartProps Props { get; private set; }
        
        protected IDictionary<string, IDecayEngine> DecayEngines { get; private set; }

        /// <summary>
        /// The current bonuses applied to the part
        /// </summary>
        public PartBonuses Bonuses { get; private set; }

        /// <summary>
        /// Wether decay has to be applied on this part (you would turn this off if you want to manually control durability)
        /// </summary>
        public virtual bool RequiresUpdateDecay => true;
        
        /// <summary>
        /// Wether there is a limit to how much maintenance can be done to this part
        /// </summary>
        public virtual bool HasMaintenanceLimit => !(WearAndTearServerConfig.Instance?.AllowForInfiniteMaintenance ?? false) && Props.MaintenanceLimit != null;
        //TODO Check and see if above is still required //public virtual bool HasMaintenanceLimit => !WearAndTearServerConfig.Instance.AllowForInfiniteMaintenance && Props.MaintenanceLimit != null;

        /// <summary>
        /// Method to add info about this part to the string builder
        /// </summary>
        /// <param name="forPlayer">The player who will see the info</param>
        /// <param name="dsc"/>
        public virtual void GetWearAndTearInfo(IPlayer forPlayer, StringBuilder dsc) => dsc.AppendLine(Props.GetDurabilityStringForPlayer(Api, forPlayer, Durability));

        public virtual bool CanRepairWith(RepairItemProps props)
        {
            if (props.RequiredMaterialVariant != null && props.RequiredMaterialVariant != Props.MaterialVariant) return false;
            return props.RepairType == Props.RepairType;
            //TODO see about getting this info in handbook
        }

        public virtual float? EfficiencyModifier => Props.DurabilityEfficiencyRatio == 0 ? null : 1 - (1f - Durability) * Props.DurabilityEfficiencyRatio;

        /// <summary>
        /// Repairs item and returns remaining repair strength
        /// </summary>
        /// <param name="maintenanceStrength">How much can be repaired</param>
        /// <param name="player"/>
        /// <returns>How much can still be repaired with this item (on other parts that is)</returns>
        public virtual float DoMaintenanceFor(float maintenanceStrength, EntityPlayer player)
        {
            var allowedMaintenanceStrength = maintenanceStrength;
            if (HasMaintenanceLimit)
            {
                var limit = Props.MaintenanceLimit.Value;

                if (WearAndTearModSystem.XlibEnabled) limit = SkillsAndAbilities.ApplyLimitBreakerBonus(player.Api, player.Player, limit);

                allowedMaintenanceStrength = GameMath.Clamp(maintenanceStrength, 0, limit - RepairedDurability);
            }

            Durability += allowedMaintenanceStrength;
            var leftOverMaintenanceStrength = maintenanceStrength - allowedMaintenanceStrength + Math.Max(Durability - 1, 0);

            if (HasMaintenanceLimit) RepairedDurability += allowedMaintenanceStrength - Math.Max(Durability - 1, 0);

            Durability = GameMath.Clamp(Durability, WearAndTearServerConfig.Instance.MinDurability, 1);

            return Math.Max(leftOverMaintenanceStrength, 0);
        }

        /// <summary>
        /// Code that can run/override breaking behavior
        /// </summary>
        /// <returns>true if default behavior should run</returns>
        public virtual bool OnBreak() => Props.IsCritical;

        public virtual ItemStack[] ModifyDroppedItemStacks(ItemStack[] itemStacks, IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier, bool isBlockDestroyed)
        {
            if (isBlockDestroyed && RubbleConfig.Instance.GenerateScrap && Props.ScrapCode != null)
            {
                var scrap = Api.World.GetItem(Props.ScrapCode);
                if (scrap != null && !scrap.IsMissing)
                {
                    var factor = (Durability * RubbleConfig.Instance.DurabilityDropPercentage) + RubbleConfig.Instance.FixedDropPercentage;
                    var item = new ItemStack(scrap)
                    {
                        StackSize = (int)(GameMath.Clamp(Props.ContentLevel * factor, 0, Props.ContentLevel) * dropQuantityMultiplier)
                    };

                    if (item.StackSize > 0) itemStacks = itemStacks.Append(item);
                }
            }
            return itemStacks;
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            Props ??= properties.AsObject<PartProps>() ?? new();
            Props.Decay ??= new DecayProps[]
            {
                new() {
                    Type = "time"
                }
            };
            DecayEngines = Api.ModLoader.GetModSystem<WearAndTearModSystem>().DecayEngines;
            Controller = Blockentity.GetBehavior<PartController>();
            
            if (WearAndTearModSystem.XlibEnabled) Bonuses ??= new();
        }

        public override void OnBlockPlaced(ItemStack byItemStack = null)
        {
            base.OnBlockPlaced(byItemStack);

            var durabilityTree = byItemStack?.Attributes?.GetTreeAttribute(Constants.DurabilityTreeName);
            if (durabilityTree != null)
            {
                if (CompatibilityConfig.Instance.LegacyCompatibility) LegacyCompatibilityFixes(durabilityTree);
                Durability = durabilityTree.GetFloat(Props.Code, Durability);
                RepairedDurability = durabilityTree.GetFloat(Props.Code + Constants.RepairedPrefix, RepairedDurability);
            }
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);

            //This is to deal with this method being called before Initialize
            Props ??= properties.AsObject<PartProps>() ?? new();
            if (WearAndTearModSystem.XlibEnabled) Bonuses ??= new();

            if (Props == null) return;

            var durabilityTree = tree.GetOrAddTreeAttribute(Constants.DurabilityTreeName);
            if (CompatibilityConfig.Instance.LegacyCompatibility) LegacyCompatibilityFixes(durabilityTree);
            Durability = durabilityTree.GetFloat(Props.Code, Durability);
            if (HasMaintenanceLimit) RepairedDurability = durabilityTree.GetFloat(Props.Code + Constants.RepairedPrefix, RepairedDurability);

            Bonuses?.FromTreeAttributes(tree, Props);
        }

        protected virtual void LegacyCompatibilityFixes(ITreeAttribute durabilityTree)
        {
            if (durabilityTree[Props.Code] != null) return;
            var parts = Props.Code.Path.Split('-');
            var key = durabilityTree.Select(pair => pair.Key).FirstOrDefault(key => Array.TrueForAll(parts, part => key.ToLower().Replace(" ", string.Empty).Contains(part)));
            if (key != null)
            {
                var durability = durabilityTree.GetFloat(key, Durability);
                durabilityTree.RemoveAttribute(key);
                durabilityTree.SetFloat(Props.Code, durability);

                var durability_repaired = durabilityTree.GetFloat(key + Constants.RepairedPrefix, RepairedDurability);
                durabilityTree.RemoveAttribute(key + Constants.RepairedPrefix);
                durabilityTree.SetFloat(Props.Code + Constants.RepairedPrefix, durability_repaired);
            }
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            var durabilityTree = tree.GetOrAddTreeAttribute(Constants.DurabilityTreeName);
            durabilityTree.SetFloat(Props.Code, Durability);
            if (HasMaintenanceLimit) durabilityTree.SetFloat(Props.Code + Constants.RepairedPrefix, RepairedDurability);
            Bonuses?.ToTreeAttributes(tree, Props);
        }

        public virtual void UpdateDecay(double daysPassed)
        {
            foreach (var decay in Props.Decay)
            {
                var loss = DecayEngines[decay.Type].GetDecayLoss(Api, this, decay, daysPassed) / Props.Decay.Length;

                foreach (var protectivePart in Controller.Parts.OfType<IProtectivePart>())
                {
                    if (protectivePart is IOptionalPart optionalPart && !optionalPart.IsPresent) continue;

                    loss *= protectivePart.GetDecayMultiplierFor(Props);
                }

                if (Bonuses != null) loss *= Bonuses.DecayModifier;
                Durability -= loss;
            }
            Durability = GameMath.Clamp(Durability, WearAndTearServerConfig.Instance.MinDurability, 1);
        }

        private float _durability = 1;

        public virtual float Durability
        {
            get => _durability;
            set
            {
                if (float.IsNaN(value))
                {
                    if (WearAndTearClientConfig.Instance.EnableDebugLogging) Api?.Logger.Warning($"[WearAndTear] Invalid Durability assignment at {Pos} for {Props?.MaterialVariant} {Props?.Code} (ignoring attempt)");
                    return;
                }
                _durability = value;
            }
        }

        public float RepairedDurability { get; set; } = 0;

        //HACK mod system is disposed before ToTreeAttributes is called resulting in Config being null...

        public bool IsActive
        {
            get
            {
                var powerDevice = Blockentity.GetBehavior<IMechanicalPowerDevice>();
                return powerDevice?.Network != null && powerDevice.Network.Speed > 0.001;
            }
        }


    }
}