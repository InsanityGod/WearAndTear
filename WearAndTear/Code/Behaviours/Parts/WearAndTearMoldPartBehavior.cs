using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;
using WearAndTear.Code.Interfaces;

namespace WearAndTear.Code.Behaviours.Parts
{
    public class WearAndTearMoldPartBehavior : WearAndTearPartBehavior , IWearAndTearPart
    {
        public WearAndTearMoldPartBehavior(BlockEntity blockentity) : base(blockentity)
        {
        }

        public bool RequiresUpdateDecay => false;

        public bool OnBreak()
        {
            if(Blockentity is BlockEntityToolMold toolMold)
            {
                Api.World.PlaySoundAt(new AssetLocation("sounds/block/ceramicbreak"), Pos, -0.4, null, true, 32f, 1f);
				Block.SpawnBlockBrokenParticles(Pos);
				Block.SpawnBlockBrokenParticles(Pos);
                toolMold.MetalContent = null;
				toolMold.Shattered = true;
                toolMold.UpdateRenderer();
				toolMold.MarkDirty(true);
                //Api.World.BlockAccessor.SetBlock(0, Pos);
                return false;
            }
            
            return true;
        }

        public void Damage()
        {
            var damage = Api.World.Rand.Next(16, 32);
            Durability -= damage * 0.01f;
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
