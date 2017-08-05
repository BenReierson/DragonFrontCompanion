using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DragonFrontCompanion.ViewModel;
using GalaSoft.MvvmLight.Views;
using Moq;
using DragonFrontCompanion;
using DragonFrontCompanion.Data;
using System.Threading.Tasks;
using System.Threading;
using DragonFrontDb;
using DragonFrontDb.Enums;
using System.Linq;
using System.Collections.Generic;

namespace DragonFrontCompanion.Tests.ViewModelTests
{
    [TestClass]
    public class CardsViewModelTests
    {
        Cards cardsDb;
        CardsViewModel cardsVM;
        Mock<INavigationService> mockNav;
        Mock<ICardsService> mockCardsService;
        Mock<IDialogService> mockDialogService;
        Mock<IDeckService> mockDeckService;

        public static readonly TimeSpan FILTER_TIMEOUT = TimeSpan.FromMilliseconds(50);

        [TestInitialize]
        public void VMSetup()
        {
            cardsDb = new Cards();
            Deck.CardDictionary = cardsDb.CardDictionary;
            mockNav = new Mock<INavigationService>();
            mockDialogService = new Mock<IDialogService>(MockBehavior.Loose);
            mockDeckService = new Mock<IDeckService>();

            mockCardsService = new Mock<ICardsService>();
            mockCardsService.Setup(c => c.GetAllCardsAsync()).Returns(async () => cardsDb.All);

            cardsVM = new CardsViewModel(mockNav.Object, mockCardsService.Object, mockDialogService.Object, mockDeckService.Object);
        }

        [TestMethod]
        public async Task TestInitalized()
        {
            await cardsVM.InitializeAsync();
            mockCardsService.Verify(c => c.GetAllCardsAsync());
            Assert.IsFalse(cardsVM.IsBusy);
            Assert.AreEqual(cardsDb.All.Count, cardsVM.AllCards.Count);
            Assert.IsTrue(cardsVM.CardsTitle.Contains("Cards"));
            Assert.IsFalse(cardsVM.IsChooser);
        }

        [TestMethod]
        public async Task TestNullDeckInitialize()
        {
            await cardsVM.InitializeAsync();
            Assert.IsFalse(cardsVM.IsChooser);
            Assert.AreEqual(cardsDb.All.Count, cardsVM.AllCards.Count);

            VerifyNonChooserFilterDefaults();
        }

        [TestMethod]
        public async Task TestNewEclipseDeckInitialize()
        {
            await cardsVM.InitializeAsync(new Deck(Faction.ECLIPSE));

            Assert.IsTrue(cardsVM.IsChooser);
            Assert.AreEqual(cardsDb.All.Count(c => c.Rarity != Rarity.TOKEN && c.ValidFactions.Contains(Faction.ECLIPSE)), cardsVM.AllCards.Count);

            //Check all filters status
            Assert.IsTrue(cardsVM.CanFilterByEclipse);
            Assert.IsFalse(cardsVM.CanFilterByScales);
            Assert.IsFalse(cardsVM.CanFilterBySilence);
            Assert.IsFalse(cardsVM.CanFilterByStrife);
            Assert.IsFalse(cardsVM.CanFilterByThorns);

            Assert.IsFalse(cardsVM.FilterByDeck);
            Assert.IsFalse(cardsVM.FilteredByFaction);
            Assert.IsFalse(cardsVM.FilteredByType);

            Assert.AreEqual(CardsViewModel.MAX_COSTS_FILTER, cardsVM.CostFilter);
            Assert.AreEqual((int)Rarity.INVALID, cardsVM.RarityFilter);
            Assert.AreEqual(CardType.INVALID, cardsVM.TypeFilter);
            Assert.AreEqual(null, cardsVM.TraitFilter);
            Assert.AreEqual(Faction.INVALID, cardsVM.FactionFilter);

            Assert.AreEqual(cardsDb.All.Count(c => c.Rarity != Rarity.TOKEN && c.ValidFactions.Contains(Faction.ECLIPSE)), cardsVM.FilteredCards.Count, "All faction cards should be included by default.");

        }

