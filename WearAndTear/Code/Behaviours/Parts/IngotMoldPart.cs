using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;
using WearAndTear.Code.Behaviours.Parts.Abstract;
using WearAndTear.Code.Enums;
using WearAndTear.Code.Interfaces;
using WearAndTear.Code.XLib;
using WearAndTear.Config.Props;
using WearAndTear.Config.Server;

namespace WearAndTear.Code.Behaviours.Parts
{
    public class IngotMoldPart : OptionalPart
    {
        public BlockEntityIngotMold IngotMoldEntity { get; private set; }

        public EIngotMoldSide Side { get; private set; }

        public IngotMoldPart(BlockEntity blockentity) : base(blockentity)
        {
            IngotMoldEntity = (BlockEntityIngotMold)blockentity;
        }

        public DurabilityUsageProps DurabilityProps { get; private set; }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            DurabilityProps ??= properties.AsObject<DurabilityUsageProps>() ?? new();

            var index = Blockentity.Behaviors.OfType<IngotMoldPart>().ToList().FindIndex(x => x == this);
            if (index == 0)
            {
                Side = EIngotMoldSide.Left;
                if(Durability == 0 && IngotMoldEntity.MoldLeft is not null) Durability = 1;
            }
            else if (index == 1)
            {
                if(Durability == 0 && IngotMoldEntity.MoldRight is not null) Durability = 1;
                Side = EIngotMoldSide.Right;
            }
        }

        public override bool RequiresUpdateDecay => false;

        public override bool OnBreak()
        {
            if ((Side == EIngotMoldSide.Left && IngotMoldEntity.ShatteredLeft) || (Side == EIngotMoldSide.Right && IngotMoldEntity.ShatteredRight)) return false;

            Durability = 1; //Reset durability so it won't create breakage decal
            Api.World.PlaySoundAt(new AssetLocation("sounds/block/ceramicbreak"), Pos, -0.4, null, true, 32f, 1f);
            Block.SpawnBlockBrokenParticles(Pos);
            Block.SpawnBlockBrokenParticles(Pos);

            if (Side == EIngotMoldSide.Left)
            {
                IngotMoldEntity.ShatteredLeft = true;
                IngotMoldEntity.ContentsLeft = null;
            }
            else if (Side == EIngotMoldSide.Right)
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

        //TODO Refactor this
        public override void OnBlockPlaced(ItemStack byItemStack = null)
        {
            base.OnBlockPlaced(byItemStack);

            if (Side == EIngotMoldSide.Left)
            {
                var tree = byItemStack?.Attributes?.GetTreeAttribute(Constants.DurabilityTreeName);
                Durability = tree?.GetFloat("Mold", 1) ?? 1;
            }
        }

        public override bool IsPresent
        {
            get
            {
                if ((Side == EIngotMoldSide.Left && IngotMoldEntity.ShatteredLeft) || (Side == EIngotMoldSide.Right && IngotMoldEntity.ShatteredRight)) return false;
                if (Side == EIngotMoldSide.Right && IngotMoldEntity.QuantityMolds < 2) return false;
                return true;
            }
        }
    }
}