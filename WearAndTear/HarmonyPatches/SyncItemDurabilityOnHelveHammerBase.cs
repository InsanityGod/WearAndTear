using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent.Mechanics;

namespace WearAndTear.HarmonyPatches
{
    [HarmonyPatch(typeof(BEHelveHammer), nameof(BEHelveHammer.ToTreeAttributes))]
    public static class SyncItemDurabilityOnHelveHammerBase_ToTree
    {
        public static void Postfix(BEHelveHammer __instance, ITreeAttribute tree)
        {
            //tree.SetInt("itemDurability", __instance.HammerStack?.Collectible?.Durability ?? 1);
        }
    }

    [HarmonyPatch(typeof(BEHelveHammer), nameof(BEHelveHammer.FromTreeAttributes))]
    public static class SyncItemDurabilityOnHelveHammerBase_FromTree
    {
        public static void Postfix(BEHelveHammer __instance, ITreeAttribute tree)
        {
            if (__instance.HammerStack?.Collectible != null)
            {
                //__instance.HammerStack.Collectible.Durability = tree.GetInt("itemDurability", 1);
            }
        }
    }
}