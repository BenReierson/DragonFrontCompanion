
using DragonFrontDb;
using Rg.Plugins.Popup.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DragonFrontCompanion.ViewModel;
using Xamarin.Forms;
using static DragonFrontCompanion.Deck;

namespace DragonFrontCompanion.Views
{
    public partial class DeckPage : ContentPage
    {

        public DeckViewModel Vm => (DeckViewModel)BindingContext;

        public DeckPage(Deck deck)
        {
            InitializeComponent();

            if (Device.OS == TargetPlatform.Windows || Device.OS == TargetPlatform.WinPhone)
            {
                Title = "Deck";
            }

            Vm.CurrentDeck = deck;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Vm.HasNavigated = false;

            ResizeCardLists();

            var popup = ((BindableObject)Resources["SelectedCardPopup"]).BindingContext = Vm;

            Vm.PropertyChanged += Vm_PropertyChanged;

            if (Vm.CurrentDeck.Count == 0 && 
                string.IsNullOrEmpty(Vm.CurrentDeck.FilePath) &&
                Vm.CurrentDeck.LastModified > DateTime.Now - TimeSpan.FromSeconds(2))
            {//this deck was just created, automatically start in edit mode
                Vm.ToggleEditDeckDetailsCommand.Execute(null);
            }

        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Vm.CurrentDeck))
            {//needed because change to deck is not being picked up
                DeckHeader.BindingContext = null;
                DeckHeader.BindingContext = Vm.CurrentDeck;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            var popup = ((BindableObject)Resources["SelectedCardPopup"]).BindingContext = null;

            Vm.SelectedCard = null;

            Vm.PropertyChanged -= Vm_PropertyChanged;

            if (Vm.EditMode) Vm.ToggleEditDeckDetailsCommand.Execute(null);
            else Vm.SaveDeckCommand.Execute(true);
        }

        //make grid big enough to prevent list scrolling, allowing the whole page to scroll
        private void ResizeCardLists()
        {
            var rowHeight = FactionList.RowHeight > UnalignedList.RowHeight ? FactionList.RowHeight : UnalignedList.RowHeight;
            var longestList = Vm.CurrentDeck.DistinctFaction.Count() > Vm.CurrentDeck.DistinctUnaligned.Count() ? Vm.CurrentDeck.DistinctFaction.Count() : Vm.CurrentDeck.DistinctUnaligned.Count();
            CardLists.HeightRequest = (rowHeight * longestList) + 40;
        }

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            FactionList.SelectedItem = null;
            UnalignedList.SelectedItem = null;

            SelectCardAsync(((Deck.CardGroup)e.Item).Card);
        }

        private void DeckHeader_ChampionTapped(object sender, ItemTappedEventArgs e)
        {
            if (Vm.EditMode)
            {//close edit mode
                Vm.ToggleEditDeckDetailsCommand.Execute(null);
            }
            else if (e.Item != null)
            {//Show champion popup
                SelectCardAsync(e.Item as Card);
            }
            else
            {//no champion, go to edit 
                Vm.EditDeckCardsCommand.Execute(null);
            }
        }

        private void DeckHeader_EditModeToggleRequest(object sender, EventArgs e)
        {
            Vm.ToggleEditDeckDetailsCommand.Execute(null);
        }

        private async Task SelectCardAsync(Card selectedCard)
        {
            if (selectedCard == null) return;

            if (Vm.SelectedCard != null && Vm.SelectedCard == selectedCard)
            {//force update, otherwise properychanged won't register
                Vm.SelectedCard = null;
                await Task.Delay(50);
            }

            Device.BeginInvokeOnMainThread(()=>Vm.SelectedCard = selectedCard);
        }

    }
}
