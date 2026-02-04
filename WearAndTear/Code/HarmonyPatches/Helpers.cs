using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.GameContent.Mechanics;

namespace WearAndTear.Code.HarmonyPatches;

public static class Helpers
{
    //TODO block interaction for rolling up sails!
    public static IEnumerable<Type> WindmillRotorBehaviorTypes()
    {
        var UDRotor = AccessTools.TypeByName("Millwright.ModSystem.BEBehaviorWindmillRotorUD");
        if(UDRotor != null) yield return UDRotor;

        var baseType = typeof(BEBehaviorMPRotor);
        var derivedTypes = AccessTools.AllTypes().Where(type => type != baseType && baseType.IsAssignableFrom(type) && type.Name.StartsWith(nameof(BEBehaviorWindmillRotor)));
        foreach (var type in derivedTypes)
        {
            if(type == UDRotor) continue; //Skip just in case this switched back to inheriting rotor class
            yield return type;
        }
    }
}
