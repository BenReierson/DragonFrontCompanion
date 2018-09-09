// Helpers/Settings.cs
using DragonFrontDb;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System;

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
        private const string HighestNotifiedDataSettingsKey = "data_notify";

        public const bool DEFAULT_AllowDeckOverload = false;
        public const bool DEFAULT_EnableRandomDeck = true;
        public static string DEFAULT_CardDataVersion = Info.Current.CardDataVersion.ToString();
        #endregion


        public static bool AllowDeckOverload
        {
            get
            {
                return AppSettings.GetValueOrDefault(AllowDeckOverloadSettingsKey, DEFAULT_AllowDeckOverload);
            }
            set
            {
                AppSettings.AddOrUpdateValue(AllowDeckOverloadSettingsKey, value);
            }
        }

        public static bool EnableRandomDeck
        {
            get
            {
                return AppSettings.GetValueOrDefault(EnableRandomDeckSettingsKey, DEFAULT_EnableRandomDeck);
            }
            set
            {
                AppSettings.AddOrUpdateValue(EnableRandomDeckSettingsKey, value);
            }
        }

        public static Version ActiveCardDataVersion
        {
            get
            {
                var setting = Version.Parse(AppSettings.GetValueOrDefault(CardDataVersionSettingsKey, DEFAULT_CardDataVersion));
                if (setting < Info.Current.CardDataVersion) setting = Info.Current.CardDataVersion;
                return setting;
            }
            set
            {
                AppSettings.AddOrUpdateValue(CardDataVersionSettingsKey, value != null ? value.ToString() : DEFAULT_CardDataVersion);
            }
        }

        public static Version HighestNotifiedCardDataVersion
        {
            get
            {
                return Version.Parse(AppSettings.GetValueOrDefault(HighestNotifiedDataSettingsKey, DEFAULT_CardDataVersion));
            }
            set
            {
                AppSettings.AddOrUpdateValue(HighestNotifiedDataSettingsKey, value.ToString());
            }
        }

        public static bool EnableAutoUpdate
        {
            get
            {
                return AppSettings.GetValueOrDefault(EnableAutoUpdateSettingsKey, true);
            }
            set
            {
                AppSettings.AddOrUpdateValue(EnableAutoUpdateSettingsKey, value);
            }
        }
    }
}