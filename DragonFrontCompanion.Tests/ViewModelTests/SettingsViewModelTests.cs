using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DragonFrontCompanion.ViewModel;
using GalaSoft.MvvmLight.Views;
using Moq;
using DragonFrontCompanion;

namespace DragonFrontCompanion.Tests
{
    [TestClass]
    public class SettingsViewModelTests
    {
        SettingsViewModel settingsVM;
        Mock<INavigationService> mockNav;

        [TestInitialize]
        public void VMSetup()
        {
            mockNav = new Mock<INavigationService>();
            settingsVM = new SettingsViewModel(mockNav.Object);
        }

        [TestMethod]
        public void TestDeckOverload()
        {
            Assert.AreEqual(Settings.DEFAULT_AllowDeckOverload, settingsVM.AllowDeckOverload);
            settingsVM.AllowDeckOverload = !Settings.DEFAULT_AllowDeckOverload;
            Assert.AreEqual(!Settings.DEFAULT_AllowDeckOverload, settingsVM.AllowDeckOverload);

            //Check change is persisted across instances
            settingsVM = new SettingsViewModel(mockNav.Object);
            Assert.AreEqual(!Settings.DEFAULT_AllowDeckOverload, settingsVM.AllowDeckOverload);
        }

        [TestMethod]
        public void TestEnableRandomDeck()
        {
            Assert.AreEqual(Settings.DEFAULT_EnableRandomDeck, settingsVM.EnableRandomDeck);
            settingsVM.EnableRandomDeck = !Settings.DEFAULT_EnableRandomDeck;
            Assert.AreEqual(!Settings.DEFAULT_EnableRandomDeck, settingsVM.EnableRandomDeck);

            //Check change is persisted across instances
            settingsVM = new SettingsViewModel(mockNav.Object);
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
