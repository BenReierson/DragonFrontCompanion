
using DragonFrontDb;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DragonFrontCompanion.ViewModel;
using Xamarin.Forms;

namespace DragonFrontCompanion.Views
{
    public partial class MainPage : ContentPage
    {
        public MainViewModel Vm
        {
            get
            {
                return (MainViewModel)BindingContext;
            }
        }

		public MainPage()
		{
			InitializeComponent();

			if (Device.RuntimePlatform == Device.iOS)
			{//Add padding to ios button
				this.SizeChanged += (o, e) =>
				{
					NewCardsButton.WidthRequest = NewCardsButton.Width + 20;
				};
			}
		}

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await Vm.InitializeAsync();
        }

    }
}
