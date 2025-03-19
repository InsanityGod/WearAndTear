using Cairo;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
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
            if(inSlot.Itemstack?.Block == null || inSlot.Itemstack.Block.BlockEntityBehaviors == null) return;
            var wearandtear = Array.Find(inSlot.Itemstack.Block.BlockEntityBehaviors, beh => beh.Name == "WearAndTear");
            if(wearandtear == null) return;

            var components = new List<RichTextComponentBase>();
            AddHeading(components, capi, "wearandtear:handbook-heading", true);
            List<AssetLocation> ScrapCodes = new();
            bool hasParts = false;
            foreach (var beh in inSlot.Itemstack.Block.BlockEntityBehaviors)
            {
                var behType = capi.ClassRegistry.GetBlockEntityBehaviorClass(beh.Name);
                if(!typeof(IWearAndTearPart).IsAssignableFrom(behType)) continue;

                var props = beh.properties.AsObject<WearAndTearPartProps>();
                if (hasParts)
                {
                    components.Add(new ClearFloatTextComponent(capi, 8f));
                } else hasParts = true;
                var header = props.GetDisplayName();
                if(typeof(IWearAndTearOptionalPart).IsAssignableFrom(behType)) header += $" ({Lang.Get("wearandtear:optional")})";
                AddSubHeading(components, capi, openDetailPageFor, header); //TODO maybe props.DetailsPage for extra info per part?
                
                if(props.Decay != null && props.Decay.Length > 0)
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
                    components.Add(new RichTextComponent(capi, Lang.Get("wearandtear:handbook-decay", Lang.Get($"wearandtear:decay-usage")) + "\n", new CairoFont 
                    { 
                        Color = (double[])GuiStyle.WarningTextColor.Clone(), 
                        Fontname = GuiStyle.StandardFontName, 
                        UnscaledFontsize = GuiStyle.SmallFontSize 
                    }));
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

                if(props.ScrapCode != null && !ScrapCodes.Contains(props.ScrapCode)) ScrapCodes.Add(props.ScrapCode);
            }

            if (ScrapCodes.Any())
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
                    
                    while(items.Count > 0)
                    {
                        var item = items.PopOne();
                        components.Add(new SlideshowItemstackTextComponent(capi, item, items, 40, EnumFloat.Inline, cs => openDetailPageFor(GuiHandbookItemStackPage.PageCodeForStack(cs))));
                    }
                }
                
            }

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
			components.Add(new LinkTextComponent(capi, $"{subheading}\n", CairoFont.WhiteSmallText(), delegate(LinkTextComponent cs)
			{
				openDetailPageFor(detailpage);
			}));
		}

    }
}
