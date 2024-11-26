﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using Vintagestory.GameContent.Mechanics;
using WearAndTear.Behaviours;

namespace WearAndTear.HarmonyPatches
{
    [HarmonyPatch(typeof(BEHelveHammer), "onEvery25ms")]
    public static class ConsumeDurabilityOnHelveHammerHit
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool found = false;
            var onHelveHammerHit = AccessTools.Method(typeof(BlockEntityAnvil), nameof(BlockEntityAnvil.OnHelveHammerHit));
            var consumeDurability = AccessTools.Method(typeof(ConsumeDurabilityOnHelveHammerHit), nameof(ConsumeDurability), new Type[] { typeof(BEHelveHammer) });
            foreach (var instruction in instructions)
            {
                if (instruction.Calls(onHelveHammerHit))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, consumeDurability);
                    found = true;
                }
                yield return instruction;
            }
            if (!found) throw new InvalidOperationException("Transpiler failed to find OnHelveHammerHit call to inject code after");
        }

        public static void ConsumeDurability(BEHelveHammer instance)
        {
            var wearAndTearBehaviour = instance.GetBehavior<WearAndTearHelveHammerBlockEntityBehavior>();
            if (wearAndTearBehaviour == null || !wearAndTearBehaviour.Enabled || !wearAndTearBehaviour.ItemCanBeDamaged) return;

            var anvil = Traverse.Create(instance).Field("targetAnvil").GetValue<BlockEntityAnvil>();
            if (anvil.GetType().Name == "FakeBlockEntityAnvil")
            {
                var container = Traverse.Create(anvil)
                    .Field("IDGChoppingBlockContainer")
                    .GetValue<BlockEntityDisplay>();

                if (container == null || container.Inventory.Empty) return;
            }
            else if (!WearAndTearModSystem.Config.DamageHelveHammerEvenIfNothingOnAnvil && anvil.WorkItemStack == null) return;

            var durability = instance.HammerStack.Attributes.GetInt("durability", instance.HammerStack.Collectible.GetMaxDurability(instance.HammerStack));

            if (durability <= 1)
            {
                AssetLocation location = new AssetLocation("sounds/effect/toolbreak");
                instance.Api.World.PlaySoundAt(location, instance.Pos.X, instance.Pos.Y, instance.Pos.Z, null, true, 32f, 1f);
                instance.HammerStack = null;
            }
            else instance.HammerStack.Attributes.SetInt("durability", durability - 1);

            instance.MarkDirty();
        }
    }
}