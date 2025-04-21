using System.ComponentModel;
using WearAndTear.Config.Props.rubble;

namespace WearAndTear.Config.Props
{
    public class WearAndTearProps
    {
        /// <summary>
        /// Configuration for rubble that will be left behind when the block breaks
        /// </summary>
        [Browsable(false)] //This is auto filled by the Auto Part Registry
        public WearAndTearRubbleProps Rubble { get; set; }
    }
}