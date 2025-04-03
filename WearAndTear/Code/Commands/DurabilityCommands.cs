using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using WearAndTear.Code.Extensions;
using WearAndTear.Code.Interfaces;

namespace WearAndTear.Code.Commands
{
    public static class DurabilityCommands
    {
        public static void RegisterDurabilityCommands(this IChatCommand parentCommand) => parentCommand
            .BeginSubCommand("SetDurability")
                .WithDescription("Used to change the durability of a WearAndTear object")
                .RequiresPrivilege(Privilege.controlserver)
                .RequiresPlayer()
                .WithArgs(
                    new WordArgParser("PartCode", true, new string[] { "frame", "reinforcement", "wax" }),
                    new FloatArgParser("Durability", 0, 1, true)
                ).HandleWith(SetDurability)
            .EndSubCommand()
            .BeginSubCommand("SetDurabilityByIndex")
                .WithDescription("Used to change the durability of a WearAndTear object")
                .RequiresPrivilege(Privilege.controlserver)
                .RequiresPlayer()
                .WithArgs(
                    new IntArgParser("PartIndex", 1, int.MaxValue, 1, true),
                    new FloatArgParser("Durability", 0, 1, true)
                ).HandleWith(SetDurabilityByIndex)
            .EndSubCommand();

        private static TextCommandResult SetDurabilityByIndex(TextCommandCallingArgs args)
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
            return TextCommandResult.Success($"Set durability of '{part.Props.GetDisplayName()}' to {part.Durability.ToPercentageString()}");
        }

        private static TextCommandResult SetDurability(TextCommandCallingArgs args)
        {
            var api = args.Caller.Entity.Api;
            var blockSelect = args.Caller.Player.CurrentBlockSelection;
            var entity = blockSelect != null ? api.World.BlockAccessor.GetBlockEntity(blockSelect.Position) : null;
            var wearAndTear = entity?.GetBehavior<IWearAndTear>();
            if (wearAndTear == null) return TextCommandResult.Error("Not aiming at a WearAndTear affected block");
            var partCodeStr = (string)args.Parsers[0].GetValue();
            var partCodeStrParts = partCodeStr.Split(":");
            var partCode = partCodeStrParts.Length > 1 ?
                new AssetLocation(partCodeStrParts[0], partCodeStrParts[1]) :
                new AssetLocation(null, partCodeStrParts[0]);

            var part = wearAndTear.Parts.Find(part =>
            {
                if(partCode.HasDomain() && partCode.Domain != part.Props.Code.Domain) return false;
                return partCode.Path == part.Props.Code.Path;
            });
            if (part == null) return TextCommandResult.Error($"{partCode} was not found in parts ({string.Join(", ", wearAndTear.Parts.Select(p => p.Props.Code))})");

            part.Durability = (float)args.Parsers[1].GetValue();
            entity.MarkDirty();
            return TextCommandResult.Success($"Set durability of '{part.Props.GetDisplayName()}' to {part.Durability.ToPercentageString()}");
        }
    }
}