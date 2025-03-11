using HarmonyLib;
using InDappledGroves.Util.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace WearAndTear.Code.HarmonyPatches.immersivewoodsawing
{
    [HarmonyPatchCategory("immersivewoodsawing")]
    [HarmonyPatch]
    public static class CreateSawdust
    {

        [HarmonyPatch("ImmersiveWoodSawing.WoodSawing", nameof(CollectibleBehavior.OnHeldInteractStep))]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var methodToFind = AccessTools.Method(typeof(IBlockAccessor), nameof(IBlockAccessor.BreakBlock));

            for(var i  = 0; i < codes.Count; i++)
            {
                if (codes[i].Calls(methodToFind))
                {
                    codes.InsertRange(i,new CodeInstruction[]
                    {
                        new(OpCodes.Ldloc_1),
                        new(OpCodes.Ldarg_S, 4),
                        new(OpCodes.Call, AccessTools.Method(typeof(CreateSawdust), nameof(SpawnOutput)))
                    });
                    break;
                }
            }

            return codes;
        }

        public static void SpawnOutput(IWorldAccessor world,BlockSelection blockSel)
        {
            var sawdust = world.GetItem(new AssetLocation("wearandtear:sawdust"));

            world.SpawnItemEntity(new ItemStack(sawdust, 1), blockSel.Position.ToVec3d());
        }
    }
}
