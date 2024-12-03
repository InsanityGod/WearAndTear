﻿using HarmonyLib;
using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Behaviours.Parts.Abstract;
using WearAndTear.Config.Props;
using WearAndTear.Interfaces;

namespace WearAndTear.Behaviours.Parts
{
    public class WearAndTearSailBehavior : WearAndTearOptionalPartBehavior, IWearAndTearPart
    {
        public WearAndTearSailBehavior(BlockEntity blockentity) : base(blockentity)
        {
        }

        public bool AreSailsRolledUp { get; set; } = false;

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            tree.SetBool("AreSailsRolledUp", AreSailsRolledUp);
            base.ToTreeAttributes(tree);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            AreSailsRolledUp = tree.GetAsBool("AreSailsRolledUp", AreSailsRolledUp);
            base.FromTreeAttributes(tree, worldAccessForResolve);
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

        public bool CanDoMaintenanceWith(WearAndTearRepairItemProps props) => IsPresent && props.RepairType == Props.RepairType;

        public override bool IsPresent => CachedSailLength != 0;

        public int? CachedSailLength { get; set; }

        public override void UpdateDecay(double daysPassed)
        {
            //Fix sail durability when more sails have been added
            var sailLength = SailLength;
            if(CachedSailLength != null && sailLength != CachedSailLength)
            {
                if(CachedSailLength == 0 && sailLength > 0)
                {
                    Durability = 1;
                }
                else if (CachedSailLength < sailLength)
                {
                    var addedSails = sailLength - CachedSailLength.Value;
                    Durability = (Durability * CachedSailLength.Value + addedSails) / sailLength;
                }
            }

            CachedSailLength = sailLength;
            if(AreSailsRolledUp) return;
            base.UpdateDecay(daysPassed);
        }

        public void DropSails()
        {
            if(!IsPresent) return;

            if (Durability > WearAndTearModSystem.Config.DurabilityLeeway) Durability = 1;
            var item = SailAssetLocation;
            var sailCount = BladeCount;
            var sailLength = SailLength;

            var sailItemCount = sailLength * sailCount * Durability;

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
            

            SailLength = 0;
            Durability = 1;
            Blockentity.MarkDirty(true);
        }

        public override void GetWearAndTearInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            if(!IsPresent) return;
            dsc.AppendLine($"{Lang.Get(Props.Name)}{(AreSailsRolledUp ? " (Rolled Up)" : "")}: {(int)(Durability * 100)}%");
        }

        public float? EfficiencyModifier
        {
            get
            {
                if(AreSailsRolledUp) return 0;
                return Props.DurabilityEfficiencyRatio == 0 ? null : 1 - ((1f - Durability) * Props.DurabilityEfficiencyRatio);
            }
        }

        public float DoMaintenanceFor(float repairStrength)
        {
            var realRepairStrength = repairStrength * (4f / (SailLength * BladeCount));
            Durability += realRepairStrength;

            float realLeftOver = (Durability - 1f) * SailLength * BladeCount / 4f;

            Durability = Math.Min(Durability, 1);
            return Math.Min(realLeftOver, 0);
        }

        public virtual void UpdateShape(BEBehaviorMPBase beh)
        {
            //TODO revamp this to be more generic
            if (Api == null) return;
            AssetLocation newLocation = null;

            if (AreSailsRolledUp)
            {
                    newLocation = beh.Shape.Base.Clone();
                newLocation.Path = $"{newLocation.Path}-rolledup";
            }
            else 
            {
                int? durabilityVariant = null;

                if (Durability < 0.05) durabilityVariant = 0;
                else if (Durability < 0.50) durabilityVariant = 50;
                else if (Durability < 0.75) durabilityVariant = 75;

                if(durabilityVariant != null)
                {
                    newLocation = beh.Shape.Base.Clone();
                    newLocation.Path = $"{newLocation.Path}-torn-{durabilityVariant}";
                }
            }
            if(newLocation == null) return;

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