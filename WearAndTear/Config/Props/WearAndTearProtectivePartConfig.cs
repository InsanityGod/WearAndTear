using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WearAndTear.Config.Props
{
    public class WearAndTearProtectivePartConfig
    {
        /// <summary>
        /// The general part properties
        /// </summary>
        public WearAndTearPartProps PartProps { get; set; } = new();
        
        /// <summary>
        /// The properties specific to protective parts
        /// </summary>
        public WearAndTearProtectivePartProps ProtectiveProps { get; set; } = new();

        internal JContainer AsMergedJContainer()
        {
            var container = (JContainer)JToken.FromObject(PartProps);
            container.Merge(JToken.FromObject(ProtectiveProps));
            return container;
        }
    }
}
