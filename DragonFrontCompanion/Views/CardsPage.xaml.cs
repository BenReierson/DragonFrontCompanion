﻿
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
using DragonFrontDb.Enums;

namespace DragonFrontCompanion.Views
{
    public partial class CardsPage : MenuContainerPage
    {
        private Deck _deckToShow = null;
        private string _pendingSearch = null;
        private bool _initialized = false;

        public CardsViewModel Vm => (CardsViewModel)BindingContext;

        public CardsPage() : this(deck: null) { }

        public CardsPage(string searchText) : this(deck: null)
        {
            _pendingSearch = searchText;
        }

        public CardsPage(Deck deck = null)
        {
            InitializeComponent();

            _deckToShow = deck;

            if (App.RuntimePlatform == App.Device.Android &&
                CrossDeviceInfo.Current.VersionNumber.Major < 5)
            {SlideMenu = new CardTypeFilterLegacy();}
            else SlideMenu = new CardTypeFilter();
           
            this.SlideMenu.BindingContext = Vm;

            //Android uses toast messages instead of the label
            MessageLabel.IsVisible = App.RuntimePlatform != App.Device.Android;
        }
        

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            ((CardPopup)Resources["SelectedCardPopup"]).BindingContext = Vm;

            if (!_initialized)
            {
				_initialized = true;
				await Vm.InitializeAsync(_deckToShow, _pendingSearch);
                _deckToShow = null;
                _pendingSearch = null;
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
