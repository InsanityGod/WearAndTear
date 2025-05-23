﻿using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;
using WearAndTear.Code.Interfaces;
using WearAndTear.Code.XLib;
using WearAndTear.Config.Props;
using WearAndTear.Config.Server;

namespace WearAndTear.Code.Behaviours.Parts
{
    public class MoldPart : Part
    {
        public MoldPart(BlockEntity blockentity) : base(blockentity) {}

        public DurabilityUsageProps DurabilityProps { get; private set; }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            DurabilityProps ??= properties.AsObject<DurabilityUsageProps>() ?? new();
        }

        public override bool RequiresUpdateDecay => false;

        public override bool OnBreak()
        {
            var canShatter = Block.Attributes != null && Block.Attributes["shatteredShape"].Exists;
            if (!canShatter) return true;
            if (Blockentity is BlockEntityToolMold toolMold)
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

        public void Damage(IPlayer byPlayer)
        {
            float damage = WearAndTearModSystem.XlibEnabled && SkillsAndAbilities.IsExpertCaster(Api, byPlayer) ?
                DurabilityProps.MinDurabilityUsage :
                (float)(DurabilityProps.MinDurabilityUsage + (Api.World.Rand.NextDouble() * (DurabilityProps.MaxDurabilityUsage - DurabilityProps.MinDurabilityUsage)));

            damage *= DecayModifiersConfig.Instance.Mold;
            foreach (var protectivePart in Controller.Parts.OfType<IProtectivePart>())
            {
                if (protectivePart is IOptionalPart optionalPart && !optionalPart.IsPresent) continue;

                damage *= protectivePart.GetDecayMultiplierFor(Props);
            }

            if (WearAndTearModSystem.XlibEnabled) damage = SkillsAndAbilities.ApplyMoldDurabilityCostModifier(Api, byPlayer, damage);
            Durability -= damage;

            Blockentity.GetBehavior<PartController>().UpdateDecay(0, false);
        }

        public override void UpdateDecay(double daysPassed)
        {
            //Molds have manual decay
        }

        public override void GetWearAndTearInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            if (Blockentity is BlockEntityToolMold toolMold && toolMold.Shattered) return;
            base.GetWearAndTearInfo(forPlayer, dsc);
        }
    }
}