        [TestMethod]
        public async Task TestCostFilter()
        {
            await cardsVM.InitializeAsync(new Deck(Faction.ECLIPSE));
            var unfilteredCards = cardsVM.FilteredCards;

            for (int cost = 0; cost < CardsViewModel.MAX_COSTS_FILTER - 1; cost++)
            {
                cardsVM.CostFilter = cost;
                await Task.Delay(FILTER_TIMEOUT); //Allow filters to run

                foreach (var card in cardsVM.FilteredCards)
                {
                    Assert.AreEqual(cost, card.Cost, $"Only cards that cost {cost} should be included.");
                }
            }

            cardsVM.CostFilter = CardsViewModel.MAX_COSTS_FILTER - 1;
            await Task.Delay(FILTER_TIMEOUT); //Allow filters to run

            foreach (var card in cardsVM.FilteredCards)
            {
                Assert.IsTrue(cardsVM.CostFilter <= card.Cost, $"Only cards that cost {cardsVM.CostFilter} should be included.");
            }

            cardsVM.CostFilter = CardsViewModel.MAX_COSTS_FILTER;
            await Task.Delay(FILTER_TIMEOUT); //Allow filters to run

            Assert.AreEqual(unfilteredCards.Count, cardsVM.FilteredCards.Count);
            Assert.IsTrue(unfilteredCards.TrueForAll(c => cardsVM.FilteredCards.Contains(c)));
        }

        [TestMethod]
        public async Task TestFactionFilter()
        {
            await cardsVM.InitializeAsync(new Deck(Faction.ECLIPSE));

            var unfilteredCards = cardsVM.FilteredCards;

            var factions = Enum.GetValues(typeof(Faction));

            foreach (var faction in factions)
            {
                cardsVM.FilterFactionCommand.Execute(faction);
                await Task.Delay(FILTER_TIMEOUT); //Allow filters to run

                if (!((Faction)faction == Faction.INVALID))
                {
                    foreach (var card in cardsVM.FilteredCards)
                    {
                        Assert.AreEqual((Faction)faction, card.Faction, $"Only {(Faction)faction} cards should be included.");
                    }
                }
            }

            cardsVM.ResetFactionFiltersCommand.Execute(null);
            await Task.Delay(FILTER_TIMEOUT); //Allow filters to run

            Assert.AreEqual(unfilteredCards.Count, cardsVM.FilteredCards.Count);
            Assert.IsTrue(unfilteredCards.TrueForAll(c => cardsVM.FilteredCards.Contains(c)));
        }

        [TestMethod]
        public async Task TestTypeFilter()
        {
            await cardsVM.InitializeAsync(new Deck(Faction.ECLIPSE));

            var unfilteredCards = cardsVM.FilteredCards;

            var types = Enum.GetValues(typeof(CardType));

            foreach (var type in types)
            {
                cardsVM.FilterCardTypeCommand.Execute(type);
                await Task.Delay(FILTER_TIMEOUT); //Allow filters to run

                if (!((CardType)type == CardType.INVALID))
                {
                    foreach (var card in cardsVM.FilteredCards)
                    {
                        Assert.AreEqual((CardType)type, card.Type, $"Only {(CardType)type} cards should be included.");
                    }
                }
            }

            cardsVM.ResetTypeFilterCommand.Execute(null);
            await Task.Delay(FILTER_TIMEOUT); //Allow filters to run

            Assert.AreEqual(unfilteredCards.Count, cardsVM.FilteredCards.Count);
            Assert.IsTrue(unfilteredCards.TrueForAll(c => cardsVM.FilteredCards.Contains(c)));
        }

