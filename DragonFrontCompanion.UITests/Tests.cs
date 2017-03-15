using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace DragonFrontCompanion.UITests
{
    [TestFixture(Platform.Android)]
    [TestFixture(Platform.iOS)]
    public class Tests
    {
        IApp app;
        Platform platform;

        public Tests(Platform platform)
        {
            this.platform = platform;
        }

        [SetUp]
        public void BeforeEachTest()
        {
            app = AppInitializer.StartApp(platform);
        }

        [Test]
        public void AppLaunches()
        {
            app.Screenshot("First screen.");
        }

        [Test]
        public void TestCardsPage()
        {
            app.Tap(x => x.Marked("CardsPage"));
            app.Screenshot("All Cards");

            app.ScrollDown();
            app.ScrollDown();
            app.Screenshot("After scrolling down");

            app.Tap(x => x.Class("ActionMenuItemView").Index(1));
            app.Screenshot("Filters");

            app.Tap(x => x.Marked("FilterUnaligned"));
            app.Screenshot("Filtered by Unaligned");

            app.Tap(x => x.Class("ActionMenuItemView").Index(1));
            app.Screenshot("Unaligned Cards");

            app.Tap(x => x.Class("ActionMenuItemView").Index(0));
            app.Screenshot("Reset filters");

            app.Tap(x => x.Class("EntryEditText"));
            app.EnterText(x => x.Class("EntryEditText"), "Big");
            app.EnterText(x => x.Class("EntryEditText"), " Berth");
            app.Screenshot("Searched for Big Berth");

            app.Tap(x => x.Text("Big Bertha"));
            app.WaitForElement(x => x.Text("Giant?"));
            app.Screenshot("Opened Card detail");

           // app.Tap(x => x.Marked("TraitsButton"));
            //app.Screenshot("Opened Card Traits");

            //app.Tap(x => x.Class("ScrollViewRenderer"));
            app.Tap(x => x.Class("ScrollViewRenderer"));

            app.Screenshot("Closed Card detail");

        }

        [Test]
        public void TestDecksPageAndDeckEdit()
        {
            app.Tap(x => x.Marked("DecksPage"));
            app.Screenshot("Decks Page");

            app.Tap(x => x.Marked("New Deck"));
            app.Screenshot("New Deck Faction Select");

            app.Tap(x => x.Text("ECLIPSE"));
            app.Screenshot("New Eclipse Deck Edit");

            app.ClearText(x => x.Class("EntryEditText").Text("New ECLIPSE Deck"));
            app.EnterText(x => x.Class("EntryEditText"), "Test Cloud Deck");
            app.Tap(x => x.Marked("Done"));
            app.Screenshot("New Eclipse Deck Saved");

            app.Tap(x => x.Marked("Edit Deck"));
            app.Screenshot("Deck Card Edit");

            app.Tap(x => x.Text("+"));
            app.Screenshot("Champion added");

            app.Back();
            app.Screenshot("Deck detail with champion");

            app.Tap(x => x.Marked("Undo"));
            app.Screenshot("Deck Undo Prompt");

            app.Tap(x => x.Id("button1"));
            app.Screenshot("Deck Undo - Champion Removed");

            app.Back();
            app.Tap(x => x.Marked("DeckContextMenu"));
            app.Screenshot("Deck Context Menu");

            app.Tap(x => x.Id("text1"));
            app.Screenshot("Deck Deleted");

        }
        

    }
}

