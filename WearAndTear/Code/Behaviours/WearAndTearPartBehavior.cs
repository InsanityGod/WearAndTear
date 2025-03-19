using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code.Extensions;
using WearAndTear.Code.Interfaces;
using WearAndTear.Code.XLib.Containers;
using WearAndTear.Config.Props;

namespace WearAndTear.Code.Behaviours
{
    public class WearAndTearPartBehavior : BlockEntityBehavior, IWearAndTearPart
    {
        public IWearAndTear WearAndTear { get; private set; }
        protected IDictionary<string, IDecayEngine> DecayEngines { get; private set; }

        public WearAndTearPartBehavior(BlockEntity blockentity) : base(blockentity)
        {
        }
        
        public PartBonuses PartBonuses { get; private set; }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            Props ??= properties.AsObject<WearAndTearPartProps>() ?? new();
            Props.Decay ??= new WearAndTearDecayProps[]
            {
                new() {
                    Type = "time"
                }
            };
            DecayEngines = Api.ModLoader.GetModSystem<WearAndTearModSystem>().DecayEngines;
            WearAndTear = Blockentity.GetBehavior<IWearAndTear>();
            if(WearAndTearModSystem.XlibEnabled) PartBonuses ??= new();
        }

        public override void OnBlockPlaced(ItemStack byItemStack = null)
        {
            base.OnBlockPlaced(byItemStack);

            var tree = byItemStack?.Attributes?.GetTreeAttribute("WearAndTear-Durability");
            if (tree != null)
            {
                Durability = tree.GetFloat(Props.Code, Durability);
                RepairedDurability = tree.GetFloat(Props.Code + "_Repaired", RepairedDurability);
            }
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);

            //This is to deal with this method being called before Initialize
            Props ??= properties.AsObject<WearAndTearPartProps>() ?? new();
            if(WearAndTearModSystem.XlibEnabled) PartBonuses ??= new();

            if (Props == null) return;
            
            var durabilityTree = tree.GetOrAddTreeAttribute("WearAndTear-Durability");
            Durability = durabilityTree.GetFloat(Props.Code, Durability);
            if (HasMaintenanceLimit) RepairedDurability = durabilityTree.GetFloat(Props.Code + "_Repaired", RepairedDurability);

            PartBonuses?.FromTreeAttributes(tree, Props);
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            
            var durabilityTree = tree.GetOrAddTreeAttribute("WearAndTear-Durability");
            durabilityTree.SetFloat(Props.Code, Durability);
            if (HasMaintenanceLimit) durabilityTree.SetFloat(Props.Code + "_Repaired", RepairedDurability);
            PartBonuses?.ToTreeAttributes(tree, Props);
        }

        public virtual void GetWearAndTearInfo(IPlayer forPlayer, StringBuilder dsc) => dsc.AppendLine(GetDurabilityStringForPlayer(forPlayer));

        public virtual string GetDisplayName() => Props.GetDisplayName();

        public string GetDurabilityStringForPlayer(IPlayer player) => $"{GetDisplayName()}: {WearAndTearModSystem.IsRoughEstimateEnabled(Api, player) switch
        {
            true when Durability > 0.7 => Lang.Get("wearandtear:durability-good"),
            true when Durability > 0.4 => Lang.Get("wearandtear:durability-decent"),
            true when Durability > 0.1 => Lang.Get("wearandtear:durability-bad"),
            true => Lang.Get("wearandtear:durability-critical"),
            _ => $"{(int)(Durability * 100)}%"
        }}";

        public virtual void UpdateDecay(double daysPassed)
        {
            foreach (var decay in Props.Decay)
            {
                var loss = DecayEngines[decay.Type].GetDecayLoss(Api, this, decay, daysPassed) / Props.Decay.Length;

                foreach (var protectivePart in WearAndTear.Parts.OfType<IWearAndTearProtectivePart>())
                {
                    if (protectivePart is IWearAndTearOptionalPart optionalPart && !optionalPart.IsPresent) continue;

                    loss *= protectivePart.GetDecayMultiplierFor(Props);
                }

                if(PartBonuses != null) loss *= PartBonuses.DecayModifier;
                Durability -= loss;
            }
            Durability = GameMath.Clamp(Durability, WearAndTearModSystem.Config.MinDurability, 1);
        }

        public WearAndTearPartProps Props { get; private set; }

        private float _durability = 1;

        public virtual float Durability
        {
            get => _durability;
            set
            {
                if (float.IsNaN(value))
                {
                    if(WearAndTearModSystem.Config.EnableDebugLogging) Api?.Logger.Warning($"[WearAndTear] Invalid Durability assignment at {Pos} for {Props?.MaterialVariant} {Props?.Code} (ignoring attempt)");
                    return;
                }
                _durability = value;
            }
        }
        public float RepairedDurability { get; set; } = 0;

        //HACK mod system is disposed before ToTreeAttributes is called resulting in Config being null...
        public bool HasMaintenanceLimit => !(WearAndTearModSystem.Config?.AllowForInfiniteMaintenance ?? false) && Props.MaintenanceLimit != null;

        public bool IsActive
        {
            get
            {
                var powerDevice = Blockentity.GetBehavior<IMechanicalPowerDevice>();
                return powerDevice?.Network != null && powerDevice.Network.Speed > 0.001;
            }
        }

        public virtual ItemStack[] ModifyDroppedItemStacks(ItemStack[] itemStacks, IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier, bool isBlockDestroyed)
        {
            var cfg = WearAndTearModSystem.Config.Rubble;

            if (isBlockDestroyed && cfg.GenerateScrap && Props.ScrapCode != null)
            {
                var scrap = Api.World.GetItem(Props.ScrapCode);
                if(scrap != null && !scrap.IsMissing)
                {
                    var factor = (Durability * cfg.DurabilityDropPercentage) + cfg.FixedDropPercentage;
                    var item = new ItemStack(scrap)
                    {
                        StackSize = (int)(GameMath.Clamp(Props.ContentLevel * factor, 0, Props.ContentLevel) * dropQuantityMultiplier)
                    };

                    //TODO XSkills
                    if(item.StackSize > 0) itemStacks = itemStacks.Append(item);
                }
            }
            return itemStacks;
        }
    }
}