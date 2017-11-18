using DragonFrontCompanion.Data;
using DragonFrontDb;
using DragonFrontDb.Enums;
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
        void HandleFunc()
        {

        }

        private INavigationService _navigationService;
        private ICardsService _cardsService;
        private IDialogService _dialogService;
        private Info _latestInfo;

        public SettingsViewModel(INavigationService navigationService, ICardsService cardsService, IDialogService dialogService)
        {
            _navigationService = navigationService;
            _cardsService = cardsService;
            _dialogService = dialogService;

            _cardsService.DataUpdated += (o, e) => CheckForUpdate(false);
        }

        public async Task Initialize()
        {
            await CheckForUpdate();
        }

        public async Task CheckForUpdate(bool invalidateCache = true)
        {
            var activeVersion = Settings.ActiveCardDataVersion ?? Info.Current.CardDataVersion;
            ResetAvailable = activeVersion != Info.Current.CardDataVersion;
            RaisePropertyChanged(nameof(ActiveCardDataVersion));

            try
            {
                if (_latestInfo == null || invalidateCache) _latestInfo = await _cardsService.CheckForUpdatesAsync();
                CardDataUpdateAvailable = _latestInfo.CardDataVersion > activeVersion;
                UpdateAvailableText = CardDataUpdateAvailable ? $"Newer card data v{_latestInfo.CardDataVersion} available" : "";
                if (CardDataUpdateAvailable && _latestInfo.CardDataStatus == DragonFrontDb.Enums.DataStatus.PREVIEW) UpdateAvailableText += " - PREVIEW";
            }
            catch (Exception) { //TODO:Log
            }
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
                var version = Settings.ActiveCardDataVersion ?? Info.Current.CardDataVersion;
                var label = $"Cards v{version}";
                if (_latestInfo?.CardDataVersion == version) label += $" '{_latestInfo.CardDataName}'";
                else if (Info.Current.CardDataVersion == version) label += $" '{Info.Current.CardDataName}'";
                return label;
            }
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

        public bool EnableAutoUpdate
        {
            get { return Settings.EnableAutoUpdate; }
            set
            {
                Settings.EnableAutoUpdate = value;
                RaisePropertyChanged();
                if (value && CardDataUpdateAvailable && _latestInfo?.CardDataStatus == DataStatus.RELEASE)
                {
                    UpdateCardDataCommand.Execute(null);
                }
            }
        }

        private bool _dataSourceVisible = false;
        public bool DataSourceVisible
        {
            get { return _dataSourceVisible; }
            set { Set(ref _dataSourceVisible, value); }
        }

        public string ActiveDataSource
        {
            get { return _cardsService.ActiveDataSource; }
            set { _cardsService.ActiveDataSource = value; RaisePropertyChanged(); }
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
                        ResetAvailable = false;
                        await _cardsService.ResetCardDataAsync();
                        RaisePropertyChanged(nameof(ActiveDataSource));
                        await CheckForUpdate();
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
                        CardDataUpdateAvailable = false;
                        MessagingCenter.Send<string>("Updating Card Data", App.MESSAGES.SHOW_TOAST);
                        await _cardsService.UpdateCardDataAsync();
                        await CheckForUpdate(false);
                    }));
            }
        }

        private RelayCommand _showCardData;

        /// <summary>
        /// Gets the ShowCardDataSourceCommand.
        /// </summary>
        public RelayCommand ShowCardDataSourceCommand
        {
            get
            {
                return _showCardData
                    ?? (_showCardData = new RelayCommand(
                    () =>
                    {
                        DataSourceVisible = true;
                    }));
            }
        }

        private RelayCommand _showCardChangeHistory;
        public RelayCommand ShowCardChangeHistory =>
        _showCardChangeHistory ?? (_showCardChangeHistory = new RelayCommand(async () =>
        {
            if (_latestInfo != null)
            {
                var history = new StringBuilder();
                foreach (var item in _latestInfo.CardDataChangeLog)
                {
                    history.Append(item.Key);
                    history.Append(" - ");
                    history.Append(item.Value);
                    history.Append("\n");
                }
                await _dialogService.ShowMessage(history.ToString(), "Card Change History");
            }
        }));

    }
}
