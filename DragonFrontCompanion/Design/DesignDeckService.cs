using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DragonFrontCompanion.Data;
using DragonFrontDb.Enums;
using DragonFrontDb;

namespace DragonFrontCompanion.Design
{
    public class DesignDeckService : IDeckService
    {
        public bool DeckRestoreAvailable
        {
            get
            {
                return true;
            }
        }

        public async Task<bool> DeleteDeckAsync(Deck deckToDelete)
        {
            return true;
        }

        public Task<Deck> GetSavedDeckAsync(Guid ID)
        {
            return null;
        }

        public async Task<List<Deck>> GetSavedDecksAsync()
        {
            var decks = new List<Deck>() { new Deck(Faction.THORNS, "Design") { Name = "Design Deck", Description = "Design Description" } };
            decks[0].Add(Cards.Instance().All.First(c=>c.Faction == Faction.THORNS));
            return decks;
        }

        public Task<Deck> OpenDeckDataAsync(string deckData, bool sourceExternal = true)
        {
            throw new NotImplementedException();
        }

        public Task<Deck> OpenDeckFileAsync(string filePath, bool sourceExternal = true)
        {
            throw new NotImplementedException();
        }

        public Task<Deck> RestoreLastDeletedDeck(bool saveFirst = false)
        {
            return null;
        }

        public async Task<Deck> SaveDeckAsync(Deck deckToSave)
        {
            return deckToSave;
        }

        public async Task<Deck> UndoLastSave(Deck deckToUndo)
        {
            return deckToUndo;
        }
    }
}
