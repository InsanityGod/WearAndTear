using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace WearAndTear.Code.Behaviours.Util
{
    public class WearAndTearMaterialName : CollectibleBehavior
    {
        public WearAndTearMaterialName(CollectibleObject collObj) : base(collObj)
        {
        }

        public override void GetHeldItemName(StringBuilder sb, ItemStack itemStack)
        {
            foreach(var variant in collObj.Variant)
            {
                var str = "material-" + variant.Value;
                var material = Lang.Get(str);
                if(material != str)
                {
                    sb.Append($" ({material})"); 
                    return;
                }
            }
        }
    }
}
