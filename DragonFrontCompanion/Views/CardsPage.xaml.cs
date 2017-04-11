
using DragonFrontDb;
using Plugin.DeviceInfo;
using Rg.Plugins.Popup.Extensions;
using SlideOverKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DragonFrontCompanion.Controls;
using DragonFrontCompanion.ViewModel;
using Xamarin.Forms;

namespace DragonFrontCompanion.Views
{
    public partial class CardsPage : MenuContainerPage
    {
        public CardsViewModel Vm => (CardsViewModel)BindingContext;

        public CardsPage() : this(deck: null) { }

        public CardsPage(Deck deck = null)
        {
            InitializeComponent();

            if (App.RuntimePlatform == App.Device.Android &&
                CrossDeviceInfo.Current.VersionNumber.Major < 5)
            {SlideMenu = new CardTypeFilterLegacy();}
            else SlideMenu = new CardTypeFilter();


            Vm.CurrentDeck = deck;
            this.SlideMenu.BindingContext = Vm;

            //Android uses toast messages instead of the label
            MessageLabel.IsVisible = App.RuntimePlatform != App.Device.Android;
        }
        

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ((CardPopup)Resources["SelectedCardPopup"]).BindingContext = Vm;

            if (App.RuntimePlatform == App.Device.iOS)
            {//bug in xamarin forms 2.3.3 preventing color from sticking the first time
                MessageLabel.BackgroundColor = Color.White;
                DeckStatusLabel.BackgroundColor = Color.White;

                MessageLabel.BackgroundColor = Color.Black;
                DeckStatusLabel.BackgroundColor = Color.Black;
            }
        }

        private void FiltersButton_Clicked(object sender, EventArgs e)
        {
            if (SlideMenu.IsShown) this.HideMenu();
            else this.ShowMenu();
        }

        private void ResetFilters_Clicked(object sender, EventArgs e)
        {
            this.HideMenu();
        }

        private void FindSimilar_Clicked(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            Vm.FindSimilarCommand.Execute(mi.CommandParameter);
        }

        protected override bool OnBackButtonPressed()
        {
            if (SlideMenu.IsShown)
            {
                try { this.HideMenu(); } catch (Exception) { }
                return true;
            }
            else return base.OnBackButtonPressed();
            
        }

        private async void CardsList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            CardsList.SelectedItem = null;

            if (Vm.SelectedCard == e.Item)
            {//force update, otherwise properychanged won't register
                Vm.SelectedCard = null;
                await Task.Delay(50); 
            }

            Vm.SelectedCard = e.Item as Card;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            ((CardPopup)Resources["SelectedCardPopup"]).BindingContext = null;
            Vm.SelectedCard = null;

            if (Vm.CurrentDeck != null)
            {
                //save the deck
                Vm.SaveDeckCommand.Execute(null);
            }
        }
    }
}
