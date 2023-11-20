using DragonFrontDb;
namespace DragonFrontCompanion;

public interface IPreferences
{
    string Get(string key, string defaultValue);
    bool Get(string key, bool defaultValue);
    void Set(string key, string value);
    void Set(string key, bool value);
    bool ContainsKey(string key);
    
    string AppDataDirectory { get; }
}

public class MauiPreferences : IPreferences
{
    public string Get(string key, string defaultValue) => Preferences.Get(key, defaultValue);
    public bool Get(string key, bool defaultValue) => Preferences.Get(key, defaultValue);

    public void Set(string key, string value) => Preferences.Set(key, value);
    public void Set(string key, bool value) => Preferences.Set(key, value);
    public bool ContainsKey(string key) => Preferences.ContainsKey(key);

    public string AppDataDirectory => FileSystem.AppDataDirectory;
}

public static class Settings
{
    public static IPreferences Preferences { get; set; } = new MauiPreferences();
    
    public static string AppDataDirectory => Preferences.AppDataDirectory;
    
    public static bool AllowDeckOverload
    {
        get => Preferences.Get(nameof(AllowDeckOverload), false);
        set => Preferences.Set(nameof(AllowDeckOverload), value);
    }
    
    public static Version ActiveCardDataVersion
    {
        get
        {
            var setting = Version.Parse(Preferences.Get(nameof(ActiveCardDataVersion), Info.Current.CardDataVersion.ToString()));
            if (setting < Info.Current.CardDataVersion) setting = Info.Current.CardDataVersion;
            return setting;
        }
        set => Preferences.Set(nameof(ActiveCardDataVersion), value?.ToString());
    }
    
    public static Version HighestNotifiedCardDataVersion
    {
        get => Version.Parse(Preferences.Get(nameof(HighestNotifiedCardDataVersion), Info.Current.CardDataVersion.ToString()));
        set => Preferences.Set(nameof(HighestNotifiedCardDataVersion), value?.ToString());
    }
    
    public static bool EnableRandomDeck
    {
        get => Preferences.Get(nameof(EnableRandomDeck), false);
        set => Preferences.Set(nameof(EnableRandomDeck), value);
    }
    public static bool EnableAutoUpdate
    {
        get => Preferences.Get(nameof(EnableAutoUpdate), true);
        set => Preferences.Set(nameof(EnableAutoUpdate), value);
    }
}