using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace HexBaseEnhancements.Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.UpdateCover))]
    internal static class PatchPlayerUpdateCover
    {
        private const float NearFireTimeout = 0.25f;

        private static readonly FieldInfo NearFireTimerField = AccessTools.Field(typeof(Player), "m_nearFireTimer");
        private static readonly MethodInfo GetCoverForPointMethod = AccessTools.Method(typeof(Cover), nameof(Cover.GetCoverForPoint));
        private static readonly MethodInfo ApplyBaseEnhancementsMethod = AccessTools.Method(typeof(PatchPlayerUpdateCover), nameof(ApplyBaseEnhancements));

        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();

            var matches = codes
                .Select((instruction, index) => new { instruction, index })
                .Where(x => x.instruction.Calls(GetCoverForPointMethod))
                .Select(x => x.index)
                .ToList();

            if (matches.Count != 1)
            {
                Plugin.Log.LogError($"Player.UpdateCover transpiler expected 1 call to Cover.GetCoverForPoint, but found {matches.Count}. Patch not applied.");
                return codes;
            }

            var insertIndex = matches[0] + 1;

            codes.InsertRange(insertIndex, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, ApplyBaseEnhancementsMethod)
            });

            return codes;
        }

        private static void ApplyBaseEnhancements(Player player)
        {
            if (player == null)
            {
                return;
            }

            if (NearFireTimerField == null || !player.InShelter())
            {
                return;
            }

            var nearFireTimer = (float)NearFireTimerField.GetValue(player);

            if (nearFireTimer >= NearFireTimeout)
            {
                return;
            }

            RemoveWetDebuff(player);
            RestoreHealthAndStamina(player);
            RepairInventory(player);
        }

        private static void RemoveWetDebuff(Player player)
        {
            if (!Plugin.IsRemoveWetDebuffEnabled)
            {
                return;
            }

            var seMan = player.GetSEMan();

            if (seMan == null || !seMan.HaveStatusEffect(SEMan.s_statusEffectWet))
            {
                return;
            }

            seMan.RemoveStatusEffect(SEMan.s_statusEffectWet);
        }

        private static void RestoreHealthAndStamina(Player player)
        {
            if (!Plugin.IsRapidHealthAndStaminaRegenEnabled)
            {
                return;
            }

            player.Heal(player.GetMaxHealth(), false);
            player.AddStamina(player.GetMaxStamina());
        }

        private static void RepairInventory(Player player)
        {
            if (!Plugin.IsAutoRepairEnabled)
            {
                return;
            }

            var inventory = player.GetInventory();
            var items = inventory.GetAllItems();

            var repairedAnyItem = false;

            foreach (var item in items)
            {
                if (!item.m_shared.m_useDurability)
                {
                    continue;
                }

                var maxDurability = item.GetMaxDurability();

                if (item.m_durability >= maxDurability)
                {
                    continue;
                }

                item.m_durability = maxDurability;
                repairedAnyItem = true;
            }

            if (!repairedAnyItem)
            {
                return;
            }

            player.Message(MessageHud.MessageType.TopLeft, "Inventory items repaired");

            if (!Plugin.IsAutoRepairSoundEnabled)
            {
                return;
            }

            var repairSfx = ZNetScene.instance.GetPrefab("sfx_gui_repairitem_workbench");

            if (repairSfx != null)
            {
                UnityEngine.Object.Instantiate(repairSfx, player.transform.position, UnityEngine.Quaternion.identity);
            }
        }
    }
}