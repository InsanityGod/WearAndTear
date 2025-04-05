using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;
using WearAndTear.Config.Props;

namespace WearAndTear.Code.Rendering
{
    public class RubbleTextureSource : ITexPositionSource
    {
        public TextureAtlasPosition this[string textureCode]
		{
			get
			{
                var source = Sources[textureCode];
                if(source == null) return atlasMgr.UnknownTexturePos;

                return source[textureCode] ?? source["up"] ?? source["all"] ?? atlasMgr.UnknownTexturePos;
			}
		}

        public string[] GetSelectiveElements() => Sources.Keys.Select(key => key + "*").ToArray();

        public Size2i AtlasSize => atlasSize;

        public RubbleTextureSource(ClientMain game, ITesselatorAPI tesselator , Block block)
		{
            atlasSize = game.BlockAtlasManager.Size;
			atlasMgr = game.BlockAtlasManager;
			try
			{
                foreach (var behavior in block.BlockEntityBehaviors)
                {
                    if(behavior.properties == null) continue;
                    var scrapCode = behavior.properties[nameof(WearAndTearPartProps.ScrapCode)].AsString();
                    if(string.IsNullOrEmpty(scrapCode)) continue;

                    var scrapItem = game.Api.World.GetItem(scrapCode);
                    if(scrapItem == null) continue;

                    try
                    {
                        var metal = scrapItem.Variant["metal"];
                        if(metal != null)
                        {
                            
                            var metalBlock = game.Api.World.GetBlock($"game:metalsheet-{metal}-down");
                            if(metalBlock != null) Sources["metal"] = tesselator.GetTextureSource(metalBlock, returnNullWhenMissing: true);
                        }

                        var wood = scrapItem.Variant["wood"];
                        if(wood != null)
                        {
                            
                            var woodBlock = game.Api.World.GetBlock($"game:planks-{wood}-ud");
                            woodBlock ??= game.Api.World.GetBlock($"wildcrafttree:planks-{wood}-ud");
                            if(woodBlock != null) Sources["wood"] = tesselator.GetTextureSource(woodBlock, returnNullWhenMissing: true);
                        }

                        var rock = scrapItem.Variant["rock"];
                        if(rock != null)
                        {
                            var rockBlock = game.Api.World.GetBlock($"game:rock-{rock}");
                            if(rockBlock != null) Sources["rock"] = tesselator.GetTextureSource(rockBlock, returnNullWhenMissing: true);
                        }
                    }
                    catch
                    {
                        // Ignore
                    }
                }
            }
			catch (Exception)
			{
				game.Logger.Error("[WearAndTear] Unable to initialize RubbleTextureSource for block {0}. Will crash now.", block?.Code);
				throw;
			}
		}

        private readonly BlockTextureAtlasManager atlasMgr;

		private readonly Size2i atlasSize;

        private Dictionary<string, ITexPositionSource> Sources { get; set; } = new();

    }

}
