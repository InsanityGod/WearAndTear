using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Code.Behaviours.Parts.Item;

namespace WearAndTear.HarmonyPatches.ImmersiveOreCrushCompatibility
{
    [HarmonyPatchCategory("immersiveorecrush")]
    [HarmonyPatch]
    public static class ConsumeDurabilityOnHelveHammerHit
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(ImmersiveOreCrush.ImmersiveOreCrush.AnvilWithOreSmashing), nameof(ImmersiveOreCrush.ImmersiveOreCrush.AnvilWithOreSmashing.OnHelveHammerHit));
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            bool found = false;
            var methodToFind = AccessTools.Method(typeof(ImmersiveOreCrush.ImmersiveOreCrush.ImmersiveOreCrushStaticMethods), nameof(ImmersiveOreCrush.ImmersiveOreCrush.ImmersiveOreCrushStaticMethods.HandleNuggetDropStatic));
            var methodToCall = AccessTools.Method(typeof(ConsumeDurabilityOnHelveHammerHit), nameof(TryConsumeDurability));

            for (var i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if (code.opcode == OpCodes.Call && code.operand == methodToFind)
                {
                    codes.InsertRange(i + 1, new CodeInstruction[]
                    {
                        new(OpCodes.Ldarg_0),
                        new(OpCodes.Ldloc_0),
                        new(OpCodes.Call, methodToCall)
                    });
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                throw new InvalidOperationException("Could not apply ImmersiveOreCrush HelveHammer Compatibility Patch");
            }

            return codes;
        }

        public static void TryConsumeDurability(BlockEntityAnvil anvilEntity, ICoreServerAPI serverApi)
        {
            BEHelveHammer helveHammerEntity;

            var pos = anvilEntity.Pos.Copy();

            pos.X -= 3;
            helveHammerEntity = serverApi.World.BlockAccessor.GetBlockEntity<BEHelveHammer>(pos);

            if (helveHammerEntity != null && Traverse.Create(helveHammerEntity).Field<BlockEntityAnvil>("targetAnvil").Value == anvilEntity)
            {
                helveHammerEntity.GetBehavior<HelveItemPart>()?.ManualDamageIem();
                return;
            }

            pos.X += 6;
            helveHammerEntity = serverApi.World.BlockAccessor.GetBlockEntity<BEHelveHammer>(pos);

            if (helveHammerEntity != null && Traverse.Create(helveHammerEntity).Field<BlockEntityAnvil>("targetAnvil").Value == anvilEntity)
            {
                helveHammerEntity.GetBehavior<HelveItemPart>()?.ManualDamageIem();
                return;
            }

            pos.X -= 3;
            pos.Z -= 3;
            helveHammerEntity = serverApi.World.BlockAccessor.GetBlockEntity<BEHelveHammer>(pos);

            if (helveHammerEntity != null && Traverse.Create(helveHammerEntity).Field<BlockEntityAnvil>("targetAnvil").Value == anvilEntity)
            {
                helveHammerEntity.GetBehavior<HelveItemPart>()?.ManualDamageIem();
                return;
            }

            pos.Z += 6;
            helveHammerEntity = serverApi.World.BlockAccessor.GetBlockEntity<BEHelveHammer>(pos);
            if (helveHammerEntity != null && Traverse.Create(helveHammerEntity).Field<BlockEntityAnvil>("targetAnvil").Value == anvilEntity)
            {
                helveHammerEntity.GetBehavior<HelveItemPart>()?.ManualDamageIem();
            }
        }
    }
}