using System;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;
using WearAndTear.Code.Interfaces;
using WearAndTear.Config.Props;

namespace WearAndTear.Code.Behaviours.Parts
{
    public class WearAndTearMoldPartBehavior : WearAndTearPartBehavior , IWearAndTearPart
    {
        public WearAndTearMoldPartBehavior(BlockEntity blockentity) : base(blockentity)
        {
        }

        public WearAndTearDurabilityPartProps DurabilityProps { get; private set; }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            DurabilityProps ??= properties.AsObject<WearAndTearDurabilityPartProps>() ?? new();
        }

        public bool RequiresUpdateDecay => false;

        public bool OnBreak()
        {
            var canShatter = Block.Attributes != null && Block.Attributes["shatteredShape"].Exists;
            if(!canShatter) return true;
            if(Blockentity is BlockEntityToolMold toolMold)
            {
                if (!toolMold.Shattered)
                {
                    Durability = 1; //Reset durability so it won't create breakage decal
                    Api.World.PlaySoundAt(new AssetLocation("sounds/block/ceramicbreak"), Pos, -0.4, null, true, 32f, 1f);
				    Block.SpawnBlockBrokenParticles(Pos);
				    Block.SpawnBlockBrokenParticles(Pos);
                    toolMold.MetalContent = null;
				    toolMold.Shattered = true;
                    toolMold.UpdateRenderer();
				    toolMold.MarkDirty(true);
                }
                return false;
            }
            
            return true;
        }

        public void Damage()
        {
            float damage = (float)(DurabilityProps.MinDurabilityUsage + (Api.World.Rand.NextDouble() * (DurabilityProps.MaxDurabilityUsage - DurabilityProps.MinDurabilityUsage)));

            foreach (var protectivePart in WearAndTear.Parts.OfType<IWearAndTearProtectivePart>())
            {
                if (protectivePart is IWearAndTearOptionalPart optionalPart && !optionalPart.IsPresent) continue;

                var protection = Array.Find(protectivePart!.ProtectiveProps.EffectiveFor, target => target.IsEffectiveFor(Props));
                if (protection != null)
                {
                    damage *= protection.DecayMultiplier;
                }
            }
            Durability -= damage;
            Blockentity.GetBehavior<WearAndTearBehavior>().UpdateDecay(0, false);
        }

        public override void UpdateDecay(double daysPassed)
        {
            //Molds have manual decay
        }

        public override void GetWearAndTearInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            if(Blockentity is BlockEntityToolMold toolMold && toolMold.Shattered) return;
            base.GetWearAndTearInfo(forPlayer, dsc);
        }
    }
}
