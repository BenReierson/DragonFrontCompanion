using DragonFrontDb;
using DragonFrontDb.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonFrontDeckBuilder.Data
{
    public class CardsService : ICardsService
    {
        public async Task<ReadOnlyDictionary<string, Card>> GetCardsDictionaryAsync()
        {
            return Cards.CardDictionary;
        }

        public async Task<ReadOnlyCollection<Card>> GetAllCardsAsync()
        {
            return Cards.All;
        }

        public async Task<ReadOnlyCollection<Card>> GetAllClassCardsAsync(CardClass cardClass)
        {
            switch (cardClass)
            {
                case CardClass.INVALID:
                    throw new ArgumentException("Invalid CardClass");
                    break;
                case CardClass.UNALIGNED:
                    return Cards.AllUnaligned;
                    break;
                case CardClass.ECLIPSE:
                    return Cards.AllEclipse;
                    break;
                case CardClass.SCALES:
                    return Cards.AllScales;
                    break;
                case CardClass.STRIFE:
                    return Cards.AllStrife;
                    break;
                case CardClass.THORNS:
                    return Cards.AllThorns;
                case CardClass.UNKNOWN1:
                case CardClass.UNKNOWN2:
                case CardClass.UNKNOWN3:
                default:
                    return new ReadOnlyCollection<Card>(new List<Card>());
                    break;
            }
        }
    }
}
