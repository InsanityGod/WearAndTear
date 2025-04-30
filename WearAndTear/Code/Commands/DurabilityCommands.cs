using InsanityLib.Attributes.Auto.Command;
using InsanityLib.Enums.Auto.Commands;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vintagestory.API.Common;
using WearAndTear.Code.Behaviours;
using WearAndTear.Code.Extensions;
using WearAndTear.Code.Interfaces;

namespace WearAndTear.Code.Commands
{
    public static class DurabilityCommands
    {
        /// <summary>
        /// Sets the durability of a part on the targeted WearAndTear affected block
        /// </summary>
        /// <param name="controller"/>
        /// <param name="PartIdentifier">The identifier of the part, either as index or as code</param>
        /// <param name="Durability">The durability to set the part to, as a number between 0 and 1</param>
        /// <example>/wearandtear setdurability frame 0.5</example>
        /// <example>/wearandtear setdurability 1 0.5</example>
        [AutoCommand(Path = "wearandtear", RequiredPrivelege = "controlserver")]
        public static TextCommandResult SetDurability(
            [CommandParameter(Source = EParamSource.CallerTarget)][Required(ErrorMessage = Constants.TARGET_NOT_WEARANDTEAR_AFFECTED)] PartController controller,
            [CommandParameter] string PartIdentifier,
            [CommandParameter][Range(0f, 1f)] float Durability)
        {
            Part part = null;

            if (int.TryParse(PartIdentifier, out int partIndex))
            {
                if (partIndex < 1 || partIndex > controller.Parts.Count) return TextCommandResult.Error($"Could not find part '{partIndex}', expected a number between 1 and {controller.Parts.Count}");
                partIndex--;

                part = controller.Parts[partIndex];
            }
            else
            {
                var partCodeArray = PartIdentifier.Split(":");

                part = controller.Parts.Find(
                    partCodeArray.Length > 1 ?
                    p => p.Props.Code.Domain == partCodeArray[0] && p.Props.Code.Path == partCodeArray[1] :
                    p => p.Props.Code.Path == partCodeArray[0]
                );
            }

            if (part == null) return TextCommandResult.Error($"Could not find part '{PartIdentifier}', expected one of the following: {string.Join(", ", controller.Parts.Select(p => p.Props.Code))}");

            part.Durability = Durability;
            controller.Blockentity.MarkDirty();
            return TextCommandResult.Success($"Set durability of '{part.Props.GetDisplayName()}' to {part.Durability.ToPercentageString()}");
        }

        /// <summary>
        /// Clears all WearAndTear attributes from the itemstack
        /// </summary>
        /// <example>/wearandtear removeattributes</example>
        [AutoCommand(Path = "wearandtear", RequiredPrivelege = "controlserver")]
        public static TextCommandResult RemoveAttributes([CommandParameter(Source = EParamSource.Caller)] ItemSlot itemslot)
        {
            if (itemslot.Empty) return TextCommandResult.Error("Not holding an itemstack");
            
            if(itemslot.Itemstack.Attributes != null && itemslot.Itemstack.Attributes.HasAttribute(Constants.DurabilityTreeName))
            {
                itemslot.Itemstack.Attributes.RemoveAttribute(Constants.DurabilityTreeName);
                itemslot.MarkDirty();
                return TextCommandResult.Success("Removed WearAndTear attributes from item");
            }
            return TextCommandResult.Error("No WearAndTear attributes on item");
        }
    }
}