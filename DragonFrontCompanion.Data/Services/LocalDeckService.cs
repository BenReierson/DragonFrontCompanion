using DragonFrontCompanion.Data.Services;
using Newtonsoft.Json;
using DragonFrontDb.Enums;
using DragonFrontDb;
using DragonFrontCompanion.Data.Helpers;
using System.Text;

namespace DragonFrontCompanion.Data;

public class LocalDeckService : IDeckService
{
    public static string AppVersion = "NA";

    public const string SAVED_DECKS_FOLDER_NAME = "Dragon Front Decks";

    private static Deck _lastDelectedDeck = null;
    private static Dictionary<Guid, string> _deckUndoStates = new Dictionary<Guid, string>();

    private ICardsService _cardsService;
    private Deck _randomDeck;
    private Task _initializing;
    
    public event EventHandler<Deck> DeckChanged;

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

    private static async Task<string> GetDecksFolderAsync()
    {
        var rootFolder = Settings.AppDataDirectory;
        var decksFolderPath = Path.Combine(rootFolder, SAVED_DECKS_FOLDER_NAME);
        if (!Directory.Exists(decksFolderPath))
            Directory.CreateDirectory(decksFolderPath);
            
        return decksFolderPath;
    }

    public async Task<bool> DeleteDeckAsync(Deck deckToDelete)
    {
        return await DeleteDeckAsync(deckToDelete, true);
    }

    private async Task<bool> DeleteDeckAsync(Deck deckToDelete, bool allowRestore = true)
    {
        var fileToDelete = deckToDelete.FilePath;
        if (!File.Exists(deckToDelete.FilePath))
        {//try to find the file by deck id
            var folder = await GetDecksFolderAsync();
            var deckFiles = Directory.EnumerateFiles(folder);
            foreach (var deck in deckFiles)
            {
                if (deck.Contains(deckToDelete.ID.ToString()))
                    fileToDelete = deck;
            }
        }

        if (File.Exists(fileToDelete))
        {
            File.Delete(fileToDelete);
            if (allowRestore) _lastDelectedDeck = deckToDelete;
            return true;
        }

        return false;
    }

