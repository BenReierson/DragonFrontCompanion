using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCLStorage;
using Newtonsoft.Json;
using DragonFrontDb.Enums;
using DragonFrontDb;

namespace DragonFrontCompanion.Data
{
    public class LocalDeckService : IDeckService
    {
        public static string AppVersion = "NA";

        public const string SAVED_DECKS_FOLDER_NAME = "Dragon Front Decks";

        private static Deck _lastDelectedDeck = null;
        private static Dictionary<Guid, string> _deckUndoStates = new Dictionary<Guid, string>();

        private ICardsService _cardsService;
        private Deck _randomDeck;
        private Task _initializing;
        public LocalDeckService(ICardsService cardsService)
        {
            _cardsService = cardsService;
            _initializing = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            Deck.CurrentAppVersion = AppVersion;
            Deck.CardDictionary = await _cardsService.GetCardsDictionaryAsync();
        }

        public bool DeckRestoreAvailable => _lastDelectedDeck != null;

        private static async Task<IFolder> GetDecksFolderAsync()
        {
            var rootFolder = FileSystem.Current.LocalStorage;
            var folder = await rootFolder.CreateFolderAsync(SAVED_DECKS_FOLDER_NAME,
            CreationCollisionOption.OpenIfExists);
            return folder;
        }

        public async Task<bool> DeleteDeckAsync(Deck deckToDelete)
        {
            return await DeleteDeckAsync(deckToDelete, true);
        }

        private async Task<bool> DeleteDeckAsync(Deck deckToDelete, bool allowRestore = true)
        {
            var folder = await GetDecksFolderAsync();
            var deckFiles = await folder?.GetFilesAsync();
            foreach (var deck in deckFiles)
            {
                if (deck.Name.Contains(deckToDelete.ID.ToString()))
                {
                    await deck.DeleteAsync();
                    if (allowRestore) _lastDelectedDeck = deckToDelete;
                    return true;
                }
            }
            return false;
        }

        public async Task<Deck> GetSavedDeckAsync(Guid ID)
        {
            if (!_initializing.IsCompleted) await _initializing;

            var folder = await GetDecksFolderAsync();
            var deckFiles = await folder?.GetFilesAsync();

            if (deckFiles?.Count > 0)
            {
                var deckFile = deckFiles.FirstOrDefault((f) => f.Name.Contains(ID.ToString()));
                if (deckFile != null)
                {
                    var text = await deckFile.ReadAllTextAsync();
                    var newDeck = await Task.Run(() => JsonConvert.DeserializeObject<Deck>(text)).ConfigureAwait(false);
                    newDeck.FilePath = deckFile.Path;
                    return newDeck;
                }
            }

            return null;
        }

        public async Task<List<Deck>> GetSavedDecksAsync()
        {
            if (!_initializing.IsCompleted)
                await _initializing;

            var folder = await GetDecksFolderAsync();
            var deckFiles = await folder?.GetFilesAsync();
            var savedDecks = new List<Deck>();

            if (deckFiles?.Count > 0)
            {
                foreach (var file in deckFiles)
                {
                    var fileData = await file.ReadAllTextAsync();
                    try
                    {
                        var newDeck = await Task.Run(() => JsonConvert.DeserializeObject<Deck>(fileData)).ConfigureAwait(false);
                        newDeck.FilePath = file.Path;
                        newDeck.CanUndo = _deckUndoStates.ContainsKey(newDeck.ID);
                        savedDecks.Add(newDeck);
                    }
                    catch (Exception ex)
                    {
                        string version = "NA";
                        if (fileData?.Length > 0)
                        {
                            var index = fileData?.IndexOf("AppVersion");
                            if (index.HasValue && fileData.Length > index + 19)
                            { version = fileData.Substring(index.Value + 14, 5); }
                        }
                    }
                }

                //sort by date
                savedDecks = savedDecks.OrderByDescending(c => c.LastModified).ToList();
            }

            if (Settings.EnableRandomDeck) savedDecks.Add(_randomDeck ?? (_randomDeck = await CreateRandomDeck()));

            return savedDecks;
        }

