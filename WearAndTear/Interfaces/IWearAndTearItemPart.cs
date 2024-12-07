using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using WearAndTear.Behaviours;

namespace WearAndTear.Interfaces
{
    public interface IWearAndTearItemPart : IWearAndTearOptionalPart
    {
        ItemStack ItemStack { get; set; }
        
        ItemSlot ItemSlot { get; }

        bool ItemCanBeDamaged { get; }

        void DamageItem(int amount = 1);
    }
}
