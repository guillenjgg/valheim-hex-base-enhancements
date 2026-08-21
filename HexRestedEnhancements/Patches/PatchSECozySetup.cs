using HarmonyLib;

namespace HexRestedEnhancements.Patches
{
    [HarmonyPatch(typeof(SE_Cozy), nameof(SE_Cozy.Setup))]
    internal static class PatchSECozySetup
    {
        // Vanilla SE_Cozy.m_delay default in Valheim.
        private const float VanillaRestedDelay = 10f;

        private static SE_Cozy _currentCozy;

        [HarmonyPostfix]
        private static void Postfix(SE_Cozy __instance)
        {
            if (__instance == null || __instance.m_statusEffect != "Rested")
            {
                return;
            }

            _currentCozy = __instance;
            ApplyCurrentDelay();
        }

        internal static void ApplyCurrentDelay()
        {
            if (_currentCozy == null)
            {
                return;
            }

            _currentCozy.m_delay = Plugin.IsCustomRestedDelayEnabled
                ? Plugin.RestedDelayInSeconds
                : VanillaRestedDelay;
        }
    }
}