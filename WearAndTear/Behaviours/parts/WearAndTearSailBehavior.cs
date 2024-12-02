using HarmonyLib;
using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Interfaces;

namespace WearAndTear.Behaviours.Parts
{
    public class WearAndTearSailBehavior : WearAndTearPartBehavior, IWearAndTearPart
    {
        public WearAndTearSailBehavior(BlockEntity blockentity) : base(blockentity)
        {
        }

        //TODO think of a neater solution
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

        public int BladeCount { get; private set; }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            var bladeType = Block.FirstCodePart(1).ToString();
            BladeCount = bladeType switch
            {
                "double" => 8,
                "three" => 3,
                "six" => 6,
                _ => 4,
            };

            base.Initialize(api, properties);
        }

        private int? prevSailLength;

        public override void UpdateDecay(double daysPassed)
        {
            //Fix sail durability when more sails have been added
            var sailLength = SailLength;
            if (prevSailLength != null && prevSailLength < sailLength)
            {
                var addedSails = sailLength - prevSailLength.Value;
                Durability = (Durability * prevSailLength.Value + addedSails) / sailLength;
            }
            prevSailLength = sailLength;

            if (sailLength == 0)
            {
                Durability = 1;
                return;
            }
            base.UpdateDecay(daysPassed);
        }

        public override void GetWearAndTearInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            if (SailLength == 0) return;
            base.GetWearAndTearInfo(forPlayer, dsc);
        }

        public void DropSails()
        {
            if (Durability > WearAndTearModSystem.Config.DurabilityLeeway) Durability = 1;
            var item = SailAssetLocation;
            var sailCount = BladeCount;
            var sailLength = SailLength;

            var sailItemCount = sailLength * sailCount * Durability;

            if (sailItemCount > 0)
            {
                while (sailItemCount >= 1)
                {
                    var stackSize = (int)Math.Min(sailItemCount, sailCount);
                    sailItemCount -= stackSize;

                    Api.World.SpawnItemEntity(new ItemStack(Api.World.GetItem(new AssetLocation(item)), stackSize), Pos.ToVec3d().Add(0.5, 0.5, 0.5), null);
                }

                if (sailItemCount > 0)
                {
                    sailItemCount *= sailCount;
                }

                while (sailItemCount >= 1)
                {
                    var stackSize = (int)Math.Min(sailItemCount, 32);
                    sailItemCount -= stackSize;

                    Api.World.SpawnItemEntity(new ItemStack(Api.World.GetItem(new AssetLocation("flaxtwine")), stackSize), Pos.ToVec3d().Add(0.5, 0.5, 0.5), null);
                }
            }

            SailLength = 0;
            Durability = 1;
            Blockentity.MarkDirty(true);
        }

        public float DoMaintenanceFor(float repairStrength)
        {
            var realRepairStrength = repairStrength * (4f / (SailLength * BladeCount));
            Durability += realRepairStrength;

            float realLeftOver = (Durability - 1f) * SailLength * BladeCount / 4f;

            Durability = Math.Min(Durability, 1);
            return Math.Min(realLeftOver, 0);
        }

        public virtual void UpdateShape(BEBehaviorMPBase beh, string typeVariant)
        {
            //TODO revamp this to be more generic
            if (Api == null) return;
            int? durabilityVariant = null;

            if (Durability < 0.05) durabilityVariant = 0;
            else if (Durability < 0.50) durabilityVariant = 50;
            else if (Durability < 0.75) durabilityVariant = 75;

            if (durabilityVariant == null) return;

            var newLocation = beh.Shape.Base.Clone();
            newLocation.Path = $"{newLocation.Path}-{typeVariant}-{durabilityVariant}";

            var oldShape = beh.Shape;
            try
            {
                beh.Shape = new CompositeShape
                {
                    Base = newLocation,
                    rotateY = Block.Shape.rotateY
                };
            }
            catch //Just in case the shape doesn't exist
            {
                beh.Shape = oldShape;
            }
        }
    }
}