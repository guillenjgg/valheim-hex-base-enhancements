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
        private ConfigEntry<bool> _isRapidHealthAndStaminaRegenEnabled;
        private ConfigEntry<bool> _isAutoRepairEnabled;

        internal static ManualLogSource Log;
        internal static Plugin Instance;

        internal static bool IsRemoveWetDebuffEnabled => Instance?._isRemoveWetDebuffEnabled?.Value ?? false;
        internal static bool IsCustomRestedDelayEnabled => Instance?._isCustomRestedDelayEnabled?.Value ?? false;
        internal static int RestedDelayInSeconds => Instance?._restedDelayInSeconds?.Value ?? 2;
        internal static bool IsRapidHealthAndStaminaRegenEnabled => Instance?._isRapidHealthAndStaminaRegenEnabled?.Value ?? false;
        internal static bool IsAutoRepairEnabled => Instance?._isAutoRepairEnabled?.Value ?? false;

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
                _isCustomRestedDelayEnabled.SettingChanged -= OnCustomRestedDelayEnabledChanged;
            }

            if (_restedDelayInSeconds != null)
            {
                _restedDelayInSeconds.SettingChanged -= OnRestedDelaySecondsChanged;
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

            _isRapidHealthAndStaminaRegenEnabled = Config.Bind(
                "General",
                "EnableRapidHealthAndStaminaRegen",
                true,
                "Rapidly restores health and stamina while the player is resting.");

            _isAutoRepairEnabled = Config.Bind(
                "General",
                "EnableAutoRepair",
                true,
                "Automatically repairs all repairable items in the player's inventory when the player is sheltered and near a fire.");

            _isCustomRestedDelayEnabled.SettingChanged += OnCustomRestedDelayEnabledChanged;
            _restedDelayInSeconds.SettingChanged += OnRestedDelaySecondsChanged;
        }

        private void OnCustomRestedDelayEnabledChanged(object sender, System.EventArgs e)
        {
            Patches.PatchSECozySetup.ApplyRestedDelay();
        }

        private void OnRestedDelaySecondsChanged(object sender, System.EventArgs e)
        {
            Patches.PatchSECozySetup.ApplyRestedDelay();
        }
    }
}