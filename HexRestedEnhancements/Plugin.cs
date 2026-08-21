using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;

namespace HexRestedEnhancements
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "com.hex.restedenhancements";
        private const string PluginName = "HexRestedEnhancements";
        private const string PluginVersion = "1.0.0";

        private Harmony _harmonyInstance;
        private ConfigEntry<bool> _isRemoveWetDebuffEnabled;
        private ConfigEntry<bool> _isCustomRestedDelayEnabled;
        private ConfigEntry<int> _restedDelayInSeconds;

        internal static ManualLogSource Log;
        internal static Plugin Instance;

        internal static bool IsRemoveWetDebuffEnabled => Instance?._isRemoveWetDebuffEnabled?.Value ?? false;
        internal static bool IsCustomRestedDelayEnabled => Instance?._isCustomRestedDelayEnabled?.Value ?? false;
        internal static int RestedDelayInSeconds => Instance?._restedDelayInSeconds?.Value ?? 2;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            BindConfig();

            var assembly = Assembly.GetExecutingAssembly();
            _harmonyInstance = new Harmony(PluginGuid);
            _harmonyInstance.PatchAll(assembly);

            Log.LogInfo($"{PluginName} v{PluginVersion} loaded.");
        }

        private void OnDestroy()
        {
            if (_isCustomRestedDelayEnabled != null)
            {
                _isCustomRestedDelayEnabled.SettingChanged -= OnCustomRestedDelayChanged;
            }

            if (_restedDelayInSeconds != null)
            {
                _restedDelayInSeconds.SettingChanged -= OnRestedDelayChanged;
            }

            Log.LogInfo($"{PluginName} v{PluginVersion} unloaded.");

            _harmonyInstance?.UnpatchSelf();
            _harmonyInstance = null;
            Instance = null;
            Log = null;
        }

        private void BindConfig()
        {
            _isRemoveWetDebuffEnabled = Config.Bind(
                "General",
                "RemoveWetDebuff",
                true,
                "Removes the Wet status effect when the player is sheltered and near a fire.");

            _isCustomRestedDelayEnabled = Config.Bind(
                "General",
                "CustomRestedDelay",
                true,
                "Enables a custom delay before the player receives the Rested status effect.");

            _restedDelayInSeconds = Config.Bind(
                "General",
                "RestedDelayInSeconds",
                2,
                "The number of seconds the player must rest before receiving the Rested status effect. Only used when CustomRestedDelay is enabled.");

            _isCustomRestedDelayEnabled.SettingChanged += OnCustomRestedDelayChanged;
            _restedDelayInSeconds.SettingChanged += OnRestedDelayChanged;
        }

        private void OnCustomRestedDelayChanged(object sender, System.EventArgs e)
        {
            Patches.PatchSECozySetup.ApplyCurrentDelay();
        }

        private void OnRestedDelayChanged(object sender, System.EventArgs e)
        {
            Patches.PatchSECozySetup.ApplyCurrentDelay();
        }
    }
}