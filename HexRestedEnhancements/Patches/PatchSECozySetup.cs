using HarmonyLib;

namespace HexBaseEnhancements.Patches
{
    [HarmonyPatch(typeof(SE_Cozy), nameof(SE_Cozy.Setup))]
    internal static class PatchSECozySetup
    {
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

            ApplyRestedDelay();
        }

        internal static void ApplyRestedDelay()
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