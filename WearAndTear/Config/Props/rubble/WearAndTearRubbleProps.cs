using System.ComponentModel;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace WearAndTear.Config.Props.rubble
{
    public class WearAndTearRubbleProps
    {
        public const string Key = "rubble";

        public bool Unstable { get; set; } = true;

        public AssetLocation Shape { get; set; }

        public Cuboidf[] CollisionSelectionBoxes { get; set; }

        public bool DamageOnTouch { get; set; }

    }
}
