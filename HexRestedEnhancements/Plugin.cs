using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;

namespace HexBaseEnhancements
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "com.hex.baseenhancements";
        private const string PluginName = "HexBaseEnhancements";
        private const string PluginVersion = "1.0.0";

        private Harmony _harmonyInstance;
        private ConfigEntry<bool> _isRemoveWetDebuffEnabled;
        private ConfigEntry<bool> _isCustomRestedDelayEnabled;
        private ConfigEntry<int> _restedDelayInSeconds;
        private ConfigEntry<bool> _isRapidHealthAndStaminaRegenEnabled;
        private ConfigEntry<bool> _isAutoRepairEnabled;
        private ConfigEntry<bool> _isDoorOpenFaster;
        private ConfigEntry<bool> _isDoorsAutoClose;
        private ConfigEntry<int> _doorAutoCloseInSeconds;
        private ConfigEntry<bool> _isIncreasedComfortRadiusEnabled;
        private ConfigEntry<bool> _isAutoRepairSoundEnabled;
        private ConfigEntry<int> _comfortRadiusInMeters;

        internal static ManualLogSource Log;
        internal static Plugin Instance;

        internal static bool IsRemoveWetDebuffEnabled => Instance?._isRemoveWetDebuffEnabled?.Value ?? false;
        internal static bool IsCustomRestedDelayEnabled => Instance?._isCustomRestedDelayEnabled?.Value ?? false;
        internal static int RestedDelayInSeconds => Instance?._restedDelayInSeconds?.Value ?? 2;
        internal static bool IsRapidHealthAndStaminaRegenEnabled => Instance?._isRapidHealthAndStaminaRegenEnabled?.Value ?? false;
        internal static bool IsAutoRepairEnabled => Instance?._isAutoRepairEnabled?.Value ?? false;
        internal static bool DoorsOpensFaster => Instance?._isDoorOpenFaster?.Value ?? false;
        internal static bool DoorsAutoClose => Instance?._isDoorsAutoClose?.Value ?? false;
        internal static int DoorAutoCloseInSeconds => Instance?._doorAutoCloseInSeconds?.Value ?? 1;
        internal static bool IsIncreasedComfortRadiusEnabled => Instance?._isIncreasedComfortRadiusEnabled?.Value ?? false;
        internal static int ComfortRadiusInMeters => Instance?._comfortRadiusInMeters?.Value ?? 10;
        internal static bool IsAutoRepairSoundEnabled => Instance?._isAutoRepairSoundEnabled?.Value ?? false;

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

            _isDoorOpenFaster = Config.Bind(
                "General",
                "EnableDoorOpenFaster",
                true,
                "Makes doors open really fast.");

            _isDoorsAutoClose = Config.Bind(
                "General",
                "EnableDoorsAutoClose",
                true,
                "Automatically closes doors after a short delay when opened.");

            _doorAutoCloseInSeconds = Config.Bind(
                "General",
                "DoorAutoCloseInSeconds",
                1,
                "The number of seconds after which doors will automatically close when opened. Only used when EnableDoorsAutoClose is enabled.");

            _isIncreasedComfortRadiusEnabled = Config.Bind(
                "General",
                "EnableIncreasedComfortRadius",
                true,
                "Increases the radius used to detect nearby comfort-providing pieces.");

            _comfortRadiusInMeters = Config.Bind(
                "General",
                "ComfortRadiusInMeters",
                20,
                new ConfigDescription(
                    "The radius in meters used to detect nearby comfort-providing pieces. Only used when EnableIncreasedComfortRadius is enabled.",
                    new AcceptableValueRange<int>(1, 50)));

            _isAutoRepairSoundEnabled = Config.Bind(
                "General",
                "EnableAutoRepairSound",
                true,
                "Plays a sound when auto-repairing items. Only used when EnableAutoRepair is enabled.");

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