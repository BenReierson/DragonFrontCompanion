using DragonFrontCompanion.Data;
using DragonFrontDb;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DragonFrontCompanion.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {

        private INavigationService _navigationService;
        private ICardsService _cardsService;

        public SettingsViewModel(INavigationService navigationService, ICardsService cardsService)
        {
            _navigationService = navigationService;
            _cardsService = cardsService;
        }

        public async Task Initialize()
        {
            await CheckForUpdate();
        }

        private async Task CheckForUpdate()
        {
            var info = await _cardsService.CheckForUpdatesAsync();
            var activeVersion = Settings.ActiveCardDataVersion == null ? Info.Current.CardDataVersion : Version.Parse(Settings.ActiveCardDataVersion);
            CardDataUpdateAvailable = info.CardDataVersion > activeVersion;
            UpdateAvailableText = CardDataUpdateAvailable ? $"Newer card data v{info.CardDataVersion.ToString()} available" : "";
            ResetAvailable = activeVersion != Info.Current.CardDataVersion;
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

        public string ActiveCardDataVersion
        {
            get
            {
                var version = Settings.ActiveCardDataVersion != null ? Settings.ActiveCardDataVersion : Info.Current.CardDataVersion.ToString();
                return "Card Data v" + version;
            }
            set { Settings.ActiveCardDataVersion = value;  RaisePropertyChanged(); }
        }

        private bool _updateAvailable;
        public bool CardDataUpdateAvailable
        {
            get { return _updateAvailable; }
            set { _updateAvailable = value; RaisePropertyChanged(); }
        }

        private bool _resetAvailable = false;
        public bool ResetAvailable
        {
            get { return _resetAvailable; }
            set { Set(ref _resetAvailable, value); }
        }

        private string _updateText;
        public string UpdateAvailableText
        {
            get { return _updateText; }
            set { _updateText = value; RaisePropertyChanged(); }
        }



        #endregion

        private RelayCommand _resetCardData;

        /// <summary>
        /// Gets the ResetCardDataCommand.
        /// </summary>
        public RelayCommand ResetCardDataCommand
        {
            get
            {
                return _resetCardData
                    ?? (_resetCardData = new RelayCommand(
                    async () =>
                    {
                        await _cardsService.ResetCardDataAsync();
                        await CheckForUpdate();
                        RaisePropertyChanged(nameof(ActiveCardDataVersion));
                    }));
            }
        }

        private RelayCommand _updateCardData;

        /// <summary>
        /// Gets the UpdateCardDataCommand.
        /// </summary>
        public RelayCommand UpdateCardDataCommand
        {
            get
            {
                return _updateCardData
                    ?? (_updateCardData = new RelayCommand(
                    async () =>
                    {
                        await _cardsService.UpdateCardDataAsync();
                        await CheckForUpdate();
                        RaisePropertyChanged(nameof(ActiveCardDataVersion));
                    }));
            }
        }
    }
}
