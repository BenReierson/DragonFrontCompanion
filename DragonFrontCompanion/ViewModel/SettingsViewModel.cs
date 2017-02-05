using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonFrontCompanion.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {

        private INavigationService _navigationService;

        public SettingsViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            
        }

        #region Properties
        
        public bool AllowDeckOverload
        {
            get { return Settings.AllowDeckOverload; }
            set { Settings.AllowDeckOverload = value; RaisePropertyChanged(); }
        }

        public bool EnableRandomDeck
        {
            get { return Settings.EnableRandomDeck; }
            set { Settings.EnableRandomDeck = value; RaisePropertyChanged(); }
        }
        #endregion

    }
}
