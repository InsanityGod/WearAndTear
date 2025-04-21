using Vintagestory.API.Common;

namespace WearAndTear.Code.Extensions
{
    public static class CollectibleExtensions
    {
        public static CollectibleObject GetActualPlacementItem(this CollectibleObject collectible, ICoreAPI api)
        {
            if (collectible.Attributes != null)
            {
                var redirect = collectible.Attributes["WearAndTear_PlacementItemRedirect"].AsString();
                if (redirect != null)
                {
                    CollectibleObject newCollectible = api.World.GetBlock(redirect);
                    newCollectible ??= api.World.GetItem(redirect);
                    if (newCollectible == null) api.Logger.Error($"[WearAndTear] Invalid placement item redirect {collectible.Code} -> {redirect}");
                    else collectible = newCollectible;
                }
            }

            return collectible;
        }

        public static Block GetActualPlacementBlock(this CollectibleObject collectible, ICoreAPI api)
        {
            if (collectible.Attributes != null)
            {
                var redirect = collectible.Attributes["WearAndTear_ActualBlockRedirect"].AsString();
                if (redirect != null)
                {
                    Block block = api.World.GetBlock(redirect);
                    if (block == null) api.Logger.Error($"[WearAndTear] Invalid actual block redirect {collectible.Code} -> {redirect}");
                    else collectible = block;
                }
            }

            return collectible as Block;
        }
    }
}