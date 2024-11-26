using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent.Mechanics;

namespace WearAndTear.Behaviours
{
    public class WearAndTearSailBlockEntityBehavior : WearAndTearBlockEntityBehavior
    {
        public WearAndTearSailBlockEntityBehavior(BlockEntity blockentity) : base(blockentity)
        {
        }

        public int SailLength
        {
            get
            {
                var beh = Blockentity.GetBehavior<BEBehaviorMPRotor>();
                if (beh is BEBehaviorWindmillRotor rotor)
                {
                    return rotor.SailLength;
                }
                return Traverse.Create(beh).Property("SailLength").GetValue<int>();
            }
            set
            {
                var beh = Blockentity.GetBehavior<BEBehaviorMPRotor>();
                if (beh is BEBehaviorWindmillRotor)
                {
                    Traverse.Create(beh).Field("sailLength").SetValue(value);
                    return;
                }
                Traverse.Create(beh).Property("SailLength").SetValue(value);
            }
        }

        public int BladeCount
        {
            get
            {
                var beh = Blockentity.GetBehavior<BEBehaviorMPRotor>();
                if (beh is BEBehaviorWindmillRotor)
                {
                    return 4;
                }
                return Traverse.Create(beh).Field("bladeCount").GetValue<int>();
            }
        }

        public string SailAssetLocation
        {
            get
            {
                var beh = Blockentity.GetBehavior<BEBehaviorMPRotor>();
                if (beh is BEBehaviorWindmillRotor)
                {
                    return "sail";
                }

                var traverse = Traverse.Create(beh);
                var sailType = traverse.Property("SailType").GetValue<string>();
                if (string.IsNullOrEmpty(sailType)) sailType = "sailcentered";

                return $"millwright:{sailType}";
            }
        }

        private int? prevSailLength;

        public override void UpdateDurability()
        {
            base.UpdateDurability();
            var sailLength = SailLength;
            if (prevSailLength != null && prevSailLength < sailLength)
            {
                var addedSails = sailLength - prevSailLength.Value;
                Durability = ((Durability * prevSailLength.Value) + addedSails) / sailLength;
            }

            prevSailLength = sailLength;
        }

        public static void DropSails(ICoreAPI api, WearAndTearSailBlockEntityBehavior wearAndTearBehaviour, BlockPos pos)
        {
            if (wearAndTearBehaviour.Durability > WearAndTearModSystem.Config.DurabilityLeeway) wearAndTearBehaviour.Durability = 1;
            var item = wearAndTearBehaviour.SailAssetLocation;
            var sailCount = wearAndTearBehaviour.BladeCount;
            var sailLength = wearAndTearBehaviour.SailLength;

            var sailItemCount = sailLength * sailCount * wearAndTearBehaviour.Durability;

            if (sailItemCount > 0)
            {
                while (sailItemCount >= 1)
                {
                    var stackSize = (int)Math.Min(sailItemCount, sailCount);
                    sailItemCount -= stackSize;

                    api.World.SpawnItemEntity(new ItemStack(api.World.GetItem(new AssetLocation(item)), stackSize), pos.ToVec3d().Add(0.5, 0.5, 0.5), null);
                }

                if (sailItemCount > 0)
                {
                    sailItemCount *= sailCount;
                }

                while (sailItemCount >= 1)
                {
                    var stackSize = (int)Math.Min(sailItemCount, 32);
                    sailItemCount -= stackSize;

                    api.World.SpawnItemEntity(new ItemStack(api.World.GetItem(new AssetLocation("flaxtwine")), stackSize), pos.ToVec3d().Add(0.5, 0.5, 0.5), null);
                }
            }
        }

        public override bool TryRepair(float repairStrength) => base.TryRepair(repairStrength * (4f / (SailLength * BladeCount)));
    }
}