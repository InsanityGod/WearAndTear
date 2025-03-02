using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using WearAndTear.Code.Extensions;
using WearAndTear.Config;

namespace WearAndTear.Code.HarmonyPatches
{
    [HarmonyPatch]
    public static class ClassPatches
    {
        [HarmonyPatch(typeof(CharacterSystem), "loadCharacterClasses")]
        [HarmonyPostfix]
        public static void ModifyClasses(CharacterSystem __instance)
        {
            
            var traitConfigs = Traverse.Create(__instance).Field<ICoreAPI>("api").Value.Assets.Get<TraitConfig[]>("wearandtear:config/traitconfig.json");
            foreach(var traitConfig in traitConfigs)
            {
                if (!traitConfig.XLibPresenceRequirement.IsFullfilled()) continue;
                if (traitConfig.OnlyWithTraitRequirementEnabled && !WearAndTearModSystem.TraitRequirements) continue;
                if (__instance.TraitsByCode.ContainsKey(traitConfig.Code)) continue;

                __instance.traits.Add(traitConfig);
                __instance.TraitsByCode[traitConfig.Code] = traitConfig;

                foreach(var appendTo in traitConfig.AppendToClasses)
                {
                    if (__instance.characterClassesByCode.TryGetValue(appendTo, out var characterClass) && !characterClass.Traits.Contains(traitConfig.Code))
                    {
                        characterClass.Traits = characterClass.Traits.Append(traitConfig.Code);
                    }
                }
            }
        }
    }
}
