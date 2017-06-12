using DragonFrontCompanion.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace DragonFrontCompanion.Views
{
    public partial class SettingsPage : ContentPage
    {

        public SettingsViewModel Vm
        {
            get
            {
                return (SettingsViewModel)BindingContext;
            }
        }

        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Vm.DataSourceVisible = false;
            Vm.Initialize();
        }

        private void DataSource_Unfocused(object sender, FocusEventArgs e)
        {
            Vm.CheckForUpdate();
        }
    }
}
