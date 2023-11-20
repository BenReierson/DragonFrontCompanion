namespace DragonFrontCompanion.Tests;

public class TestPreferences : IPreferences
{
    private Dictionary<string, string> preferences = new Dictionary<string, string>();
    
    public string Get(string key, string defaultValue) => preferences.GetValueOrDefault(key, defaultValue);
    public bool Get(string key, bool defaultValue) => bool.Parse(preferences.GetValueOrDefault(key, defaultValue.ToString()));
    public void Set(string key, string value) => preferences[key] = value;
    public void Set(string key, bool value) => preferences[key] = value.ToString();
    public bool ContainsKey(string key) => preferences.ContainsKey(key);
    public string AppDataDirectory => Directory.GetCurrentDirectory();
}