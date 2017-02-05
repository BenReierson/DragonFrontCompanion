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
    }
}