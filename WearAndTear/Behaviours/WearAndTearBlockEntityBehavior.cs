using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Config;

namespace WearAndTear.Behaviours
{
    public class WearAndTearBlockEntityBehavior : BlockEntityBehavior
    {
        public WearAndTearBlockEntityBehavior(BlockEntity blockentity) : base(blockentity)
        {
        }

        /// <summary>
        /// Average life spanAsuming an avgWindspeed of 0.5 and avgRainfall of 0.5
        /// </summary>
        public float AvgLifeSpanInYears { get; private set; } = 1;

        public EWearAndTearType WearAndTearType { get; private set; }

        public string RepairType { get; private set; }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            Durability = tree.GetFloat("WearAndTear_Durability", Durability);
            LastUpdatedAt = tree.TryGetDouble("WearAndTear_LastUpdatedAt");
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetFloat("WearAndTear_Durability", Durability);
            tree.SetDouble("WearAndTear_LastUpdatedAt", Api.World.Calendar.TotalDays);
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            AvgLifeSpanInYears = properties["avgLifeSpanInYears"].AsFloat(AvgLifeSpanInYears);
            WearAndTearType = properties["wearAndTearType"].AsObject<EWearAndTearType>();
            RepairType = properties["repairType"].AsString();

            if (Api.Side == EnumAppSide.Client) return;

            LastUpdatedAt ??= Api.World.Calendar.TotalDays;
            Blockentity.RegisterGameTickListener(UpdateWearAndTear, WearAndTearModSystem.Config.DurabilityUpdateFrequencyInMs);
        }

        /// <summary>
        /// Can be changed to false if the block can't currently be affected by WearAndTear
        /// (For instance when a windmillrotor has no sails)=
        /// </summary>
        public bool Enabled { get; set; } = true;

        public float Durability { get; set; } = 1;

        public double? LastUpdatedAt { get; set; }

        public void UpdateWearAndTear(float secondsPassed)
        {
            UpdateDurability();
            LastUpdatedAt = Api.World.Calendar.TotalDays;
        }

        public virtual void UpdateDurability()
        {
            if (!Enabled || WearAndTearType == EWearAndTearType.None) return;

            var daysPassed = Api.World.Calendar.TotalDays - LastUpdatedAt.Value;

            var avgWindSpeed = .5;

            var avgClimate = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.WorldGenValues);
            var avgWetness = avgClimate.Rainfall;

            if (daysPassed < 1)
            {
                //HACK: haven't been able to figure out how to get the average windspeed over the passed period
                //(this is to prevent massive damage when returning from a long trip while it's very windy)

                avgWindSpeed = Api.World.BlockAccessor.GetWindSpeedAt(Pos).Length(); //TODO test;

                var climate = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.NowValues);
                avgWetness = climate.Rainfall;
            }

            double degradationFactors = 0;
            int factorCount = 0;
            if ((WearAndTearType & EWearAndTearType.Wind) == EWearAndTearType.Wind)
            {
                degradationFactors += (avgWindSpeed / 0.5);
                factorCount++;
            }

            if ((WearAndTearType & EWearAndTearType.Rain) == EWearAndTearType.Rain)
            {
                degradationFactors += (avgWetness / 0.5);
                factorCount++;
            }

            // Calculate degradation factor (wind-adjusted daily wear rate)
            double degradationRate = (degradationFactors / factorCount) / (AvgLifeSpanInYears * Api.World.Calendar.DaysPerYear);

            double degradation = degradationRate * daysPassed;

            Durability = (float)Math.Max(WearAndTearModSystem.Config.MinDurability, Durability - degradation);
            Blockentity.MarkDirty(false, null);
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);
            dsc.AppendLine(Lang.Get("Durability: {0}%", (int)(Durability * 100)));
        }

        public virtual float TorqueFactorModifier => Durability;

        public virtual void UpdateShape(BEBehaviorMPBase beh, string typeVariant)
        {
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

        /// <summary>
        /// Wether this item can be repaired with given tool
        /// </summary>
        /// <param name="repairTool">Tool to use for repairing</param>
        /// <returns>True if it can be repaired with given tool, false if not</returns>
        public virtual bool IsRepairableWith(WearAndTearRepairToolCollectibleBehavior repairTool) => repairTool.RepairType == RepairType;

        public virtual bool TryRepair(float repairStrength)
        {
            if (repairStrength <= 0) return false;

            if (Durability > WearAndTearModSystem.Config.DurabilityLeeway)
            {
                if (Api.Side == EnumAppSide.Client) (Api as ICoreClientAPI)?.TriggerIngameError(this, "wearandtear:repairfailed-notbroken", Lang.Get("wearandtear:repairfailed-notbroken"));
                return false;
            }

            var powerDevice = Blockentity.GetBehavior<IMechanicalPowerDevice>();
            if(powerDevice != null && powerDevice.Network.Speed > 0.001)
            {
                if (Api.Side == EnumAppSide.Client) (Api as ICoreClientAPI)?.TriggerIngameError(this, "wearandtear:repairfailed-moving", Lang.Get("wearandtear:repairfailed-moving"));
                return false;
            }

            if (Api.Side == EnumAppSide.Server)
            {
                Durability = Math.Min(Durability + repairStrength, 1);
                Blockentity.MarkDirty(false, null);
            }

            return true;
        }
    }
}