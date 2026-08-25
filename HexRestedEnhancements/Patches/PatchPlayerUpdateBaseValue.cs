using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace HexBaseEnhancements.Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.UpdateBaseValue))]
    internal static class PatchPlayerUpdateBaseValue
    {
        private static readonly FieldInfo BaseValueField = AccessTools.Field(typeof(Player), "m_baseValue");
        private static readonly MethodInfo TryAutoFuelIfInBaseMethod = AccessTools.Method(typeof(PatchPlayerUpdateBaseValue), nameof(TryAutoFuelIfInBase));

        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();

            var matches = codes
                .Select((instruction, index) => new { instruction, index })
                .Where(x =>
                    x.instruction.opcode == OpCodes.Stfld &&
                    Equals(x.instruction.operand, BaseValueField))
                .Select(x => x.index)
                .ToList();

            if (matches.Count != 1)
            {
                return codes;
            }

            var insertIndex = matches[0] + 1;

            codes.InsertRange(insertIndex, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, TryAutoFuelIfInBaseMethod)
            });

            return codes;
        }

        private static void TryAutoFuelIfInBase(Player player)
        {
            if (player == null || BaseValueField == null)
            {
                return;
            }

            var baseValue = (int)BaseValueField.GetValue(player);

            if (baseValue <= 0)
            {
                return;
            }

            Features.AutoFuelFireplaces.TryAutoFuel(player);
        }
    }
}