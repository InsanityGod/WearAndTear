using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WearAndTear.Interfaces
{
    public interface IWearAndTearOptionalPart : IWearAndTearPart
    {
        bool IsPresent => Durability != 0;
    }
}
