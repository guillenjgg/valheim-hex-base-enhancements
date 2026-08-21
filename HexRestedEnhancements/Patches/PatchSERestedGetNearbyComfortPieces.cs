using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace HexBaseEnhancements.Patches
{
    [HarmonyPatch(typeof(SE_Rested), nameof(SE_Rested.GetNearbyComfortPieces))]
    internal static class PatchSERestedGetNearbyComfortPieces
    {
        private const float VanillaComfortRadius = 10f;

        private static readonly FieldInfo TempPiecesField = AccessTools.Field(typeof(SE_Rested), nameof(SE_Rested.s_tempPieces));

        private static readonly MethodInfo GetAllComfortPiecesInRadiusMethod = AccessTools.Method(typeof(Piece), nameof(Piece.GetAllComfortPiecesInRadius));

        private static readonly MethodInfo GetComfortRadiusMethod = AccessTools.Method(typeof(PatchSERestedGetNearbyComfortPieces), nameof(GetComfortRadius));

        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(
                false,
                new CodeMatch(OpCodes.Ldc_R4, VanillaComfortRadius),
                new CodeMatch(OpCodes.Ldsfld, TempPiecesField),
                new CodeMatch(instruction => instruction.Calls(GetAllComfortPiecesInRadiusMethod)));

            if (!matcher.IsValid)
            {
                Plugin.Log.LogError("SE_Rested.GetNearbyComfortPieces transpiler could not locate the comfort radius call. Patch not applied.");

                return matcher.InstructionEnumeration();
            }

            matcher.Instruction.opcode = OpCodes.Call;
            matcher.Instruction.operand = GetComfortRadiusMethod;

            return matcher.InstructionEnumeration();
        }

        private static float GetComfortRadius()
        {
            return Plugin.IsIncreasedComfortRadiusEnabled
                ? Plugin.ComfortRadiusInMeters
                : VanillaComfortRadius;
        }
    }
}