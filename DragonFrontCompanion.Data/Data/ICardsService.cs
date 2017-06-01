using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DragonFrontDb;
using DragonFrontDb.Enums;

namespace DragonFrontCompanion.Data
{
    public interface ICardsService
    {
        Task<ReadOnlyCollection<Card>> GetAllCardsAsync();
        Task<ReadOnlyDictionary<string, Card>> GetCardsDictionaryAsync();

        Task<ReadOnlyDictionary<Traits, string>> GetCardTraitsAsync();
    }
}