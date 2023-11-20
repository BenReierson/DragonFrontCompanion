using System.Collections.ObjectModel;
using DragonFrontDb;
namespace DragonFrontCompanion.Data.Services;

public interface ICardsService
{
    string ActiveDataSource { get; set; }
    Task<ReadOnlyCollection<Card>> GetAllCardsAsync();
    Task<ReadOnlyDictionary<string, Card>> GetCardsDictionaryAsync();

    Task<ReadOnlyDictionary<string, string>> GetCardTraitsAsync();

    Task<Info> CheckForUpdatesAsync();
    Task<Cards> UpdateCardDataAsync();
    Task ResetCardDataAsync();

    event EventHandler<Info> DataUpdateAvailable;
    event EventHandler<Cards> DataUpdated;

}