namespace DragonFrontCompanion.Data;

public interface IDeckService
{
    bool DeckRestoreAvailable { get; }

    Task<List<Deck>> GetSavedDecksAsync();
    Task<Deck> SaveDeckAsync(Deck deckToSave);
    Task<bool> DeleteDeckAsync(Deck deckToDelete);
    Task<Deck> GetSavedDeckAsync(Guid ID);
    Task<Deck> RestoreLastDeletedDeck(bool saveFirst = false);
    Task<Deck> OpenDeckFileAsync(string filePath, bool sourceExternal = true);
    Task<Deck> OpenDeckDataAsync(string deckData, bool sourceExternal = true);
    Task<Deck> UndoLastSave(Deck deckToUndo);

    string SerializeToDeckString(Deck deck);
    Deck DeserializeDeckString(string deckString);

    string GetDeckVersionFromDeckJson(string deckJson);
    
    event EventHandler<Deck> DeckChanged;

}