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
    public class MainViewModelTests
    {
        Cards cardsDb;
        MainViewModel mainVM;
        Mock<INavigationService> mockNav;
        Mock<ICardsService> mockCardsService;

        [TestInitialize]
        public void MainVMSetup()
        {
            cardsDb = new Cards();
            mockCardsService = new Mock<ICardsService>();
            mockCardsService.Setup(c => c.GetAllCardsAsync()).Returns(async () => cardsDb.All);

            mockNav = new Mock<INavigationService>();
            mainVM = new MainViewModel(mockNav.Object, mockCardsService.Object);
        }

        [TestMethod]
        public void TestMainVMConstructor()
        {
            Assert.IsFalse(string.IsNullOrEmpty(App.VersionName));
            Assert.AreEqual(App.APP_NAME, mainVM.Title);
            Assert.IsTrue(mainVM.VersionDisplay.Contains(App.VersionName), "App version is not being displayed");
        }

        [TestMethod]
        public void TestAboutNav()
        {
            mainVM.NavigateToAboutCommand.Execute(null);

            mockNav.Verify(n => n.NavigateTo(ViewModelLocator.AboutPageKey));
        }

        [TestMethod]
        public void TestCardBrowseNav()
        {
            mainVM.NavigateToCardsCommand.Execute(null);

            mockNav.Verify(n => n.NavigateTo(ViewModelLocator.CardsPageKey));
        }

        [TestMethod]
        public void TestDeckBuilderNav()
        {
            mainVM.NavigateToDecksCommand.Execute(null);

            mockNav.Verify(n => n.NavigateTo(ViewModelLocator.DecksPageKey));
        }

        [TestMethod]
        public void TestSettingsNav()
        {
            mainVM.NavigateToSettingsCommand.Execute(null);

            mockNav.Verify(n => n.NavigateTo(ViewModelLocator.SettingsPageKey));
        }
    }
}
