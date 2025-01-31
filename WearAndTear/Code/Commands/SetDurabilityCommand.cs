using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using WearAndTear.Code.Interfaces;

namespace WearAndTear.Code.Commands
{
    public static class SetDurabilityCommand
    {
        public static void Register(ICoreServerAPI api) => api.ChatCommands.Create("SetWearAndTearDurability")
                .WithDescription("Used to change the durability of a WearAndTear objevt")
                .RequiresPrivilege(Privilege.controlserver)
                .RequiresPlayer()
                .WithArgs(
                    new IntArgParser("PartIndex", 1, int.MaxValue, 1, true),
                    new FloatArgParser("Durability", 0, 1, true)
                ).HandleWith(Handle);

        private static TextCommandResult Handle(TextCommandCallingArgs args)
        {
            var api = args.Caller.Entity.Api;
            var blockSelect = args.Caller.Player.CurrentBlockSelection;
            var entity = blockSelect != null ? api.World.BlockAccessor.GetBlockEntity(blockSelect.Position) : null;
            var wearAndTear = entity?.GetBehavior<IWearAndTear>();
            if (wearAndTear == null) return TextCommandResult.Error("Not aiming at a WearAndTear affected block");
            var partIndex = (int)args.Parsers[0].GetValue();
            if (partIndex > wearAndTear.Parts.Count) return TextCommandResult.Error($"{partIndex} is not a valid part index, only {wearAndTear.Parts.Count} parts are present");

            var part = wearAndTear.Parts[partIndex - 1];
            part.Durability = (float)args.Parsers[1].GetValue();
            entity.MarkDirty();
            return TextCommandResult.Success($"Set durability of '{part.Props.Name}' to {part.Durability}");
        }
    }
}
