using System.Reflection;
using DragonFrontCompanion.Data;
using DragonFrontDb;
using DragonFrontDb.Enums;
using Newtonsoft.Json;

namespace DragonFrontCompanion.Tests;

public class DeckTests
{

    [SetUp]
    public void Setup()
    {
        Settings.Preferences = new TestPreferences();
        Settings.ActiveCardDataVersion = Info.Current.CardDataVersion;
        Settings.HighestNotifiedCardDataVersion = Info.Current.CardDataVersion;
    }

    [Test]
    public void TestCreateDeck()
    {
        try
        {
            var testDeck = new Deck(Faction.UNALIGNED);
            Assert.Fail("Should not be able to create an unaligned deck.");
            testDeck = new Deck(Faction.INVALID);
            Assert.Fail("Should not be able to create an invalid deck.");
        }
        catch (ArgumentException) { }

    }

    [Test]
    public async Task TestBuildDeckEclipse()
    {

        var cards = await Cards.Build();
        Deck.CardDictionary = cards.CardDictionary;

        var testDeck = new Deck(Faction.ECLIPSE);

        Assert.IsFalse(testDeck.IsValid);
        Assert.IsTrue(testDeck.Count == 0);
        Assert.IsTrue(testDeck.Champion == null);
        Assert.IsTrue(testDeck.DeckFaction == Faction.ECLIPSE);
        foreach (var group in testDeck.CostDistribution)
        {
            Assert.IsTrue(group.Count == 0);
        }

        var champion = cards.CardDictionary["EcC002"];
        testDeck.Add(champion);

        Assert.IsFalse(testDeck.IsValid);
        Assert.IsTrue(testDeck.Count == 0);

        //ensure champion is not counted in cost distribution
        Assert.IsTrue(testDeck.CostDistribution.First((g) => g.Cost == Deck.MAX_DISTRIBUTION_LEVEL).Count == 0);

        //test champion functions
        Assert.AreEqual(champion, testDeck.Champion);
        try
        {
            int count = testDeck.Count;
            testDeck.Add(champion);
            Assert.AreEqual(count, testDeck.Count);
            Assert.AreEqual(champion, testDeck.Champion);
        }
        catch (ArgumentException) { }
        try
        { //test changing champions via add
            int count = testDeck.Count;
            testDeck.Add(cards.CardDictionary["EcC001"]);
            Assert.AreEqual(count, testDeck.Count);
            Assert.AreNotEqual(champion, testDeck.Champion);
            Assert.AreEqual(cards.CardDictionary["EcC001"], testDeck.Champion);
        }
        catch (ArgumentException) { }

        //test changing champion via property
        testDeck.Champion = champion;
        Assert.AreEqual(champion, testDeck.Champion);

        //test card validation
        var unalignedCard = cards.All.First(c => c.Faction == Faction.UNALIGNED);
        testDeck.Add(unalignedCard);
        Assert.IsFalse(testDeck.IsValid);
        Assert.IsTrue(testDeck.Contains(unalignedCard));
        Assert.IsTrue(testDeck.CountCard(unalignedCard) == 1);
        Assert.IsTrue(testDeck.CostDistribution.First((g) => g.Cost == unalignedCard.Cost).Count == 1);

        //add a second copy
        testDeck.Add(unalignedCard);
        Assert.IsFalse(testDeck.IsValid);
        Assert.IsTrue(testDeck.CountCard(unalignedCard) == 2);
        Assert.IsTrue(testDeck.CostDistribution.First((g) => g.Cost == unalignedCard.Cost).Count == 2);

        //try to add a third copy
        try
        {
            testDeck.Add(unalignedCard);
            Assert.Fail("Adding a third copy should throw and exception.");
        }
        catch (ArgumentException) { }

        //Try to add invalid cards
        try
        {
            testDeck.Add(new Card());
            Assert.Fail("Adding a card not included in master list should throw an exception.");
        }
        catch (ArgumentException) { }
        try
        {
            testDeck.Add(cards.All.First(c => c.Faction == Faction.STRIFE));
            Assert.Fail("Adding a card from a different class should throw an exception.");
        }
        catch (ArgumentException) { }


        //Add a third card of same cost
        var newCard = cards.All.FirstOrDefault((c) => c != unalignedCard && c.Cost == unalignedCard.Cost && c.Faction == Faction.UNALIGNED);
        testDeck.Add(newCard);
        Assert.IsFalse(testDeck.IsValid);
        Assert.IsTrue(testDeck.CountCard(newCard) == 1);
        Assert.IsTrue(testDeck.CostDistribution.First((g) => g.Cost == unalignedCard.Cost).Count == 3);

        //fill deck
        int index = 0;
        var eclipseCards = cards.All.Where(c => c.Faction == Faction.ECLIPSE).ToList();
        while (testDeck.Count < Deck.MAX_CARD_COUNT)
        {
            try
            {
                if (eclipseCards[index].Type == CardType.CHAMPION) index++;
                testDeck.Add(eclipseCards[index]);
                testDeck.Add(eclipseCards[index]);
            }
            catch (ArgumentException) { index++; }
        }
        Assert.IsFalse(testDeck.IsValid);

        //finish deck with a name
        testDeck.Name = "Test Eclipse Deck";

        Assert.IsTrue(testDeck.IsValid);

        //test overfill
        testDeck.CanOverload = false;
        Assert.IsTrue(testDeck.Count == Deck.MAX_CARD_COUNT);
        try
        {
            testDeck.Add(eclipseCards[++index]);
            Assert.Fail("Adding cards beyond the limit should throw an exception.");
        }
        catch (ArgumentException) { }

        testDeck.CanOverload = true;
        testDeck.Add(eclipseCards[++index]);
        Assert.IsTrue(testDeck.Count > Deck.MAX_CARD_COUNT);

        //write to json
        var json = JsonConvert.SerializeObject(testDeck, Formatting.Indented);
        File.WriteAllText("testDeck.json", json);
    }

