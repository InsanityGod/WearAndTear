using Vintagestory.API.Common;

namespace WearAndTear.Config.Props.rubble
{
    public class WearAndTearRubbleProps
    {
        //TODO something for durability to drop ratio
        public BlockDropItemStack[] Drops { get; set; }

        //TODO see if we can merge shapes somehow
        //TODO see about providing the original block texture codes (from blocktype)
        public AssetLocation Shape { get; set; }

    }
}
