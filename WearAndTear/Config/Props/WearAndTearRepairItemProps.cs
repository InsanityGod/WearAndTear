namespace WearAndTear.Config.Props
{
    public class WearAndTearRepairItemProps
    {

        public string RepairType { get; set; }
        public string RequiredTool { get; set; }
        public int ToolDurabilityCost { get; set; } = 1;
        public float Strength { get; set; } = .5f;
        public string MissingToolLangCode { get; set; }
    }
}