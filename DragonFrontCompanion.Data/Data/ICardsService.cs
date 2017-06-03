using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DragonFrontDb;
using DragonFrontDb.Enums;
using System;

namespace DragonFrontCompanion.Data
{
    public interface ICardsService
    {
        Task<ReadOnlyCollection<Card>> GetAllCardsAsync();
        Task<ReadOnlyDictionary<string, Card>> GetCardsDictionaryAsync();

        Task<ReadOnlyDictionary<Traits, string>> GetCardTraitsAsync();

        Task<Info> CheckForUpdates();
        Task<Cards> UpdateCardData();

        event EventHandler<Info> DataUpdateAvailable;
        event EventHandler<Cards> DataUpdated;

    }
}