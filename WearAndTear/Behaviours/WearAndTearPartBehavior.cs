using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using WearAndTear.Config.Props;
using WearAndTear.Interfaces;

namespace WearAndTear.Behaviours
{
    public class WearAndTearPartBehavior : BlockEntityBehavior, IWearAndTearPart
    {
        protected IDictionary<string, IDecayEngine> DecayEngines { get; private set; }

        public WearAndTearPartBehavior(BlockEntity blockentity) : base(blockentity)
        {
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            Props ??= properties.AsObject<WearAndTearPartProps>();
            DecayEngines = Api.ModLoader.GetModSystem<WearAndTearModSystem>().DecayEngines;
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            Props ??= properties.AsObject<WearAndTearPartProps>(); //This is to deal with this method being called before Initialize
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
            foreach (var decay in Props.Decay) Durability -= DecayEngines[decay.Type].GetDecayLoss(Api, this, decay, daysPassed);
            Durability = Math.Max(Durability, WearAndTearModSystem.Config.MinDurability);
        }

        public WearAndTearPartProps Props { get; private set; }

        public virtual float Durability { get; set; } = 1;
    }
}