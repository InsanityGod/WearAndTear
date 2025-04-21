using Cairo;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using WearAndTear.Code.Extensions;
using WearAndTear.Code.Interfaces;
using WearAndTear.Config.Props;

namespace WearAndTear.Code.HarmonyPatches
{
    [HarmonyPatch]
    public static class AppendHandbookInfo
    {
        [HarmonyPatch(typeof(CollectibleBehaviorHandbookTextAndExtraInfo), nameof(CollectibleBehaviorHandbookTextAndExtraInfo.GetHandbookInfo))]
        [HarmonyPostfix]
        public static void Append(CollectibleBehaviorHandbookTextAndExtraInfo __instance, ItemSlot inSlot, ICoreClientAPI capi, ItemStack[] allStacks, ActionConsumable<string> openDetailPageFor, ref RichTextComponentBase[] __result)
        {
            var block = inSlot.Itemstack?.Block?.GetActualPlacementBlock(capi);

            if (block == null || block.BlockEntityBehaviors == null) return;
            var wearandtear = Array.Find(block.BlockEntityBehaviors, beh => beh.Name == "WearAndTear");
            if (wearandtear == null) return;

            var components = new List<RichTextComponentBase>();
            AddHeading(components, capi, "wearandtear:handbook-heading", true);
            List<AssetLocation> ScrapCodes = new();
            bool hasParts = false;

            var parts = block.BlockEntityBehaviors
                .Select(beh =>
                {
                    var behType = capi.ClassRegistry.GetBlockEntityBehaviorClass(beh.Name);
                    if (typeof(IWearAndTearPart).IsAssignableFrom(behType))
                    {
                        var props = beh.properties.AsObject<WearAndTearPartProps>();
                        return (behType, beh, props);
                    }
                    return (behType, beh, null);
                }).Where(beh => beh.props != null)
                .ToList();

            foreach ((var behType, var beh, var props) in parts)
            {
                if (hasParts)
                {
                    components.Add(new ClearFloatTextComponent(capi, 8f));
                }
                else hasParts = true;
                var header = props.GetDisplayName();
                if (typeof(IWearAndTearOptionalPart).IsAssignableFrom(behType)) header += $" ({Lang.Get("wearandtear:optional")})";
                AddSubHeading(components, capi, openDetailPageFor, header);

                if (props.Decay != null && props.Decay.Length > 0)
                {
                    components.Add(new RichTextComponent(capi, Lang.Get("wearandtear:handbook-lifespan", props.AvgLifeSpanInYears) + "\n", new CairoFont
                    {
                        Color = (double[])GuiStyle.WarningTextColor.Clone(),
                        Fontname = GuiStyle.StandardFontName,
                        UnscaledFontsize = GuiStyle.SmallFontSize
                    }));

                    components.Add(new RichTextComponent(capi, Lang.Get("wearandtear:handbook-decay", string.Join(", ", props.Decay.Select(decay => Lang.Get($"wearandtear:decay-{decay.Type}")))) + "\n", new CairoFont
                    {
                        Color = (double[])GuiStyle.WarningTextColor.Clone(),
                        Fontname = GuiStyle.StandardFontName,
                        UnscaledFontsize = GuiStyle.SmallFontSize
                    }));
                }
                else
                {
                    var minDurabilityUsage = beh.properties[nameof(WearAndTearDurabilityPartProps.MinDurabilityUsage)].AsFloat();
                    var maxDurabilityUsage = beh.properties[nameof(WearAndTearDurabilityPartProps.MaxDurabilityUsage)].AsFloat();
                    if (minDurabilityUsage != 0 && maxDurabilityUsage != 0)
                    {
                        var str = minDurabilityUsage == maxDurabilityUsage ?
                            Lang.Get("wearandtear:handbook-usage-limit", minDurabilityUsage.ToPercentageString()) :
                            Lang.Get("wearandtear:handbook-usage-limit-random", minDurabilityUsage.ToPercentageString(), maxDurabilityUsage.ToPercentageString());
                        components.Add(new RichTextComponent(capi, str + "\n", new CairoFont
                        {
                            Color = (double[])GuiStyle.WarningTextColor.Clone(),
                            Fontname = GuiStyle.StandardFontName,
                            UnscaledFontsize = GuiStyle.SmallFontSize
                        }));
                    }
                    else components.Add(new RichTextComponent(capi, Lang.Get("wearandtear:handbook-decay", Lang.Get($"wearandtear:decay-usage")) + "\n", new CairoFont
                    {
                        Color = (double[])GuiStyle.WarningTextColor.Clone(),
                        Fontname = GuiStyle.StandardFontName,
                        UnscaledFontsize = GuiStyle.SmallFontSize
                    }));
                }

                if (typeof(IWearAndTearProtectivePart).IsAssignableFrom(behType))
                {
                    var protectiveProps = beh.properties.AsObject<WearAndTearProtectivePartProps>();
                    if (protectiveProps != null)
                    {
                        var protectiveStrings = protectiveProps.EffectiveFor
                            .GroupBy(effect => (1 - effect.DecayMultiplier).ToPercentageString())
                            .SelectMany(effectGroup => effectGroup.Select(
                                effect =>
                                {
                                    var applicableParts = parts.Where(part => effect.IsEffectiveFor(part.props))
                                        .Select(part => part.props.GetDisplayName())
                                        .ToList();
                                    if (applicableParts.Count == 0) return null;
                                    return Lang.Get("wearandtear:hanbook-protection", effectGroup.Key, string.Join(", ", applicableParts));
                                })
                            ).Where(str => str != null);

                        foreach (var str in protectiveStrings)
                        {
                            components.Add(new RichTextComponent(capi, str + "\n", new CairoFont
                            {
                                Color = (double[])GuiStyle.SuccessTextColor.Clone(),
                                Fontname = GuiStyle.StandardFontName,
                                UnscaledFontsize = GuiStyle.SmallFontSize
                            }));
                        }
                    }
                }

                //Protective properties
                if (props.IsCritical)
                {
                    components.Add(new RichTextComponent(capi, Lang.Get("wearandtear:handbook-critical") + "\n", new CairoFont
                    {
                        Color = (double[])GuiStyle.ErrorTextColor.Clone(),
                        Fontname = GuiStyle.StandardFontName,
                        UnscaledFontsize = GuiStyle.SmallFontSize
                    }));
                }

                if (props.ScrapCode != null && !ScrapCodes.Contains(props.ScrapCode)) ScrapCodes.Add(props.ScrapCode);
            }

            //TODO come up with a cleaner way to hide these scrap items (maybe even do this during autoregistry instead)
            if (block is not BlockIngotMold && block is not BlockToolMold && ScrapCodes.Any())
            {
                var items = new List<ItemStack>();
                foreach (var scrapCode in ScrapCodes)
                {
                    var scrapItem = capi.World.GetItem(scrapCode);
                    if (scrapItem == null) continue;

                    items.Add(new(scrapItem));
                }
                if (items.Any())
                {
                    components.Add(new ClearFloatTextComponent(capi, 12f));
                    components.Add(new RichTextComponent(capi, Lang.Get("wearandtear:handbook-scrap") + "\n", new CairoFont
                    {
                        Color = (double[])GuiStyle.DialogDefaultTextColor.Clone(),
                        Fontname = GuiStyle.StandardFontName,
                        FontWeight = FontWeight.Bold,
                        UnscaledFontsize = GuiStyle.SmallFontSize
                    }));

                    while (items.Count > 0)
                    {
                        var item = items.PopOne();
                        components.Add(new SlideshowItemstackTextComponent(capi, item, items, 40, EnumFloat.Inline, cs => openDetailPageFor(GuiHandbookItemStackPage.PageCodeForStack(cs))));
                    }
                }
            }
            //TODO maybe display some of the xskills bonuses
            __result = __result.AddRangeToArray(components.ToArray());
        }

        public static void AddHeading(List<RichTextComponentBase> components, ICoreClientAPI capi, string heading, bool haveText)
        {
            if (haveText)
            {
                components.Add(new ClearFloatTextComponent(capi, 14f));
            }

            components.Add(new RichTextComponent(capi, Lang.Get(heading) + "\n", CairoFont.WhiteSmallText().WithWeight(FontWeight.Bold)));
        }

        public static void AddSubHeading(List<RichTextComponentBase> components, ICoreClientAPI capi, ActionConsumable<string> openDetailPageFor, string subheading, string detailpage = null)
        {
            if (detailpage == null)
            {
                components.Add(new RichTextComponent(capi, $"• {subheading}\n", CairoFont.WhiteSmallText())
                {
                    PaddingLeft = 2.0
                });
                return;
            }
            components.Add(new RichTextComponent(capi, "• ", CairoFont.WhiteSmallText())
            {
                PaddingLeft = 2.0
            });
            components.Add(new LinkTextComponent(capi, $"{subheading}\n", CairoFont.WhiteSmallText(), delegate (LinkTextComponent cs)
            {
                openDetailPageFor(detailpage);
            }));
        }
    }
}