// Helpers/Settings.cs
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace DragonFrontCompanion
{
    /// <summary>
    /// This is the Settings static class that can be used in your Core solution or in any
    /// of your client applications. All settings are laid out the same exact way with getters
    /// and setters. 
    /// </summary>
    public static class Settings
    {
        private static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }

        #region Setting Constants

        private const string AllowDeckOverloadSettingsKey = "deck_overload";
        private const string EnableRandomDeckSettingsKey = "random_decks";
        private const string CardDataVersionSettingsKey = "card_data_version";
        private const string EnableAutoUpdateSettingsKey = "auto_update";

        public const bool DEFAULT_AllowDeckOverload = false;
        public const bool DEFAULT_EnableRandomDeck = true;
        #endregion


        public static bool AllowDeckOverload
        {
            get
            {
                return AppSettings.GetValueOrDefault<bool>(AllowDeckOverloadSettingsKey, DEFAULT_AllowDeckOverload);
            }
            set
            {
                AppSettings.AddOrUpdateValue<bool>(AllowDeckOverloadSettingsKey, value);
            }
        }

        public static bool EnableRandomDeck
        {
            get
            {
                return AppSettings.GetValueOrDefault<bool>(EnableRandomDeckSettingsKey, DEFAULT_EnableRandomDeck);
            }
            set
            {
                AppSettings.AddOrUpdateValue<bool>(EnableRandomDeckSettingsKey, value);
            }
        }

        public static string ActiveCardDataVersion
        {
            get
            {
                var setting = AppSettings.GetValueOrDefault<string>(CardDataVersionSettingsKey, null);
                return string.IsNullOrEmpty(setting) ? null : setting;
            }
            set
            {
                AppSettings.AddOrUpdateValue<string>(CardDataVersionSettingsKey, value);
            }
        }

        public static bool EnableAutoUpdate
        {
            get
            {
                return AppSettings.GetValueOrDefault<bool>(EnableAutoUpdateSettingsKey, true);
            }
            set
            {
                AppSettings.AddOrUpdateValue<bool>(EnableAutoUpdateSettingsKey, value);
            }
        }
    }
}