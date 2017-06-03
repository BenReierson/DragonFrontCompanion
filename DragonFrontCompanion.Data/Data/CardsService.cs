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

namespace DragonFrontCompanion.Data
{
    public class CardsService : ICardsService
    {
        public string CardDataInfoUrl { get; set; } = "https://raw.githubusercontent.com/BenReierson/DragonFrontDb/v2/Info.json";
        public string CardDataUpdateUrl { get; set; } = "https://raw.githubusercontent.com/BenReierson/DragonFrontDb/master/AllCards.json";
        public string TraitsDataUpdateUrl { get; set; } = "https://raw.githubusercontent.com/BenReierson/DragonFrontDb/master/CardTraits.json";

        private Cards _cachedCards;

        public event EventHandler<Info> DataUpdateAvailable;
        public event EventHandler<Cards> DataUpdated;

        public async Task<Info> CheckForUpdates()
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

        public async Task<Cards> UpdateCardData()
        {
            using (var client = new HttpClient())
            {
                var latestCardJson = await client.GetStringAsync(CardDataUpdateUrl);
                _cachedCards = Cards.Instance(latestCardJson);
                DataUpdated?.Invoke(this, _cachedCards);
                return _cachedCards;
            }
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
                Deck.CardDictionary = _cachedCards.CardDictionary;
            }
        }

        private async Task<Cards> GetCachedCards()
        {
            return CachedCards ?? await Task.Run(()=>CachedCards = Cards.Instance()).ConfigureAwait(false);
        }

        public CardsService() {
            System.Diagnostics.Debug.WriteLine("Card Service");
        }

        public async Task<ReadOnlyDictionary<string, Card>> GetCardsDictionaryAsync()
        {
            return (await GetCachedCards()).CardDictionary;
        }

        public async Task<ReadOnlyCollection<Card>> GetAllCardsAsync()
        {
            return (await GetCachedCards()).All;
        }

        public async Task<ReadOnlyDictionary<Traits, string>> GetCardTraitsAsync()
        {
            return (await GetCachedCards()).TraitsDictionary;
        }
    }
}