    [Test]
    public async Task TestLocalDeckService()
    {
        Settings.EnableRandomDeck = true;
        var cardsService = new CardsService();
        var service = new LocalDeckService(cardsService);
        var decks = await service.GetSavedDecksAsync();

        Assert.IsTrue(decks.Count == 1);
        var defaultDeck = decks[0];
        Assert.AreEqual(DeckType.GENERATED_DECK, defaultDeck.Type);

        //create a deck
        var newDeck = new Deck(Faction.THORNS)
        {
            Name = "Service Test Deck"
        };
        newDeck.Add((await Cards.Build()).All.First((c) => c.Faction == Faction.THORNS && c.Type != CardType.CHAMPION));

        var savedDeck = await service.SaveDeckAsync(newDeck);
        Assert.AreEqual(newDeck.ID, savedDeck.ID);
        Assert.AreEqual(newDeck, savedDeck);

        //test retrieving saved deck
        service = new LocalDeckService(cardsService);
        decks = await service.GetSavedDecksAsync();
        var deck = decks[0];
        Assert.AreEqual(savedDeck.ID, deck.ID);

        //unmodified save
        var timestamp = deck.LastModified;
        savedDeck = await service.SaveDeckAsync(deck);
        Assert.AreEqual(timestamp, savedDeck.LastModified);

        //modified save
        savedDeck.Description = "Modified Description";
        savedDeck = await service.SaveDeckAsync(deck);
        Assert.AreNotEqual(timestamp, savedDeck.LastModified);

        Assert.IsTrue(await service.DeleteDeckAsync(deck));

        service = new LocalDeckService(cardsService);
        decks = await service.GetSavedDecksAsync();
        Assert.IsTrue(decks.Count == 1);
        defaultDeck = decks[0];
        Assert.AreEqual(DeckType.GENERATED_DECK, defaultDeck.Type);

        var restoredDeck = await service.RestoreLastDeletedDeck();
        Assert.AreEqual(deck.ID, restoredDeck.ID);

    }

    [Test]
    public async Task TestLoadDeck_v1()
    {
        var deckJson = ReadDeckFile("vone.dfd");
        Assert.IsNotNull(deckJson);

        var cardsService = new CardsService();
        var service = new LocalDeckService(cardsService);
        var deck = await service.OpenDeckDataAsync(deckJson);

        Assert.IsNotNull(deck);
        Assert.AreEqual(Faction.THORNS, deck.DeckFaction);
        Assert.AreEqual(DeckType.EXTERNAL_DECK, deck.Type);
        Assert.AreEqual(30, deck.Count);
        Assert.AreEqual("ThC002", deck.Champion.ID);

    }


    [Test]
    public async Task TestLoadDeck_minimalData()
    {
        var deckJson = ReadDeckFile("minimum.dfd");
        Assert.IsNotNull(deckJson);

        var cardsService = new CardsService();
        var service = new LocalDeckService(cardsService);
        var deck = await service.OpenDeckDataAsync(deckJson);

        Assert.IsNotNull(deck);
        Assert.AreEqual(Faction.THORNS, deck.DeckFaction);
        Assert.AreEqual(DeckType.EXTERNAL_DECK, deck.Type);

    }

    [TearDown]
    public void Cleanup()
    {
        var cardsService = new CardsService();
        var service = new LocalDeckService(cardsService);

        var decksGetter = service.GetSavedDecksAsync().ContinueWith(async (t) =>
        {
            var decks = t.Result;

            foreach (var deck in decks)
            {
                await service.DeleteDeckAsync(deck);
            }
        });
    }

    internal static string ReadDeckFile(string deckFileName)
    {
        string result = string.Empty;

        using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DragonFrontCompanion.Tests.Decks." + deckFileName))
        {
            using (StreamReader sr = new StreamReader(stream))
            {
                result = sr.ReadToEnd();
            }
        }
        return result;
    }
}