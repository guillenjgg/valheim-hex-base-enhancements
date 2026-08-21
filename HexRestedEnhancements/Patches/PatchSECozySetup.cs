using HarmonyLib;

namespace HexNowYouRest.Patches
{
    [HarmonyPatch(typeof(SE_Cozy), nameof(SE_Cozy.Setup))]
    internal static class PatchSECozySetup
    {
        private static void Postfix(SE_Cozy __instance)
        {
            if (Plugin.Instance == null || __instance == null)
            {
                return;
            }

            if (__instance.m_statusEffect != "Rested")
            {
                return;
            }

            __instance.m_delay = 2f;
        }
    }
}