        [TestMethod]
        public async Task TestRarityFilter()
        {
            await cardsVM.InitializeAsync(new Deck(Faction.ECLIPSE));

            var unfilteredCards = cardsVM.FilteredCards;
            var rarities = Enum.GetValues(typeof(Rarity));

            foreach (var rarity in rarities)
            {
                cardsVM.RarityFilter = (int)rarity;
                await Task.Delay(FILTER_TIMEOUT); //Allow filters to run

                if (!((Rarity)rarity == Rarity.INVALID))
                {
                    foreach (var card in cardsVM.FilteredCards)
                    {
                        Assert.AreEqual(rarity, card.Rarity, $"Only {(Rarity)rarity} cards should be included.");
                    }
                }
            }

            cardsVM.RarityFilter = (int)Rarity.INVALID;
            await Task.Delay(FILTER_TIMEOUT); //Allow filters to run

            Assert.AreEqual(unfilteredCards.Count, cardsVM.FilteredCards.Count);
            Assert.IsTrue(unfilteredCards.TrueForAll(c => cardsVM.FilteredCards.Contains(c)));
        }

        [TestMethod]
        public async Task TestTraitsFilter()
        {
            await cardsVM.InitializeAsync();

            var unfilteredCards = cardsVM.FilteredCards;

            var shuffledCards = new List<Card>(cardsVM.AllCards);
            shuffledCards.Shuffle();

            //test on a subset of cards with traits to reduce overall test time
            foreach (var card in shuffledCards.Where(c=>c.Traits?.Count > 0).Take(20))
            {
                cardsVM.FindSimilarCommand.Execute(card);
                await Task.Delay(FILTER_TIMEOUT); //Allow filters to run

                Assert.IsTrue(cardsVM.FilteredCards.Count > 0);
                foreach (var filtered in cardsVM.FilteredCards)
                {
                    Assert.IsTrue(card.Traits.Intersect(filtered.Traits).Any());
                }
            }

            cardsVM.ResetFiltersCommand.Execute(null);
            await Task.Delay(FILTER_TIMEOUT); //Allow filters to run

            Assert.AreEqual(unfilteredCards.Count, cardsVM.FilteredCards.Count);
            Assert.IsTrue(unfilteredCards.TrueForAll(c => cardsVM.FilteredCards.Contains(c)));
        }

        [TestMethod]
        public async Task TestTextSearchFilter()
        {
            await cardsVM.InitializeAsync();

            var unfilteredCards = cardsVM.FilteredCards;
            var matchCard = unfilteredCards[1];

            //Search for card name
            cardsVM.SearchText = matchCard.Name;
            await Task.Delay(FILTER_TIMEOUT); //Allow filters to run
            Assert.AreEqual(1, cardsVM.FilteredCards.Count());
            Assert.AreEqual(matchCard, cardsVM.FilteredCards[0]);

            //Search by card text
            matchCard = unfilteredCards[2];
            cardsVM.SearchText = matchCard.Text;
            await Task.Delay(FILTER_TIMEOUT); //Allow filters to run
            Assert.AreEqual(matchCard, cardsVM.FilteredCards[0]);

            //Search by race
            matchCard = unfilteredCards.First(c => c.Race != Race.NONE);
            cardsVM.SearchText = matchCard.Race.ToString();
            await Task.Delay(FILTER_TIMEOUT); //Allow filters to run
            Assert.AreEqual(matchCard, cardsVM.FilteredCards[0]);


            cardsVM.ResetFiltersCommand.Execute(null);
            await Task.Delay(FILTER_TIMEOUT); //Allow filters to run

            Assert.AreEqual(unfilteredCards.Count, cardsVM.FilteredCards.Count);
            Assert.IsTrue(unfilteredCards.TrueForAll(c => cardsVM.FilteredCards.Contains(c)));
        }

