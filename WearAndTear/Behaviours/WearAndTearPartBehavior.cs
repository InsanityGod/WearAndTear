using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Config.Props;
using WearAndTear.Interfaces;

namespace WearAndTear.Behaviours
{
    public class WearAndTearPartBehavior : BlockEntityBehavior, IWearAndTearPart
    {
        public IWearAndTear WearAndTear { get; private set; }
        protected IDictionary<string, IDecayEngine> DecayEngines { get; private set; }

        public WearAndTearPartBehavior(BlockEntity blockentity) : base(blockentity)
        {
        }

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
        }

        public override void OnBlockPlaced(ItemStack byItemStack = null)
        {
            base.OnBlockPlaced(byItemStack);

            var tree = byItemStack?.Attributes?.GetTreeAttribute("WearAndTear-Durability");
            if(tree != null)
            {
                Durability = tree.GetFloat(Props.Name, Durability);
            }
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            Props ??= properties.AsObject<WearAndTearPartProps>() ?? new(); //This is to deal with this method being called before Initialize
            if (Props == null) return;
            Durability = tree.GetOrAddTreeAttribute("WearAndTear-Durability").GetFloat(Props.Name, Durability);
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.GetOrAddTreeAttribute("WearAndTear-Durability").SetFloat(Props.Name, Durability);
        }

        public virtual void GetWearAndTearInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            dsc.AppendLine($"{Lang.Get(Props.Name)}: {(int)(Durability * 100)}%");
        }

        public virtual void UpdateDecay(double daysPassed)
        {
            foreach (var decay in Props.Decay)
            {
                var loss = DecayEngines[decay.Type].GetDecayLoss(Api, this, decay, daysPassed) / Props.Decay.Length;

                foreach (var protectivePart in WearAndTear.Parts.OfType<IWearAndTearProtectivePart>())
                {
                    if(protectivePart.Durability <= 0) continue;
                    var protection = Array.Find(protectivePart.ProtectiveProps.EffectiveFor, target => target.IsEffectiveFor(Props));
                    if (protection != null)
                    {
                        loss *= protection.DecayMultiplier;
                    }
                }

                Durability -= loss;
            }
            Durability = GameMath.Clamp(Durability, WearAndTearModSystem.Config.MinDurability, 1);
        }

        public WearAndTearPartProps Props { get; private set; }
        public virtual float Durability { get; set; } = 1;

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