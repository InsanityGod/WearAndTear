using HarmonyLib;
using InsanityLib.Util;
using InsanityLib.Util.SpanUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code.Behaviours.Parts.Abstract;
using WearAndTear.Code.XLib;
using WearAndTear.Config.Props;
using WearAndTear.Config.Server;

namespace WearAndTear.Code.Behaviours.Parts
{
    public class WindmillSailPart : OptionalPart
    {
        public WindmillSailPart(BlockEntity blockentity) : base(blockentity) { }

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

        private BlockEntityBehavior GetRotor()
        {
            var beh = Blockentity.GetBehavior<BEBehaviorMPRotor>();
            return beh ?? Blockentity.Behaviors.Find(b => b.GetType().Name.Contains("WindmillRotor"));
        }

        public int SailLength
        {
            get
            {
                var beh = GetRotor();
                if (beh is BEBehaviorWindmillRotor rotor)
                {
                    return rotor.SailLength;
                }
                //TODO maybe get rid of these traverses and add some extra checks that only run when MillWright is enabled
                return Traverse.Create(beh).Property("SailLength").GetValue<int>();
            }
            set
            {
                var beh = GetRotor();
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
                var beh = GetRotor();
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

        public override bool CanRepairWith(RepairItemProps props) => IsPresent && base.CanRepairWith(props); //You can only repair a sail if there is one present

        public override bool IsPresent => SailLength != 0;

        public int? CachedSailLength { get; set; }

        public override void UpdateDecay(double daysPassed)
        {
            //Fix sail durability when more sails have been added
            var sailLength = SailLength;
            if (CachedSailLength != null && sailLength != CachedSailLength)
            {
                if (CachedSailLength == 0 && sailLength > 0)
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
            if (AreSailsRolledUp) return;
            base.UpdateDecay(daysPassed);
        }

        public override ItemStack[] ModifyDroppedItemStacks(ItemStack[] itemStacks, IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier, bool isBlockDestroyed)
        {
            if (!IsPresent) return itemStacks;
            var sailItems = new List<ItemStack>();

            if (Durability > WearAndTearServerConfig.Instance.DurabilityLeeway) Durability = 1;
            var item = SailAssetLocation;
            var sailCount = BladeCount;
            var sailLength = SailLength;

            var sailItemCount = sailLength * sailCount * Durability * dropQuantityMultiplier;

            while (sailItemCount >= 1)
            {
                var stackSize = (int)Math.Min(sailItemCount, sailCount);
                sailItemCount -= stackSize;

                sailItems.Add(new ItemStack(world.GetItem(new AssetLocation(item)), stackSize));
            }

            if (sailItemCount > 0)
            {
                sailItemCount *= sailCount;
            }

            while (sailItemCount >= 1)
            {
                var stackSize = (int)Math.Min(sailItemCount, 32);
                sailItemCount -= stackSize;

                sailItems.Add(new ItemStack(world.GetItem(new AssetLocation("flaxtwine")), stackSize));
            }

            var block = Block.GetPlacedByItem(world.Api);

            //Remove sail durability from tree as it is no longer relevant
            var blockCode = block.FirstCodePartAsSpan().ToString(); //Sadly we can't use a ReadonlySpan here (due to lambda scope)
            var normalItem = Array.Find(itemStacks, item => item.Block != null && item.Block.FirstCodePartAsSpan().SequenceEqual(blockCode));
            normalItem?.Attributes.GetTreeAttribute(Constants.DurabilityTreeName).RemoveAttribute(Props.Code);

            return base.ModifyDroppedItemStacks(itemStacks.Concat(sailItems).ToArray(), world, pos, byPlayer, dropQuantityMultiplier, isBlockDestroyed);
        }

        public void DropSails()
        {
            if (!IsPresent) return;

            var items = ModifyDroppedItemStacks(Array.Empty<ItemStack>(), Api.World, Pos, null, 1, false);

            foreach (var item in items) Api.World.SpawnItemEntity(item, Pos.ToVec3d().Add(0.5, 0.5, 0.5), null);

            SailLength = 0;
            Durability = 1;
            Blockentity.MarkDirty(true);
        }

        public override void GetWearAndTearInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            if (!IsPresent) return;

            dsc.AppendLine($"{Props.GetDurabilityStringForPlayer(Api, forPlayer, Durability)} {(AreSailsRolledUp ? " (Rolled Up)" : "")}");
        }

        public override float? EfficiencyModifier
        {
            get
            {
                if (AreSailsRolledUp) return 0;
                return Props.DurabilityEfficiencyRatio == 0 ? null : 1 - (1f - Durability) * Props.DurabilityEfficiencyRatio;
            }
        }

        public override float DoMaintenanceFor(float maintenanceStrength, EntityPlayer player) //TODO see about simplifying this
        {
            var realMaintenanceStrength = maintenanceStrength * (4f / (SailLength * BladeCount));

            var realAllowedMaintenanceStrength = realMaintenanceStrength;
            if (HasMaintenanceLimit)
            {
                var limit = Props.MaintenanceLimit.Value;

                if (WearAndTearModSystem.XlibEnabled) limit = SkillsAndAbilities.ApplyLimitBreakerBonus(player.Api, player.Player, limit);

                realAllowedMaintenanceStrength = GameMath.Clamp(realMaintenanceStrength, 0, limit - RepairedDurability);
            }

            Durability += realAllowedMaintenanceStrength;

            float realLeftOverMaintenanceStrength = realMaintenanceStrength - realAllowedMaintenanceStrength + Math.Max(Durability - 1, 0);

            if (HasMaintenanceLimit) RepairedDurability += realAllowedMaintenanceStrength - Math.Max(Durability - 1, 0);

            Durability = GameMath.Clamp(Durability, WearAndTearServerConfig.Instance.MinDurability, 1);

            return Math.Max(realLeftOverMaintenanceStrength * SailLength * BladeCount / 4f, 0);
        }

        public virtual int GetRandomFactor()
        {
            try
            {
                return Math.Abs(Pos.X ^ (Pos.Y << 10) ^ (Pos.Z << 20));
            }
            catch
            {
                return 0; //can this even happen?
            }
        }

        public virtual void UpdateShape(BEBehaviorMPBase beh)
        {
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

                if (Block.Code.Domain == "game")
                {
                    if (Durability < 0.05) durabilityVariant = 0;
                    else if (Durability < 0.50) durabilityVariant = 50;
                    else if (Durability < 0.75) durabilityVariant = 75;
                }
                else if (Durability < .75)
                {
                    durabilityVariant = (int)(Durability * 100);
                }

                if (durabilityVariant != null)
                {
                    newLocation = beh.Shape.Base.Clone();
                    newLocation.Path = $"{newLocation.Path}-{GetRandomFactor()}-{durabilityVariant}";
                }
            }

            if (newLocation == null) return;

            beh.Shape = new CompositeShape
            {
                Base = newLocation,
                rotateY = Block.Shape.rotateY,
            };
        }
    }
}