        [TestMethod]
        public async Task TestFilterByDeck()
        {
            var deck = new Deck(Faction.SCALES);
            deck.Champion = cardsDb.All.Where(c => c.Faction == Faction.SCALES).First(c => c.Type == CardType.CHAMPION);
            deck.Add(cardsDb.All.Where(c => c.Faction == Faction.SCALES).ToList()[10]);
            deck.Add(cardsDb.All.First(c => c.Faction == Faction.UNALIGNED));

            await cardsVM.InitializeAsync(deck);

            var unfilteredCards = cardsVM.FilteredCards;

            cardsVM.DeckFilterCommand.Execute(null);
            await Task.Delay(FILTER_TIMEOUT); //Allow filters to run

            Assert.AreEqual(3, cardsVM.FilteredCards.Count());

            foreach (var card in deck)
            {
                Assert.IsTrue(cardsVM.FilteredCards.Contains(card));
            }
            Assert.IsTrue(cardsVM.FilteredCards.Contains(deck.Champion));

            cardsVM.ResetFiltersCommand.Execute(null);
            await Task.Delay(FILTER_TIMEOUT); //Allow filters to run

            Assert.AreEqual(unfilteredCards.Count, cardsVM.FilteredCards.Count);
            Assert.IsTrue(unfilteredCards.TrueForAll(c => cardsVM.FilteredCards.Contains(c)));

        }

        [TestMethod]
        public async Task TestAddCardToDeck()
        {
            var deck = new Deck(Faction.STRIFE);

            await cardsVM.InitializeAsync(deck);

            var factionCardToAdd = cardsDb.All.Where(c => c.Faction == Faction.STRIFE).First(c => c.Type != CardType.CHAMPION);
            var unalignedCardToAdd = cardsDb.All.Where(c => c.Faction == Faction.UNALIGNED).First();
            var invalidFactionCard = cardsDb.All.Where(c => c.Faction == Faction.ECLIPSE).First(c => c.Type != CardType.CHAMPION);
            var champToAdd = cardsDb.All.Where(c => c.Faction == Faction.STRIFE).First(c => c.Type == CardType.CHAMPION);

            //Add faction card
            Assert.IsTrue(cardsVM.AddOneToDeckCommand.CanExecute(factionCardToAdd));
            cardsVM.AddOneToDeckCommand.Execute(factionCardToAdd);
            Assert.IsTrue(deck.Contains(factionCardToAdd));

            //Add unaligned card
            Assert.IsTrue(cardsVM.AddOneToDeckCommand.CanExecute(unalignedCardToAdd));
            cardsVM.AddOneToDeckCommand.Execute(unalignedCardToAdd);
            Assert.IsTrue(deck.Contains(unalignedCardToAdd));

            //Add invalid card
            cardsVM.AddOneToDeckCommand.Execute(invalidFactionCard);
            Assert.IsFalse(deck.Contains(invalidFactionCard));

            //Add champion
            Assert.IsTrue(cardsVM.AddOneToDeckCommand.CanExecute(champToAdd));
            cardsVM.AddOneToDeckCommand.Execute(champToAdd);
            Assert.AreEqual(champToAdd, deck.Champion);
        }

        [TestMethod]
        public async Task TestRemoveCardFromDeck()
        {
            var deck = new Deck(Faction.STRIFE);
            
            var factionCardToRemove = cardsDb.All.Where(c=>c.Faction == Faction.STRIFE).First(c => c.Type != CardType.CHAMPION);
            var unalignedCardToRemove = cardsDb.All.Where(c => c.Faction == Faction.UNALIGNED).First(c => c.Type != CardType.CHAMPION);
            var invalidFactionCard = cardsDb.All.Where(c => c.Faction == Faction.ECLIPSE).First(c => c.Type != CardType.CHAMPION);
            var champToRemove = cardsDb.All.Where(c => c.Faction == Faction.STRIFE).First(c => c.Type == CardType.CHAMPION);

            deck.Add(factionCardToRemove);
            deck.Add(unalignedCardToRemove);
            deck.Champion = champToRemove;

            Assert.IsTrue(deck.Contains(factionCardToRemove));
            Assert.IsTrue(deck.Contains(unalignedCardToRemove));
            Assert.AreEqual(champToRemove, deck.Champion);

            await cardsVM.InitializeAsync(deck);

            cardsVM.RemoveCardCommand.Execute(factionCardToRemove);
            Assert.IsFalse(deck.Contains(factionCardToRemove));

            cardsVM.RemoveCardCommand.Execute(unalignedCardToRemove);
            Assert.IsFalse(deck.Contains(unalignedCardToRemove));

            cardsVM.RemoveCardCommand.Execute(champToRemove);
            Assert.IsNull(deck.Champion);
        }

