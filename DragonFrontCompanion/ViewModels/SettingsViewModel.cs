using System.Text;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DragonFrontCompanion.Data.Services;
using DragonFrontCompanion.Helpers;
using DragonFrontDb;
using DragonFrontDb.Enums;
namespace DragonFrontCompanion.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly ICardsService _cardsService;
    private Info _latestInfo;
    
    public SettingsViewModel(INavigationService navigationService, IDialogService dialogService, ICardsService cardsService) : base(navigationService, dialogService)
    {
        Title = "Settings";
        _cardsService = cardsService;
        _cardsService.DataUpdated += (o, e) => CheckForUpdate(false);
    }

    public override async Task OnAppearing()
    {
        await base.OnAppearing();
        await CheckForUpdate();
    }
    
    public async Task CheckForUpdate(bool invalidateCache = true)
    {
        var activeVersion = Settings.ActiveCardDataVersion ?? Info.Current.CardDataVersion;
        ResetAvailable = activeVersion != Info.Current.CardDataVersion;
        OnPropertyChanged(nameof(ActiveCardDataVersion));

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

    [ObservableProperty] private bool _cardDataUpdateAvailable;
    [ObservableProperty] private bool _resetAvailable;
    [ObservableProperty] private string _updateAvailableText;
    [ObservableProperty] private bool _dataSourceVisible;
    
    public string ActiveDataSource
    {
        get { return _cardsService.ActiveDataSource; }
        set { _cardsService.ActiveDataSource = value; OnPropertyChanged(); }
    }
    
    public bool EnableAutoUpdate
    {
        get { return Settings.EnableAutoUpdate; }
        set
        {
            Settings.EnableAutoUpdate = value;
            OnPropertyChanged();
            if (value && CardDataUpdateAvailable && _latestInfo?.CardDataStatus == DataStatus.RELEASE)
            {
                UpdateCardDataCommand.Execute(null);
            }
        }
    }
    
    public bool AllowDeckOverload
    {
        get { return Settings.AllowDeckOverload; }
        set { Settings.AllowDeckOverload = value; OnPropertyChanged(); }
    }

    public bool EnableRandomDeck
    {
        get { return Settings.EnableRandomDeck; }
        set { Settings.EnableRandomDeck = value; OnPropertyChanged(); }
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
    #endregion
    
    #region Commands

    [RelayCommand]
    private async Task ResetCardData()
    {
        ResetAvailable = false;
        await _cardsService.ResetCardDataAsync();
        OnPropertyChanged(nameof(ActiveDataSource));
        await CheckForUpdate();
    }

    [RelayCommand]
    private async Task UpdateCardData()
    {
        CardDataUpdateAvailable = false;
        Toast.Make("Updating Card Data").Show();
        await _cardsService.UpdateCardDataAsync();
        await CheckForUpdate(false);
    }

    [RelayCommand]
    private void ShowCardDataSource()
    {
        DataSourceVisible = true;
    }

    [RelayCommand]
    private async Task ShowCardChangeHistory()
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
    }

    #endregion
}