using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WearAndTear.Config.Props;

namespace WearAndTear.Interfaces
{
    public interface IWearAndTearProtectivePart : IWearAndTearOptionalPart
    {
        //By default protective parts don't affect efficiency
        //TODO maybe have lubrication as a protective part and allow for it to increase efficiency / reduce resistance
        float IWearAndTearPart.EfficiencyModifier => 1;

        WearAndTearProtectivePartProps ProtectiveProps { get; }
    }
}
