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
        public WearAndTearPartProps PartProps { get; set; } = new();
        public WearAndTearProtectivePartProps ProtectiveProps { get; set; } = new();

        public JContainer AsMergedJContainer()
        {
            var container = (JContainer)JToken.FromObject(PartProps);
            container.Merge(JToken.FromObject(ProtectiveProps));
            return container;
        }
    }
}
