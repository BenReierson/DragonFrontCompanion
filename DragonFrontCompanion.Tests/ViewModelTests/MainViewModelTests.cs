using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DragonFrontCompanion.ViewModel;
using GalaSoft.MvvmLight.Views;
using Moq;
using DragonFrontCompanion;

namespace DragonFrontCompanion.Tests
{
    [TestClass]
    public class MainViewModelTests
    {
        MainViewModel mainVM;
        Mock<INavigationService> mockNav;

        [TestInitialize]
        public void MainVMSetup()
        {
            mockNav = new Mock<INavigationService>();
            mainVM = new MainViewModel(mockNav.Object);
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
