using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace HexNowYouRest.Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.UpdateCover))]
    internal static class PatchPlayerUpdateCover
    {
        private const float NearFireTimeout = 0.25f;

        private static readonly FieldInfo NearFireTimerField = AccessTools.Field(typeof(Player), "m_nearFireTimer");

        private static readonly MethodInfo GetCoverForPointMethod = AccessTools.Method(typeof(Cover), nameof(Cover.GetCoverForPoint));

        private static readonly MethodInfo CheckWetRemovalMethod = AccessTools.Method(typeof(PatchPlayerUpdateCover), nameof(CheckWetRemoval));

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
                new CodeInstruction(OpCodes.Call, CheckWetRemovalMethod)
            });

            return codes;
        }

        private static void CheckWetRemoval(Player player)
        {
            if (player == null || NearFireTimerField == null || !player.InShelter())
            {
                return;
            }

            var nearFireTimer = (float)NearFireTimerField.GetValue(player);

            if (nearFireTimer >= NearFireTimeout)
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
    }
}