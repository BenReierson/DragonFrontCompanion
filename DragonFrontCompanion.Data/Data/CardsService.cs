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

        public string CardDataInfoUrl { get; set; } = "https://raw.githubusercontent.com/BenReierson/DragonFrontDb/v2_dataVersionTesting/Info.json";
        public string CardDataUpdateUrl { get; set; } = "https://raw.githubusercontent.com/BenReierson/DragonFrontDb/v2_dataVersionTesting/AllCards.json";
        public string TraitsDataUpdateUrl { get; set; } = "https://raw.githubusercontent.com/BenReierson/DragonFrontDb/master/CardTraits.json";

        private Cards _cachedCards;

        public event EventHandler<Info> DataUpdateAvailable;
        public event EventHandler<Cards> DataUpdated;

        public async Task<Info> CheckForUpdatesAsync()
        {
            var latestInfo = await GetLatestCardInfo().ConfigureAwait(false);
            if (latestInfo.CardDataVersion > Info.Current.CardDataVersion &&
                latestInfo.CardDataCompatibleVersion <= Info.Current.CardDataVersion)
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
                if (!(String.IsNullOrEmpty(Settings.ActiveCardDataVersion)))
                {
                    var cardDataFolder = await FileSystem.Current.LocalStorage.GetFolderAsync(CardsFolderName);
                    var cardsFile = await cardDataFolder.GetFileAsync(Settings.ActiveCardDataVersion);
                    var cardsJson = await cardsFile.ReadAllTextAsync();
                    return Cards.Instance(cardsJson);
                }
                else return await Task.Run(() => Cards.Instance()).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return await Task.Run(() => Cards.Instance()).ConfigureAwait(false);
            }
        }

        private async Task SaveCardDataAsync(Info version, string cardsJson)
        {
            try
            {
                var cardDataFolder = await FileSystem.Current.LocalStorage.CreateFolderAsync(CardsFolderName, CreationCollisionOption.OpenIfExists);
                var cardsFile = await cardDataFolder.CreateFileAsync(version.CardDataVersion.ToString(), CreationCollisionOption.ReplaceExisting);
                await cardsFile.WriteAllTextAsync(cardsJson);

                Settings.ActiveCardDataVersion = version.CardDataVersion.ToString();
            }
            catch (Exception)
            {
            }
        }

        public async Task<Cards> UpdateCardDataAsync()
        {
            using (var client = new HttpClient())
            {
                var latestCardInfo = await GetLatestCardInfo();
                var latestCardJson = await client.GetStringAsync(CardDataUpdateUrl);
                CachedCards = Cards.Instance(latestCardJson);
                await SaveCardDataAsync(latestCardInfo, latestCardJson);

                DataUpdated?.Invoke(this, CachedCards);
                return CachedCards;
            }
        }

        public async Task ResetCardDataAsync()
        {
            CachedCards = null;
            Settings.ActiveCardDataVersion = null;
            DataUpdated?.Invoke(this, await GetCachedCardsAsync());
        }

        private async Task<Info> GetLatestCardInfo()
        {
            using (var client = new HttpClient())
            {
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

        public CardsService() {
            System.Diagnostics.Debug.WriteLine("Card Service");
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
