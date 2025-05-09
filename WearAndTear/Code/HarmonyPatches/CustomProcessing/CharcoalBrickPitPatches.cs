using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using WearAndTear.Code.BlockEntities;
using WearAndTear.Code.Blocks;

namespace WearAndTear.Code.HarmonyPatches.CustomProcessing
{
    [HarmonyPatch]
    public static class CharcoalBrickPitPatches
    {
        [HarmonyPatch(typeof(BlockFirepit), nameof(BlockFirepit.TryConstruct))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AllowForConstruction(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = instructions.ToList();
            var label = generator.DefineLabel();

            var methodToFind = AccessTools.Method(typeof(BlockFirepit), nameof(BlockFirepit.IsFirewoodPile));
            var methodToCall = AccessTools.Method(typeof(CharcoalBrickPitPatches), nameof(CharcoalBrickPitLogic));
            for(var i = 0; i < codes.Count; i++)
            {
                if (!codes[i].Calls(methodToFind)) continue;
                
                codes[i - 4].labels.Add(label);
                codes.InsertRange(i - 4, new CodeInstruction[]
                {
                    new(OpCodes.Ldarg_1), //world
                    new(OpCodes.Ldarg_2), //pos
                    new(OpCodes.Ldarg_S, 4), //player
                    new(OpCodes.Call, methodToCall), //Attempt to run logic
                    new(OpCodes.Brfalse_S, label), //If nothing happend, return to original logic
                    new(OpCodes.Ldc_I4_1),
                    new(OpCodes.Ret) //If something does happen then return true
                });

                break;
            }
            return codes;
        }

        public static bool CharcoalBrickPitLogic(IWorldAccessor world, BlockPos pos, IPlayer player)
        {
            if (!BlockCharcoalBrickPit.IsDrySawdustBrickPile(world, pos.DownCopy())) return false;

            Block charcoalPitBlock = world.GetBlock("wearandtear:charcoalbrickpit");
            if (charcoalPitBlock == null) return false;

            world.BlockAccessor.SetBlock(charcoalPitBlock.BlockId, pos);
            world.BlockAccessor.GetBlockEntity<BlockEntityCharcoalBrickPit>(pos)?.Init(player);

            if (player is IClientPlayer clientPlayer) clientPlayer.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);

            return true;
        }
    }
}
