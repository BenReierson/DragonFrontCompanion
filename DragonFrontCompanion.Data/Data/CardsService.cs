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
        
        public CardsService() {
            System.Diagnostics.Debug.WriteLine("Card Service");
        }

        public async Task<ReadOnlyDictionary<string, Card>> GetCardsDictionaryAsync()
        {
            return await Task.Run(()=> Cards.CardDictionary);
        }

        public async Task<ReadOnlyCollection<Card>> GetAllCardsAsync()
        {
            await Task.Delay(100);
            return await Task.Run(() => Cards.All);
        }

        public async Task<ReadOnlyCollection<Card>> GetAllFactionCardsAsync(Faction cardFaction)
        {
            switch (cardFaction)
            {
                case Faction.INVALID:
                    throw new ArgumentException("Invalid CardClass");
                case Faction.UNALIGNED:
                    return await Task.Run(() => Cards.AllUnaligned);
                case Faction.ECLIPSE:
                    return await Task.Run(() => Cards.AllEclipse);
                case Faction.SCALES:
                    return await Task.Run(() => Cards.AllScales);
                case Faction.STRIFE:
                    return await Task.Run(() => Cards.AllStrife);
                case Faction.THORNS:
                    return await Task.Run(() => Cards.AllThorns);
                case Faction.SILENCE:
                    return await Task.Run(() => Cards.AllSilence);
                case Faction.ESSENCE:
                    return await Task.Run(() => Cards.AllEssence);
                case Faction.DELIRIUM:
                    return await Task.Run(() => Cards.AllDelirium);
                default:
                    return new ReadOnlyCollection<Card>(new List<Card>());
            }
        }
    }
}
