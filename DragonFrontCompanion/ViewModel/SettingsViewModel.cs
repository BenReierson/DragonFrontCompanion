﻿using DragonFrontCompanion.Data;
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

        private INavigationService _navigationService;
        private ICardsService _cardsService;
        private Info _latestInfo;

        public SettingsViewModel(INavigationService navigationService, ICardsService cardsService)
        {
            _navigationService = navigationService;
            _cardsService = cardsService;

            _cardsService.DataUpdated += (o, e) => CheckForUpdate(false);
        }

        public async Task Initialize()
        {
            await CheckForUpdate();
        }

        private async Task CheckForUpdate(bool invalidateCache = true)
        {
            if (_latestInfo == null || invalidateCache) _latestInfo = await _cardsService.CheckForUpdatesAsync();
            var activeVersion = Settings.ActiveCardDataVersion ?? Info.Current.CardDataVersion;
            CardDataUpdateAvailable = _latestInfo.CardDataVersion > activeVersion;
            UpdateAvailableText = CardDataUpdateAvailable ? $"Newer card data v{_latestInfo.CardDataVersion.ToString()} available" : "";
            if (CardDataUpdateAvailable && _latestInfo.CardDataStatus == DragonFrontDb.Enums.DataStatus.PREVIEW) UpdateAvailableText += " - PREVIEW";
            ResetAvailable = activeVersion != Info.Current.CardDataVersion;
            RaisePropertyChanged(nameof(ActiveCardDataVersion));
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
                return "Card Data v" + version;
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
    }
}
