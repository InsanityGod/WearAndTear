using System;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;
using WearAndTear.Code.Behaviours.Parts.Abstract;
using WearAndTear.Code.Enums;
using WearAndTear.Code.Interfaces;
using WearAndTear.Code.XLib;
using WearAndTear.Config.Props;

namespace WearAndTear.Code.Behaviours.Parts
{
    public class WearAndTearIngotMoldPartBehavior : WearAndTearOptionalPartBehavior, IWearAndTearPart
    {
        public BlockEntityIngotMold IngotMoldEntity { get; private set; }

        public EIngotMoldSide Side { get; private set; }

        public WearAndTearIngotMoldPartBehavior(BlockEntity blockentity) : base(blockentity)
        {
            IngotMoldEntity = (BlockEntityIngotMold)blockentity;
        }

        public WearAndTearDurabilityPartProps DurabilityProps { get; private set; }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            DurabilityProps ??= properties.AsObject<WearAndTearDurabilityPartProps>() ?? new();
            
            var index = Blockentity.Behaviors.OfType<WearAndTearIngotMoldPartBehavior>().ToList().FindIndex(x => x == this);
            if(index == 0)
            {
                Side = EIngotMoldSide.Left;
            }
            else if(index == 1)
            {
                Side = EIngotMoldSide.Right;
            }
        }

        public bool RequiresUpdateDecay => false;

        public bool OnBreak()
        {
            if((Side == EIngotMoldSide.Left && IngotMoldEntity.ShatteredLeft) || (Side == EIngotMoldSide.Right && IngotMoldEntity.ShatteredRight)) return false;

            Durability = 1; //Reset durability so it won't create breakage decal
            Api.World.PlaySoundAt(new AssetLocation("sounds/block/ceramicbreak"), Pos, -0.4, null, true, 32f, 1f);
            Block.SpawnBlockBrokenParticles(Pos);
            Block.SpawnBlockBrokenParticles(Pos);

            if(Side == EIngotMoldSide.Left)
            {
                IngotMoldEntity.ShatteredLeft = true;
                IngotMoldEntity.ContentsLeft = null;
            }
            else if(Side == EIngotMoldSide.Right)
            {
                IngotMoldEntity.ShatteredRight = true;
                IngotMoldEntity.ContentsRight = null;
            }
            IngotMoldEntity.UpdateIngotRenderer();
            IngotMoldEntity.MarkDirty(true);

            return false;
        }

        public void Damage(IPlayer byPlayer)
        {
            float damage = WearAndTearModSystem.XlibEnabled && SkillsAndAbilities.IsExpertCaster(Api, byPlayer) ?
                DurabilityProps.MinDurabilityUsage :
                (float)(DurabilityProps.MinDurabilityUsage + (Api.World.Rand.NextDouble() * (DurabilityProps.MaxDurabilityUsage - DurabilityProps.MinDurabilityUsage)));

            damage *= WearAndTearModSystem.Config.DecayModifier.Mold;

            foreach (var protectivePart in WearAndTear.Parts.OfType<IWearAndTearProtectivePart>())
            {
                if (protectivePart is IWearAndTearOptionalPart optionalPart && !optionalPart.IsPresent) continue;

                damage *= protectivePart.GetDecayMultiplierFor(Props);
            }

            if (WearAndTearModSystem.XlibEnabled) damage = SkillsAndAbilities.ApplyMoldDurabilityCostModifier(Api, byPlayer, damage);
            Durability -= damage;
            
            Blockentity.GetBehavior<WearAndTearBehavior>().UpdateDecay(0, false);
        }

        public override void UpdateDecay(double daysPassed)
        {
            //Molds have manual decay
        }


        //TODO Refactor this
        public override void OnBlockPlaced(ItemStack byItemStack = null)
        {
            base.OnBlockPlaced(byItemStack);

            var tree = byItemStack?.Attributes?.GetTreeAttribute("WearAndTear-Durability");

            if (tree != null && Side == EIngotMoldSide.Left) 
            {
                Durability = tree.GetFloat("Mold", Durability);
            }
        }

        public override bool IsPresent
        {
            get
            {
                if((Side == EIngotMoldSide.Left && IngotMoldEntity.ShatteredLeft) || (Side == EIngotMoldSide.Right && IngotMoldEntity.ShatteredRight)) return false;
                if(Side == EIngotMoldSide.Right && IngotMoldEntity.QuantityMolds < 2) return false;
                return true;
            }
        }
    }
}