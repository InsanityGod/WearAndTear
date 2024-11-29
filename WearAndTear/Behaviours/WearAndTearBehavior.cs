using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Config.Props;
using WearAndTear.Interfaces;

namespace WearAndTear.Behaviours
{
    public class WearAndTearBehavior : BlockEntityBehavior, IWearAndTear
    {
        public WearAndTearBehavior(BlockEntity blockentity) : base(blockentity)
        {
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            Parts = Blockentity.Behaviors.OfType<IWearAndTearPart>().ToList();

            LastDecayUpdate ??= Api.World.Calendar.TotalDays;

            if (api.Side != EnumAppSide.Server) return;
            Blockentity.RegisterGameTickListener(_ => UpdateDecay(Api.World.Calendar.TotalDays - LastDecayUpdate.Value), WearAndTearModSystem.Config.DurabilityUpdateFrequencyInMs);
        }

        public double? LastDecayUpdate { get; set; }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            LastDecayUpdate = tree.TryGetDouble("LastDecayUpdate") ?? LastDecayUpdate;
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            if (LastDecayUpdate.HasValue) tree.SetDouble("LastDecayUpdate", LastDecayUpdate.Value);
        }

        public List<IWearAndTearPart> Parts { get; private set; }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);
            dsc.AppendLine();
            dsc.AppendLine("<strong>WearAndTear</strong>");
            foreach (var part in Parts) part.GetWearAndTearInfo(forPlayer, dsc);

            dsc.AppendLine();
        }

        public void UpdateDecay(double daysPassed, bool updateLastUpdatedAt = true)
        {
            if (updateLastUpdatedAt) LastDecayUpdate = Api.World.Calendar.TotalDays;

            foreach (var part in Parts)
                part.UpdateDecay(daysPassed);

            Blockentity.MarkDirty();
        }

        public virtual bool TryRepair(WearAndTearRepairItemProps props, ItemSlot slot, EntityAgent byEntity)
        {
            var powerDevice = Blockentity.GetBehavior<IMechanicalPowerDevice>();
            if (powerDevice != null && powerDevice.Network.Speed > 0.001)
            {
                if (Api is ICoreClientAPI clientApi) clientApi?.TriggerIngameError(this, "wearandtear:repairfailed-moving", Lang.Get("wearandtear:repairfailed-moving"));
                return false;
            }

            var repairStrength = props.RepairStrength;
            var anyPartBrokenEnough = false;
            foreach (var part in Parts)
            {
                if (!part.IsRepairableWith(props) || part.Durability > WearAndTearModSystem.Config.MinRepairDurability) continue;

                anyPartBrokenEnough = true;

                repairStrength = part.RepairFor(repairStrength);
                if (repairStrength <= 0) break;
            }

            if (!anyPartBrokenEnough && Api is ICoreClientAPI clientApi2)
            {
                clientApi2.TriggerIngameError(this, "repairfailed-notbroken", Lang.Get("wearandtear:repairfailed-notbroken"));
            }

            if (repairStrength < props.RepairStrength)
            {
                slot.TakeOut(1);
                slot.MarkDirty();
                return true;
            }

            return false;
        }
    }
}