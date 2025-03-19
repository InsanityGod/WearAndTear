using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Vintagestory.API.Common;
using WearAndTear.Code.Interfaces;

namespace WearAndTear.Config.Props
{
    public class WearAndTearRepairItemProps
    {
        /// <summary>
        /// This should match the RepairType of the part you want it to repair
        /// </summary>
        public string RepairType { get; set; }

        /// <summary>
        /// This should match the MaterialVariant of the part you want it to repair
        /// </summary>
        public AssetLocation RequiredMaterialVariant { get; set;}

        /// <summary>
        /// The tool you are required to hold in offhand while using this material to repair
        /// </summary>
        public string RequiredTool { get; set; }

        /// <summary>
        /// The trait required to use this for maintenance
        /// </summary>
        public string[] RequiredTraits { get; set; } = new string[]
        {
            "wearandtear-engineer"
        };

        /// <summary>
        /// How much durability the tool loses on repair
        /// </summary>
        [Range(0, int.MaxValue)]
        [DefaultValue(1)]
        public int ToolDurabilityCost { get; set; } = 1;

        /// <summary>
        /// How much is repaired by using this item
        /// (a strength of 1 would fully repair the item while 0 wouldn't do anything)
        /// </summary>
        [Range(0, 1)]
        [DefaultValue(.5f)]
        [DisplayFormat(DataFormatString = "P")]
        public float Strength { get; set; } = .5f;

        /// <summary>
        /// This language code is used to get the message displayed when trying to repair without the required tool
        /// </summary>
        public string MissingToolLangCode { get; set; }
    }
}