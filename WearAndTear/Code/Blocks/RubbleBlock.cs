using System;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using WearAndTear.Code.BlockEntities;
using WearAndTear.Code.XLib;
using WearAndTear.Config.Props.rubble;

namespace WearAndTear.Code.Blocks
{
    public class RubbleBlock : Block
    {
        public override void OnLoaded(ICoreAPI api)
		{
			base.OnLoaded(api);
			sprintIntoDamage = Attributes["sprintIntoDamage"].AsFloat(WearAndTearModSystem.Config.Rubble.SprintIntoDamage);
			fallIntoDamageMul = Attributes["fallIntoDamageMul"].AsFloat(WearAndTearModSystem.Config.Rubble.FallIntoDamageMul);
		}

        public override bool CanAcceptFallOnto(IWorldAccessor world, BlockPos pos, Block fallingBlock, TreeAttribute blockEntityAttributes)
        {
            if(fallingBlock is RubbleBlock) return true;
            return base.CanAcceptFallOnto(world, pos, fallingBlock, blockEntityAttributes);
        }

        public override bool OnFallOnto(IWorldAccessor world, BlockPos pos, Block block, TreeAttribute blockEntityAttributes)
        {
            var mainContentsTree = world.BlockAccessor.GetBlockEntity<RubbleBlockEntity>(pos)?.Contents;
            var contentsTree = blockEntityAttributes.GetTreeAttribute("contents");
            if(mainContentsTree != null && contentsTree != null)
            {
                var lastAvailableIndex = mainContentsTree.Count;
                foreach (var item in contentsTree.Values)
                {
                    mainContentsTree[$"{lastAvailableIndex++}"] = item;
                }
            }
            return base.OnFallOnto(world, pos, block, blockEntityAttributes);
        }

        public override Cuboidf[] GetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            var entity = blockAccessor.GetBlockEntity<RubbleBlockEntity>(pos);
            if(entity != null)
            {
                var content = entity.PrimaryContent;
                if(content != null)
                {
                    if(content.Collectible == null) content.ResolveBlockOrItem(api.World);
                    if(content.Block?.Attributes != null)
                    {
                        var result = content.Block.Attributes[WearAndTearRubbleProps.Key][nameof(WearAndTearRubbleProps.CollisionSelectionBoxes)].AsObject<Cuboidf[]>();
                        if(result != null) return result;
                    }
                }
                
            }
            return base.GetCollisionBoxes(blockAccessor, pos);
        }

        public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos) => GetCollisionBoxes(blockAccessor, pos);

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            //redirect to entity
            var entity = world.BlockAccessor.GetBlockEntity<RubbleBlockEntity>(pos);
            if(entity == null)
            {
                api.Logger.Error("[WearAndTear] RubbleBlock does not have RubbleBlockEntity");
                return Array.Empty<ItemStack>();
            }

            var results = entity.GetDrops(world, byPlayer, dropQuantityMultiplier);

            foreach(var item in results)
            {
                if(item.Collectible != null) continue;
                item.ResolveBlockOrItem(world);
            }

            return results;
        }

        public override void OnEntityInside(IWorldAccessor world, Entity entity, BlockPos pos)
		{
            var rubbleEntity = world.BlockAccessor.GetBlockEntity<RubbleBlockEntity>(pos);
            if (rubbleEntity != null && rubbleEntity.DamageOnTouch() && world.Side == EnumAppSide.Server && entity is EntityPlayer player && player.ServerControls.Sprint && entity.ServerPos.Motion.LengthSq() > 0.001 && world.Rand.NextDouble() > 0.05)
            {
                var damage = sprintIntoDamage;
                if(WearAndTearModSystem.XlibEnabled) damage = SkillsAndAbilities.ApplyStrongFeetBonus(api, player.Player, damage);
                if (damage > 0)
                {
                    entity.ReceiveDamage(new DamageSource
                    {
                        Source = EnumDamageSource.Block,
                        SourceBlock = this,
                        Type = EnumDamageType.PiercingAttack,
                        SourcePos = pos.ToVec3d()
                    }, damage);
                    entity.ServerPos.Motion.Set(0.0, 0.0, 0.0);
                }
            }
            base.OnEntityInside(world, entity, pos);
		}

		public override void OnEntityCollide(IWorldAccessor world, Entity entity, BlockPos pos, BlockFacing facing, Vec3d collideSpeed, bool isImpact)
		{
            var rubbleEntity = world.BlockAccessor.GetBlockEntity<RubbleBlockEntity>(pos);
			if (rubbleEntity != null && rubbleEntity.DamageOnTouch() && world.Side == EnumAppSide.Server && entity is EntityPlayer player && isImpact && Math.Abs(collideSpeed.Y * 30.0) >= 0.25)
			{
                var damage = (float)Math.Abs(collideSpeed.Y * (double)fallIntoDamageMul);
                if(WearAndTearModSystem.XlibEnabled) damage = SkillsAndAbilities.ApplyStrongFeetBonus(api, player.Player, damage);
                if (damage < 0) return;

				entity.ReceiveDamage(new DamageSource
				{
					Source = EnumDamageSource.Block,
					SourceBlock = this,
					Type = EnumDamageType.PiercingAttack,
					SourcePos = pos.ToVec3d()
				}, damage);
			}
		}

		private float sprintIntoDamage;

		private float fallIntoDamageMul;
    }
}
