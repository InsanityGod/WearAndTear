using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
