using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DragonFrontBase;
using PCLStorage;
using Newtonsoft.Json;
using DragonFrontDb.Enums;
using DragonFrontDb;

namespace DragonFrontDeckBuilder.Data
{
    public class LocalDeckService : IDeckService
    {
        public async Task<bool> DeleteDeckAsync(Deck deckToDelete)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Deck>> GetSavedDecksAsync()
        {
            var rootFolder = FileSystem.Current.LocalStorage;
            var folder = await rootFolder.CreateFolderAsync("DragonFrontDeckBuilder",
                 CreationCollisionOption.OpenIfExists);
            var deckFiles = await folder?.GetFilesAsync();
            var savedDecks = new List<Deck>();

            if (deckFiles?.Count > 0)
            {
                foreach (var file in deckFiles)
                {
                    savedDecks.Add(JsonConvert.DeserializeObject<Deck>(await file.ReadAllTextAsync()));
                }
            }
            else
            {//return sample deck
                var deck = new Deck(CardClass.THORNS) { Name = "Default Deck", Description = "I wouldn't recommend actually playing with this one.", Type = DeckType.HIDDEN_DECK };
                deck.Champion = Cards.AllThorns.FirstOrDefault((c) => c.CardType == CardType.CHAMPION);
                int i = 0;
                while (!deck.IsValid)
                {
                    try { deck.Add(Cards.AllThorns[i++]); } catch (Exception) { }
                }
                savedDecks.Add(deck);
            }

            return savedDecks;
        }

        public async Task<Deck> SaveDeckAsync(Deck deckToSave)
        {
            var rootFolder = FileSystem.Current.LocalStorage;
            var folder = await rootFolder.CreateFolderAsync("DragonFrontDeckBuilder",
                CreationCollisionOption.OpenIfExists);
            var file = await folder.CreateFileAsync(deckToSave.ID.ToString() + ".dfd",
                CreationCollisionOption.ReplaceExisting);

            var json = JsonConvert.SerializeObject(deckToSave);

            await file.WriteAllTextAsync(json);

            return deckToSave;
        }
    }
}
