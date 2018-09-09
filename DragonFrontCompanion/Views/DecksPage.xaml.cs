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
    public partial class DecksPage : MenuContainerPage
    {
        private static bool _firstLoad = true;

        /// <summary>
        /// Gets the view's ViewModel.
        /// </summary>
        public DecksViewModel Vm
        {
            get
            {
                return (DecksViewModel)BindingContext;
            }
        }

        public DecksPage()
        {
            InitializeComponent();
            this.SlideMenu = new FactionPicker();

            //disable selection
            DecksList.ItemSelected += (sender, e) => {
                ((ListView)sender).SelectedItem = null;
            };
        }

        private void NewDeckClicked(object sender, EventArgs e )
        {
            if (SlideMenu.IsShown) this.HideMenu();
            else this.ShowMenu();
        }

        private void DeleteDeck_Clicked(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            Vm.DeleteDeckCommand.Execute(mi.CommandParameter);
        }

        private void Champion_Tapped(object sender, ItemTappedEventArgs e)
        {
            Vm.OpenDeckCommand.Execute(e.Group);
        }

        private void Deck_Edit(object sender, EventArgs e)
        {
            Vm.OpenDeckCommand.Execute(((View)sender).BindingContext);
        }

        private async void Deck_ContextMenu(object sender, ItemTappedEventArgs e)
        {
            var result = await DisplayActionSheet(((Deck)e.Group).Name, "Cancel", null, new string[] {"Delete", "Duplicate", "Share" });
            if (result == "Delete")
            {
                Vm.DeleteDeckCommand.Execute(e.Group);
            }
            else if (result == "Duplicate")
            {
                Vm.DuplicateDeckCommand.Execute(e.Group);
            }
            else if (result == "Share")
            {
                Vm.ShareDeckCommand.Execute(e.Group);
            }
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

        protected override void OnAppearing()
        {
            Vm.HasNavigated = false;

            if (SlideMenu.IsShown)
            {
                try { this.HideMenu(); } catch (Exception) { }
            }
            Vm.RefreshDecksCommand.Execute(_firstLoad);
            _firstLoad = false;

            base.OnAppearing();
        }
    }
}
