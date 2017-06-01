using DragonFrontDb;
using DragonFrontDb.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonFrontCompanion.Data
{
    public class CardsService : ICardsService
    {
        private Cards _cachedCards;

        public Cards CachedCards
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
