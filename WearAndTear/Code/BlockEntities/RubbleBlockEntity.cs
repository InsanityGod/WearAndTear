using HarmonyLib;
using InsanityLib.Util;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;
using Vintagestory.Client.NoObf;
using WearAndTear.Code.Behaviours.Rubble;
using WearAndTear.Code.Extensions;
using WearAndTear.Code.Rendering;
using WearAndTear.Code.XLib;
using WearAndTear.Config.Props.rubble;

namespace WearAndTear.Code.BlockEntities
{
    public class RubbleBlockEntity : BlockEntity
    {
        public ITreeAttribute Contents { get; set; } = new TreeAttribute();

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            Contents = tree.GetOrAddTreeAttribute("contents");

            base.FromTreeAttributes(tree, worldAccessForResolve);
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            tree["contents"] = Contents;

            base.ToTreeAttributes(tree);
        }

        public override void OnBlockPlaced(ItemStack byItemStack = null)
        {
            if (byItemStack != null) Contents.SetItemstack("0", byItemStack);

            base.OnBlockPlaced(byItemStack);
            MarkDirty(true); //This fixes annoying issue where stack is not yet ready when rendering on client
            Block.GetBehavior<RubbleBehavior>()?.DelayedOnBlockPlaced(Api.World, Pos); //This is to deal with rubbble trying to fall before the entity is created
        }

        public ItemStack[] GetDrops(IWorldAccessor world, IPlayer byPlayer, float dropQuantityMultiplier)
        {
            var items = new List<ItemStack>();

            foreach (var content in Contents.Values.OfType<ItemstackAttribute>())
            {
                //normal drops
                var normalDrops = content.value.Attributes.GetTreeAttribute("rubble-normal-drops");
                foreach (var drop in normalDrops.Values.OfType<ItemstackAttribute>())
                {
                    var item = drop.value.Clone();
                    if (WearAndTearModSystem.XlibEnabled && (item.Collectible != null || item.ResolveBlockOrItem(world)))
                    {
                        item.StackSize = SkillsAndAbilities.ApplyScrapperBonus(world.Api, byPlayer, item.StackSize);
                    }

                    items.Add(item);
                }
            }

            return items.ToArray();
        }

        public const string CacheKey = "WearAndTearRubbleMeshes";
        public const string defaultShape = "wearandtear:shapes/rubble.json";

        private MeshData mesh;

        public ItemStack PrimaryContent => Contents.GetItemstack("0");

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            var primaryContent = PrimaryContent;
            if (primaryContent == null || (primaryContent.Collectible == null && !primaryContent.ResolveBlockOrItem(Api.World))) return false;

            var block = PrimaryContent.Collectible.GetPlacedBlock(Api);

            if (block?.Attributes == null) return false;
            if (mesh == null)
            {
                var loc = block.Attributes[WearAndTearRubbleProps.Key][nameof(WearAndTearRubbleProps.Shape)].AsString();
                bool customShape = loc != null;
                
                Shape shape;
                if (customShape)
                {
                    var assetLocation = new AssetLocation(loc);
                    //TODO improve caching
                    shape = Shape.TryGet(Api, $"{assetLocation.Domain}:shapes/{assetLocation.Path}.json");
                }
                else shape = Shape.TryGet(Api, defaultShape);

                if (shape == null && customShape)
                {
                    Api.Logger.Error("[WearAndTear] Could not find custom rubble shape at '{0}' for block '{1}', using default shape", loc, block.Code);
                    shape = Shape.TryGet(Api, defaultShape);
                    customShape = false;
                }

                if (customShape)
                {
                    tessThreadTesselator.TesselateShape("rubble", shape, out mesh, tessThreadTesselator.GetTextureSource(block));
                }
                else
                {
                    var texSource = new RubbleTextureSource(Traverse.Create(tessThreadTesselator).Field<ClientMain>("game").Value, tessThreadTesselator, block);
                    tessThreadTesselator.TesselateShape("rubble", shape, out mesh, texSource, selectiveElements: texSource.GetSelectiveElements());
                }
            }
            if (mesh != null)
            {
                mesher.AddMeshData(mesh);
                return true;
            }
            return false;
        }

        public override void OnBlockUnloaded()
        {
            mesh?.Dispose();
            base.OnBlockUnloaded();
        }

        public bool DamageOnTouch()
        {
            //TODO we should probably look at all content at some point
            var content = PrimaryContent;
            if (content == null) return false;
            if (content.Collectible == null) content.ResolveBlockOrItem(Api.World);
            if (content.Collectible?.Attributes == null) return true;
            return content.Collectible.Attributes[WearAndTearRubbleProps.Key][nameof(WearAndTearRubbleProps.DamageOnTouch)].AsBool(true);
        }
    }
}