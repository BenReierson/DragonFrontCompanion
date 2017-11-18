using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DragonFrontCompanion.ViewModel;
using GalaSoft.MvvmLight.Views;
using Moq;
using DragonFrontCompanion;
using DragonFrontCompanion.Data;
using DragonFrontDb;

namespace DragonFrontCompanion.Tests
{
    [TestClass]
    public class SettingsViewModelTests
    {
        Cards cardsDb;
        SettingsViewModel settingsVM;
        Mock<INavigationService> mockNav;
        Mock<ICardsService> mockCardsService;
        Mock<IDialogService> mockDialogService;

        [TestInitialize]
        public void VMSetup()
        {
            cardsDb = new Cards();
            mockCardsService = new Mock<ICardsService>();
            mockCardsService.Setup(c => c.GetAllCardsAsync()).Returns(async () => cardsDb.All);

            mockNav = new Mock<INavigationService>();
            mockDialogService = new Mock<IDialogService>(MockBehavior.Loose);
            settingsVM = new SettingsViewModel(mockNav.Object, mockCardsService.Object, mockDialogService.Object);
        }

        [TestMethod]
        public void TestDeckOverload()
        {
            Assert.AreEqual(Settings.DEFAULT_AllowDeckOverload, settingsVM.AllowDeckOverload);
            settingsVM.AllowDeckOverload = !Settings.DEFAULT_AllowDeckOverload;
            Assert.AreEqual(!Settings.DEFAULT_AllowDeckOverload, settingsVM.AllowDeckOverload);

            //Check change is persisted across instances
            settingsVM = new SettingsViewModel(mockNav.Object, mockCardsService.Object, mockDialogService.Object);
            Assert.AreEqual(!Settings.DEFAULT_AllowDeckOverload, settingsVM.AllowDeckOverload);
        }

        [TestMethod]
        public void TestEnableRandomDeck()
        {
            Assert.AreEqual(Settings.DEFAULT_EnableRandomDeck, settingsVM.EnableRandomDeck);
            settingsVM.EnableRandomDeck = !Settings.DEFAULT_EnableRandomDeck;
            Assert.AreEqual(!Settings.DEFAULT_EnableRandomDeck, settingsVM.EnableRandomDeck);

            //Check change is persisted across instances
            settingsVM = new SettingsViewModel(mockNav.Object, mockCardsService.Object, mockDialogService.Object);
            Assert.AreEqual(!Settings.DEFAULT_EnableRandomDeck, settingsVM.EnableRandomDeck);
        }

        [TestCleanup]
        public void ResetSettings()
        {
            settingsVM.AllowDeckOverload = Settings.DEFAULT_AllowDeckOverload;
            settingsVM.EnableRandomDeck = Settings.DEFAULT_EnableRandomDeck;
        }
    }
}
