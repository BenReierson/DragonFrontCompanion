using DragonFrontBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonFrontDeckBuilder.Data
{
    public interface IDeckService
    {
        Task<List<Deck>> GetSavedDecksAsync();
        Task<Deck> SaveDeckAsync(Deck deckToSave);
        Task<bool> DeleteDeckAsync(Deck deckToDelete);

    }
}
