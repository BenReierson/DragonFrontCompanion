using DragonFrontDb;
using DragonFrontDb.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using PCLStorage;

namespace DragonFrontCompanion.Data
{
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
                return latestInfo;
            }
            else return Info.Current;
        }

        private async Task<Cards> GetActiveCardDataAsync()
        {
            try
            {
                if (Settings.ActiveCardDataVersion != Info.Current.CardDataVersion)
                {
                    var cardDataFolder = await FileSystem.Current.LocalStorage.GetFolderAsync(CardsFolderName).ConfigureAwait(false);
                    var cardsFile = await cardDataFolder.GetFileAsync(Settings.ActiveCardDataVersion.ToString()).ConfigureAwait(false);
                    var cardsJson = await cardsFile.ReadAllTextAsync().ConfigureAwait(false);
                    return new Cards(cardsJson);
                }
                else return await Task.Run(() => new Cards()).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return await Task.Run(() => new Cards()).ConfigureAwait(false);
            }
        }

        private async Task SaveCardDataAsync(Info version, string cardsJson)
        {
            try
            {
                var cardDataFolder = await FileSystem.Current.LocalStorage.CreateFolderAsync(CardsFolderName, CreationCollisionOption.OpenIfExists);
                var cardsFile = await cardDataFolder.CreateFileAsync(version.CardDataVersion.ToString(), CreationCollisionOption.ReplaceExisting);
                await cardsFile.WriteAllTextAsync(cardsJson);

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
                    CachedCards = new Cards(latestCardJson);
                    await SaveCardDataAsync(latestCardInfo, latestCardJson);

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
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue() { NoCache = true };
                var infoJson = await client.GetStringAsync(CardDataInfoUrl);
                var latestInfo = JsonConvert.DeserializeObject<Info>(infoJson);
                return latestInfo;
            }
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

        public async Task<ReadOnlyDictionary<Traits, string>> GetCardTraitsAsync()
        {
            return (await GetCachedCardsAsync()).TraitsDictionary;
        }
    }
}
