using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DragonFrontDb;
using DragonFrontDb.Enums;

namespace DragonFrontDeckBuilder.Data
{
    public interface ICardsService
    {
        Task<ReadOnlyCollection<Card>> GetAllCardsAsync();
        Task<ReadOnlyCollection<Card>> GetAllClassCardsAsync(CardClass cardClass);
        Task<ReadOnlyDictionary<string, Card>> GetCardsDictionaryAsync();
    }
}