    public async Task<Deck> GetSavedDeckAsync(Guid ID)
    {
        if (!_initializing.IsCompleted) await _initializing;

        var folder = await GetDecksFolderAsync();
        var deckFiles = Directory.EnumerateFiles(folder)?.ToList();

        if (deckFiles?.Count > 0)
        {
            var deckFile = deckFiles.FirstOrDefault((f) => f.Contains(ID.ToString()));
            if (deckFile != null)
            {
                var text = await File.ReadAllTextAsync(deckFile);
                var newDeck = await Task.Run(() => JsonConvert.DeserializeObject<Deck>(text)).ConfigureAwait(false);
                newDeck.FilePath = deckFile;
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
        var deckFiles = Directory.EnumerateFiles(folder)?.ToList();
        var savedDecks = new List<Deck>();

        if (deckFiles?.Count > 0)
        {
            foreach (var file in deckFiles)
            {
                var fileData = await File.ReadAllTextAsync(file);
                try
                {
                    var newDeck = await Task.Run(() => JsonConvert.DeserializeObject<Deck>(fileData)).ConfigureAwait(false);
                    newDeck.FilePath = file;
                    newDeck.CanUndo = _deckUndoStates.ContainsKey(newDeck.ID);
                    savedDecks.Add(newDeck);
                }
                catch (Exception) { } //TODO:Log/Display error
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
        var cards = (await _cardsService.GetAllCardsAsync()).Where((c) => c.ValidFactions.Contains(faction) && c.Rarity != Rarity.TOKEN).ToList();
        cards.Shuffle();
        deck.Champion = cards.FirstOrDefault((c) => c.Type == CardType.CHAMPION);
        while (!deck.IsValid)
        {
            try
            {
                cards.Shuffle();
                if (cards[0].Type != CardType.CHAMPION && deck.CountCard(cards[0]) < Deck.MAX_CARD_COUNT) deck.Add(cards[0]);
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
        var json = await File.ReadAllTextAsync(filePath);
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                var deck = await Task.Run(()=>JsonConvert.DeserializeObject<Deck>(json)).ConfigureAwait(false);
                deck.FilePath = filePath;
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
        
        DeckChanged?.Invoke(this, restoredDeck);

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
            var file = Path.Combine(folder, deckToSave.ID + ".dfd");

            deckToSave.LastModified = DateTime.Now;
            newJson = await Task.Run(() => JsonConvert.SerializeObject(deckToSave, Formatting.Indented));

            if (existingDeck != null)
            {//enable undo
                _deckUndoStates.Remove(deckToSave.ID);
                _deckUndoStates.Add(deckToSave.ID, oldJson);
                deckToSave.CanUndo = true;
            }

            // if (File.Exists(file)) File.Delete(file);
            // File.Create(file);
            await File.WriteAllTextAsync(file, newJson);
            deckToSave.FilePath = file;

            if (_randomDeck == deckToSave) _randomDeck = null;
        }

        DeckChanged?.Invoke(this, deckToSave);
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
                
            DeckChanged?.Invoke(this, restoredDeck);
            return restoredDeck;
        }
        catch (Exception) { }

        return null;
    }

    public string GetDeckVersionFromDeckJson(string deckJson)
    {
        string version = null;

        if (deckJson?.Length > 0)
        {
            var index = deckJson?.IndexOf(nameof(Deck.AppVersion));
            if (index.HasValue && deckJson.Length > index + 19)
            { version = deckJson.Substring(index.Value + 14, 5); }
        }

        return version;
    }


    public string SerializeToDeckString(Deck deck)
    {
        if (deck == null)
            throw new ArgumentNullException("Deck can not be null");
        if (!deck.IsValid)
            throw new ArgumentException("Deck is invalid or incomplete");
        if (!deck.Champion.Guid.HasValue || deck.Any(c => !c.Guid.HasValue))
            throw new ArgumentException("Deck contains cards with global ids");

        using (var ms = new MemoryStream())
        {
            void Write(int value)
            {
                if (value == 0)
                    ms.WriteByte(0);
                else
                {
                    var bytes = VarInt.GetBytes((ulong)value);
                    ms.Write(bytes, 0, bytes.Length);
                }
            }

            ms.WriteByte(0);
            Write(1);                                                           //encode format - always 1 currently

            var versionBytes = Encoding.ASCII.GetBytes(deck.AppVersion);
            Write(versionBytes.Length);                                         //app version bytes length
            Array.ForEach(versionBytes, ms.WriteByte);                          //app version bytes

            Write((int)deck.DeckFaction);                                       //faction id


            var cards = deck.DistinctView
                .ToDictionary(k => k.Card.Guid.Value, g => g.Count)
                .OrderBy(x => x.Key).ToList();

            var singleCopy = cards.Where(x => x.Value == 1).ToList();
            var doubleCopy = cards.Where(x => x.Value == 2).ToList();
            var nCopy = cards.Where(x => x.Value > 2).ToList();

            Write(singleCopy.Count);                                            //single copy count
            foreach (var card in singleCopy)                                    //single copy ids
                Write(card.Key);

            Write(doubleCopy.Count);                                            //double copy count
            foreach (var card in doubleCopy)                                    //double copy ids
                Write(card.Key);

            Write(nCopy.Count);                                                 //n copy count
            foreach (var card in nCopy)                                         //n copy ids
            {
                Write(card.Key);
                Write(card.Value);
            }


            return Convert.ToBase64String(ms.ToArray());
        }
    }

    public Deck DeserializeDeckString(string deckString)
    {
        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(deckString);
        }
        catch (Exception e)
        {
            throw new ArgumentException("Input is not a valid deck string.", e);
        }
        var offset = 0;
        ulong Read()
        {
            if (offset > bytes.Length)
                throw new ArgumentException("Input is not a valid deck string.");
            var value = VarInt.ReadNext(bytes.Skip(offset).ToArray(), out var length);
            offset += length;
            return value;
        }

        //Zero byte
        offset++;

        //encode version
        var encodeVersion = Read();
        if ((int)encodeVersion != 1)
        {//we only current suport version 1
            throw new ArgumentException($"Unrecognized deck encoding version ({encodeVersion}). Please ensure you are using the latest app.");
        }

        var appVersionLength = (int)Read();
        byte[] appVersionBytes = bytes.Skip(offset).Take(appVersionLength).ToArray();
        offset += appVersionLength;
        var appVersion = Encoding.ASCII.GetString(appVersionBytes);

        var faction = (int)Read();

        var deck = new Deck((Faction)faction, appVersion, DeckType.EXTERNAL_DECK);

        deck.Name = "Imported";
        deck.Description = $"{DateTime.Now:f}";

        var cardGuidDictionary = Deck.CardDictionary.Values.ToDictionary(c => c.Guid.Value, c=>c);

        void AddCard(int? cardGuid = null, int count = 1)
        {
            cardGuid = cardGuid ?? (int)Read();

            for (int i = 0; i < count; i++)
            {
                deck.Add(cardGuidDictionary[cardGuid.Value]);
            }
        }

        var numSingleCards = (int)Read();
        for (var i = 0; i < numSingleCards; i++)
            AddCard();

        var numDoubleCards = (int)Read();
        for (var i = 0; i < numDoubleCards; i++)
            AddCard(count: 2);

        var numMultiCards = (int)Read();
        for (var i = 0; i < numMultiCards; i++)
        {
            var dbfId = (int)Read();
            var count = (int)Read();
            AddCard(dbfId, count);
        }

        return deck;
    }
}