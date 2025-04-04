using InsanityLib.Attributes.Auto.Command;
using InsanityLib.Enums.Auto.Commands;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using WearAndTear.Code.Behaviours;
using WearAndTear.Code.Extensions;
using WearAndTear.Code.Interfaces;

namespace WearAndTear.Code.Commands
{
    public static class DurabilityCommands
    {
        //TODO Test
        [AutoCommand(Path = "wearandtear", RequiredPrivelege = "controlserver")]
        public static TextCommandResult SetDurability(
            [CommandParameter(Source = EParamSource.CallerTarget)] [Required(ErrorMessage = Constants.TARGET_NOT_WEARANDTEAR_AFFECTED)] WearAndTearBehavior wearAndTear,
            [CommandParameter] string PartIdentifier,
            [CommandParameter] [Range(0f, 1f)] float Durability)
        {
            IWearAndTearPart part = null;

            if(int.TryParse(PartIdentifier, out int partIndex))
            {
                if(partIndex < 1 || partIndex > wearAndTear.Parts.Count) return TextCommandResult.Error($"Could not find part '{partIndex}', expected a number between 1 and {wearAndTear.Parts.Count}");
                partIndex--;

                part = wearAndTear.Parts[partIndex];
            }
            else
            {
                var partCodeArray = PartIdentifier.Split(":");
                
                part = wearAndTear.Parts.Find(
                    partCodeArray.Length > 1 ?
                    p => p.Props.Code.Domain == partCodeArray[0] && p.Props.Code.Path == partCodeArray[1] :
                    p => p.Props.Code.Path == partCodeArray[0]
                );
            }

            if (part == null) return TextCommandResult.Error($"Could not find part '{PartIdentifier}', expected one of the following: {string.Join(", ", wearAndTear.Parts.Select(p => p.Props.Code))}");

            part.Durability = Durability;
            wearAndTear.Blockentity.MarkDirty();
            return TextCommandResult.Success($"Set durability of '{part.Props.GetDisplayName()}' to {part.Durability.ToPercentageString()}");
        }
    }
}