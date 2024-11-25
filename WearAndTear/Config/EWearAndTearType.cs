using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WearAndTear.Config
{
    [Flags]
    public enum EWearAndTearType
    {
        None = 0,
        Wind = 1,
        Rain = 2
    }
}