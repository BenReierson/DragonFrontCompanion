using DragonFrontCompanion.Data;
using DragonFrontDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonFrontCompanion.Tests
{
    [TestClass]
    public class CardTests
    {
        CardsService _cardsService;

        [TestInitialize]
        public void Setup()
        {
            _cardsService = new CardsService();
        }

        [TestMethod]
        public async Task TestCardDataUpdateAvailable()
        {
            _cardsService.CardDataInfoUrl = "https://raw.githubusercontent.com/BenReierson/DragonFrontDb/v2_dataVersionTesting/Info.json";

            bool UpdateAvailableFired = false;
            bool DataUpdatedFired = false;

            _cardsService.DataUpdateAvailable += (o, e) => 
                UpdateAvailableFired = true;

            _cardsService.DataUpdated += (o, e) => 
                DataUpdatedFired = true;
            
            var latestInfo = await _cardsService.CheckForUpdatesAsync();

            Assert.IsTrue(latestInfo.CardDataVersion > Info.Current.CardDataVersion);
            Assert.AreEqual(new Version(2,0,1,0), latestInfo.CardDataVersion);
            Assert.IsTrue(UpdateAvailableFired);
            Assert.IsFalse(DataUpdatedFired);
        }

        [TestMethod]
        public async Task TestCardDataUpdate()
        {
            _cardsService.CardDataInfoUrl = "https://raw.githubusercontent.com/BenReierson/DragonFrontDb/v2_dataVersionTesting/Info.json";
            _cardsService.CardDataUpdateUrl = "https://raw.githubusercontent.com/BenReierson/DragonFrontDb/v2_dataVersionTesting/AllCards.json";

            Cards updatedCards = null;
            _cardsService.DataUpdated += (o, e) => updatedCards = e;

            var latestInfo = await _cardsService.CheckForUpdatesAsync();
            await _cardsService.UpdateCardDataAsync();

            Assert.IsTrue(latestInfo.CardDataVersion > Info.Current.CardDataVersion);
            Assert.AreEqual(new Version(2, 0, 1, 0), latestInfo.CardDataVersion);
            Assert.IsNotNull(updatedCards);
            Assert.AreEqual("Test001", updatedCards.All[0].ID);
        }
    }
}