        [TestMethod]
        public async Task TestSaveDeck()
        {
            var deck = new Deck(Faction.THORNS);

            cardsVM.CurrentDeck = deck;
            await Task.Delay(FILTER_TIMEOUT); //Allow filters to run

            Assert.IsTrue(cardsVM.SaveDeckCommand.CanExecute(null));
            cardsVM.SaveDeckCommand.Execute(null);

            mockDeckService.Verify(service => service.SaveDeckAsync(deck));

            deck = new Deck(Faction.SILENCE, type:DeckType.EXTERNAL_DECK);
            cardsVM.CurrentDeck = deck;
            await Task.Delay(FILTER_TIMEOUT); //Allow filters to run

            Assert.IsTrue(cardsVM.SaveDeckCommand.CanExecute(null));
            cardsVM.SaveDeckCommand.Execute(null);

            //mockDialogService.Verify(service => service.ShowMessage(null, null, null, null, null));
            mockDeckService.Verify(service => service.SaveDeckAsync(deck), Times.Once);
        }

        [TestMethod]
        public async Task TestNextAndPreviousCard()
        {
            cardsVM.CurrentDeck = null;
            await Task.Delay(FILTER_TIMEOUT); //Allow filters to run

            var cards = new List<Card>(cardsDb.All.Take(3));

            cardsVM.FilteredCards = cards;
            cardsVM.SelectedCard = cards[1];

            Assert.IsTrue(cardsVM.PreviousCardCommand.CanExecute(null));
            Assert.IsTrue(cardsVM.NextCardCommand.CanExecute(null));

            cardsVM.NextCardCommand.Execute(null);

            Assert.AreEqual(cards[2], cardsVM.SelectedCard);
            Assert.IsFalse(cardsVM.NextCardCommand.CanExecute(null));

            cardsVM.PreviousCardCommand.Execute(null);
            Assert.AreEqual(cards[1], cardsVM.SelectedCard);

            cardsVM.PreviousCardCommand.Execute(null);
            Assert.AreEqual(cards[0], cardsVM.SelectedCard);

            Assert.IsFalse(cardsVM.PreviousCardCommand.CanExecute(null));

        }


        private void VerifyNonChooserFilterDefaults()
        {
            //Check all filters status
            Assert.IsTrue(cardsVM.CanFilterByEclipse);
            Assert.IsTrue(cardsVM.CanFilterByScales);
            Assert.IsTrue(cardsVM.CanFilterBySilence);
            Assert.IsTrue(cardsVM.CanFilterByStrife);
            Assert.IsTrue(cardsVM.CanFilterByThorns);

            Assert.IsFalse(cardsVM.FilterByDeck);
            Assert.IsFalse(cardsVM.FilteredByFaction);
            Assert.IsFalse(cardsVM.FilteredByType);

            Assert.AreEqual(CardsViewModel.MAX_COSTS_FILTER, cardsVM.CostFilter);
            Assert.AreEqual((int)Rarity.INVALID, cardsVM.RarityFilter);
            Assert.AreEqual(CardType.INVALID, cardsVM.TypeFilter);
            Assert.AreEqual(null, cardsVM.TraitFilter);
            Assert.AreEqual(Faction.INVALID, cardsVM.FactionFilter);

            Assert.AreEqual(cardsDb.All.Count, cardsVM.FilteredCards.Count, "All cards should be shown by default.");

        }
        
    }
}
