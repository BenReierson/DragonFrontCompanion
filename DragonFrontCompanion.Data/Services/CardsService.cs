using DragonFrontDb;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using DragonFrontCompanion.Data.Services;

namespace DragonFrontCompanion.Data;

public class CardsService : ICardsService
{
    private const string CardsFolderName = "Data";
    private const string DefaultCardInfoUrl = "https://raw.githubusercontent.com/BenReierson/DragonFrontDb/{0}/Info.json";

    public static readonly string[] DataSources = { "master", "development" };

    private string _activeDataSource = DataSources[0];
    public string ActiveDataSource
    {
        get
        {
            return _activeDataSource;
        }
        set
        {
            _activeDataSource = value;
            if (Uri.IsWellFormedUriString(value, UriKind.Absolute)) CardDataInfoUrl = value;
            else CardDataInfoUrl = string.Format(DefaultCardInfoUrl, value);
        }
    }

    public string CardDataInfoUrl { get; set; } = string.Format(DefaultCardInfoUrl, DataSources[0]);

    private Cards _cachedCards;
    private bool _updating;

    public event EventHandler<Info> DataUpdateAvailable;
    public event EventHandler<Cards> DataUpdated;

    public async Task<Info> CheckForUpdatesAsync()
    {
        var latestInfo = await GetLatestCardInfo().ConfigureAwait(false);
        var currentVersion = Settings.ActiveCardDataVersion != null ? Settings.ActiveCardDataVersion : Info.Current.CardDataVersion;

        if (latestInfo.CardDataVersion > currentVersion &&
            latestInfo.CardDataCompatibleVersion <= currentVersion)
        {//remote card data is newer and compatible
            DataUpdateAvailable?.Invoke(this, latestInfo);
        }

        return latestInfo;
    }

    private async Task<Cards> GetActiveCardDataAsync()
    {
        try
        {
            if (Settings.ActiveCardDataVersion != Info.Current.CardDataVersion)
            {
                var cardDataFolder = Path.Combine(Settings.AppDataDirectory, CardsFolderName);
                if (!Directory.Exists(cardDataFolder)) Directory.CreateDirectory(cardDataFolder);
                var cardsFile = Path.Combine(cardDataFolder, Settings.ActiveCardDataVersion.ToString());
                var cardsJson = await File.ReadAllTextAsync(cardsFile).ConfigureAwait(false);

                var traitsFilePath = Path.Combine(cardDataFolder, Settings.ActiveCardDataVersion + "_traits");
                var traitsExist = File.Exists(traitsFilePath);
                string traitsJson = null;
                if (traitsExist)
                    traitsJson = await File.ReadAllTextAsync(traitsFilePath).ConfigureAwait(false);

                return await Cards.Build(cardsJson, traitsJson);
            }
            else
                return await Cards.Build();
        }
        catch (Exception)
        {
            return await Cards.Build();
        }
    }

    private async Task SaveCardDataAsync(Info version, string cardsJson, string traitsJson)
    {
        try
        {
            var cardDataFolder = Path.Combine(Settings.AppDataDirectory, CardsFolderName);
            if (!Directory.Exists(cardDataFolder)) Directory.CreateDirectory(cardDataFolder);
            var cardsFile = Path.Combine(cardDataFolder, version.CardDataVersion.ToString());

            await File.WriteAllTextAsync(cardsFile, cardsJson);

            if (!string.IsNullOrEmpty(traitsJson))
            {
                var traitsFile = Path.Combine(cardDataFolder, version.CardDataVersion + "_traits");
                await File.WriteAllTextAsync(traitsFile, traitsJson);
            }

            Settings.ActiveCardDataVersion = version.CardDataVersion;
        }
        catch (Exception)
        {
        }
    }

    public async Task<Cards> UpdateCardDataAsync()
    {
        if (_updating)
            return null;

        try
        {
            _updating = true;
            //await Task.Delay(5000);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue() { NoCache = true };
                var latestCardInfo = await GetLatestCardInfo();

                var latestCardJson = await client.GetStringAsync(latestCardInfo.CardDataUrl);
                var latestTraitsJson = string.IsNullOrEmpty(latestCardInfo.CardTraitsUrl) ? null : await client.GetStringAsync(latestCardInfo.CardTraitsUrl);

                CachedCards = await Cards.Build(latestCardJson, latestTraitsJson);
                await SaveCardDataAsync(latestCardInfo, latestCardJson, latestTraitsJson);

                DataUpdated?.Invoke(this, CachedCards);
                _updating = false;
                return CachedCards;
            }
        }
        catch (Exception)
        {
            _updating = false;
            return null;
        }
    }

    public async Task ResetCardDataAsync()
    {
        CachedCards = null;
        ActiveDataSource = DataSources[0];
        CardDataInfoUrl = string.Format(DefaultCardInfoUrl, ActiveDataSource);
        Settings.ActiveCardDataVersion = null;
        Settings.HighestNotifiedCardDataVersion = Settings.ActiveCardDataVersion;
        DataUpdated?.Invoke(this, await GetCachedCardsAsync());
    }

    private async Task<Info> GetLatestCardInfo()
    {
        try
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue() { NoCache = true };
                var infoJson = await client.GetStringAsync(CardDataInfoUrl);
                var latestInfo = JsonConvert.DeserializeObject<Info>(infoJson, new LegacyVersionConverter());
                return latestInfo;
            }
        }
        catch (Exception) { return Info.Current; } 
    }

    private Cards CachedCards
    {
        get{ return _cachedCards;}
        set
        {
            if (_cachedCards == value) return;

            _cachedCards = value;
            if (_cachedCards != null) Deck.CardDictionary = _cachedCards.CardDictionary;
        }
    }

    private Task<Cards> _cachingTask;
    private async Task<Cards> GetCachedCardsAsync()
    {
        if (_cachingTask != null && !_cachingTask.IsCompleted)
        {
            await _cachingTask;
        }
        else if (CachedCards == null)
        {
            _cachingTask = GetActiveCardDataAsync();
            CachedCards = await _cachingTask;
        }

        return CachedCards;
    }

    public async Task<ReadOnlyDictionary<string, Card>> GetCardsDictionaryAsync()
    {
        return (await GetCachedCardsAsync()).CardDictionary;
    }

    public async Task<ReadOnlyCollection<Card>> GetAllCardsAsync()
    {
        return (await GetCachedCardsAsync()).All;
    }

    public async Task<ReadOnlyDictionary<string, string>> GetCardTraitsAsync()
    {
        return (await GetCachedCardsAsync()).TraitsDictionary;
    }
}