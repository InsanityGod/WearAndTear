using System;
using System.Collections.Generic;
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
                var source = textureCode switch
                {
                    "metal" => MetalSource,
                    "wood" => WoodSource,
                    _ => null
                };
                TextureAtlasPosition pos = null;
                if(source != null)
                {
                    pos = source["up"];
                }

                return pos ?? atlasMgr.UnknownTexturePosition;
			}
		}

        public string[] GetSelectiveElements()
        {
            var selective = new List<string>();

            if (MetalSource != null) selective.Add("metal*");
            if (WoodSource != null) selective.Add("wood*");

            return selective.ToArray();
        }

        public Size2i AtlasSize => atlasSize;

        public RubbleTextureSource(ClientMain game,ITesselatorAPI tesselator , Block block)
		{
            atlasSize = game.BlockAtlasManager.Size;
			atlasMgr = game.BlockAtlasManager;
			try
			{
                textureCodeToIdMapping = new MiniDictionary(3);
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
                            if(metalBlock != null) MetalSource ??= tesselator.GetTextureSource(metalBlock);
                        }

                        var wood = scrapItem.Variant["wood"];
                        if(wood != null)
                        {
                            
                            var woodBlock = game.Api.World.GetBlock($"game:planks-{wood}-ud");
                            if(woodBlock != null) WoodSource ??= tesselator.GetTextureSource(woodBlock);
                        }
                    }
                    catch
                    {
                        // Ignore
                    }
                }
                
                textureCodeToIdMapping["all"] = game.BlockAtlasManager.UnknownTexturePos.atlasTextureId;
            }
			catch (Exception)
			{
				game.Logger.Error("[WearAndTear] Unable to initialize RubbleTextureSource for block {0}. Will crash now.", block?.Code);
				throw;
			}
		}

		private readonly Size2i atlasSize;

		private readonly MiniDictionary textureCodeToIdMapping;

        private readonly ITexPositionSource WoodSource;

        private readonly ITexPositionSource MetalSource;

        private readonly BlockTextureAtlasManager atlasMgr;
    }

}
