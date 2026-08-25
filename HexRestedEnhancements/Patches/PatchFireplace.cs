using HarmonyLib;

namespace HexBaseEnhancements.Patches
{
    [HarmonyPatch(typeof(Fireplace), nameof(Fireplace.Awake))]
    internal static class PatchFireplaceAwake
    {
        [HarmonyPostfix]
        private static void Postfix(Fireplace __instance)
        {
            Features.AutoFuelFireplaces.Register(__instance);
        }
    }
}