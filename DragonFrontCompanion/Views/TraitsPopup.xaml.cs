using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace DragonFrontCompanion.Views
{
    public partial class TraitsPopup : PopupPage
    {
        public TraitsPopup()
        {
            InitializeComponent();

            TraitsList.ItemTapped += (o, e) =>
			{
                TraitsList.SelectedItem = null;
                ClosePopup();
            };
        }

        protected override bool OnBackButtonPressed()
        {
            ClosePopup();

            return false;
        }

        protected override bool OnBackgroundClicked()
        {
            ClosePopup();

            return false;
        }

        private async void ClosePopup()
        {
            await Navigation.RemovePopupPageAsync(this, true);
        }

        private void CloseButton_Clicked(object sender, EventArgs e)
        {
            ClosePopup();
        }
        
    }
}