        private async Task<Deck> CreateRandomDeck()
        {
            var diceRoll = new Random();
            var faction = (Faction)diceRoll.Next(2, 8);
            var deck = new Deck(faction, AppVersion, DeckType.GENERATED_DECK) { Name = "RANDOM DECK", Description = "I wouldn't recommend actually playing as is. Edit this deck to save it, or share it as a challenge!"};
            var cards = (await _cardsService.GetAllCardsAsync()).Where((c) => c.Faction == faction || c.Faction == Faction.UNALIGNED).ToList();
            cards.Shuffle();
            deck.Champion = cards.FirstOrDefault((c) => c.Type == CardType.CHAMPION);
            while (!deck.IsValid)
            {
                try
                {
                    cards.Shuffle();
                    deck.Add(cards[0]);
                    if (diceRoll.Next(0, 1) == 1) deck.Add(cards[0]);
                }
                catch (Exception) {
                    break;
                }
            }

            return deck;
        }

        public async Task<Deck> OpenDeckDataAsync(string deckData, bool sourceExternal = true)
        {
            if (!_initializing.IsCompleted) await _initializing;

            if (deckData != null)
            {
                try
                {
                    var deck = await Task.Run(() => JsonConvert.DeserializeObject<Deck>(deckData)).ConfigureAwait(false);
                    if (sourceExternal) deck.Type = DeckType.EXTERNAL_DECK;
                    return deck;
                }
                catch (Exception) { }
            }
            return null;
        }

        public async Task<Deck> OpenDeckFileAsync(string filePath, bool sourceExternal = true)
        {
            if (!_initializing.IsCompleted) await _initializing;

            filePath = filePath.Replace(@"file://", "");
            var deckFile = await FileSystem.Current.GetFileFromPathAsync(filePath);
            var json = await deckFile?.ReadAllTextAsync();
            if (json != null)
            {
                try
                {
                    var deck = await Task.Run(()=>JsonConvert.DeserializeObject<Deck>(json)).ConfigureAwait(false);
                    deck.FilePath = deckFile.Path;
                    if (sourceExternal) deck.Type = DeckType.EXTERNAL_DECK;
                    return deck;
                }
                catch (Exception) { }
            }
            return null;
        }

        public async Task<Deck> RestoreLastDeletedDeck(bool saveFirst = false)
        {
            if (_lastDelectedDeck == null) return null;

            var restoredDeck = _lastDelectedDeck;

            if (saveFirst)
            {
                restoredDeck = await SaveDeckAsync(restoredDeck);
            }

            restoredDeck.CanUndo = false;
            _lastDelectedDeck = null;

            return restoredDeck;
        }

        public async Task<Deck> SaveDeckAsync(Deck deckToSave)
        {
            if (!_initializing.IsCompleted) await _initializing;

            var existingDeck = await GetSavedDeckAsync(deckToSave.ID);
            var oldJson = await Task.Run(() => JsonConvert.SerializeObject(existingDeck, Formatting.Indented));
            var newJson = await Task.Run(() => JsonConvert.SerializeObject(deckToSave, Formatting.Indented));

            if (existingDeck == null || oldJson != newJson)
            {
                //assign ID if empty
                if (deckToSave.ID == null || deckToSave.ID == Guid.Empty) deckToSave.ID = Guid.NewGuid();
                if (deckToSave.Type != DeckType.NORMAL_DECK) deckToSave.Type = DeckType.NORMAL_DECK;

                var folder = await GetDecksFolderAsync();
                var file = await folder.CreateFileAsync(deckToSave.ID.ToString() + ".dfd",
                    CreationCollisionOption.ReplaceExisting);

                deckToSave.LastModified = DateTime.Now;
                newJson = await Task.Run(() => JsonConvert.SerializeObject(deckToSave, Formatting.Indented));

                if (existingDeck != null)
                {//enable undo
                    _deckUndoStates.Remove(deckToSave.ID);
                    _deckUndoStates.Add(deckToSave.ID, oldJson);
                    deckToSave.CanUndo = true;
                }

                await file.WriteAllTextAsync(newJson);
                deckToSave.FilePath = file.Path;

                if (_randomDeck == deckToSave) _randomDeck = null;
            }

            return deckToSave;
        }

        public async Task<Deck> UndoLastSave(Deck deckToUndo)
        {
            if (!deckToUndo.CanUndo) throw new ArgumentException("No undo states available for deck.");

            if (!_deckUndoStates.ContainsKey(deckToUndo.ID))
            {
                deckToUndo.CanUndo = false;
                return null;
            }

            try
            {
                var restoredDeck = await Task.Run(() => JsonConvert.DeserializeObject<Deck>(_deckUndoStates[deckToUndo.ID])).ConfigureAwait(false);

                await DeleteDeckAsync(deckToUndo, false);
                await SaveDeckAsync(restoredDeck);
                deckToUndo.CanUndo = false;
                restoredDeck.CanUndo = false;
                
                return restoredDeck;
            }
            catch (Exception) { }

            return null;
        }
    }
}
