using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DragonFrontCompanion.Data.Services;
using DragonFrontCompanion.Helpers;
using DragonFrontCompanion.Views;
using DragonFrontDb;
using DragonFrontDb.Enums;
namespace DragonFrontCompanion.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    private const string NEW_CARDS_DEFAULT_TEXT = "Aegis Set";

    private ICardsService _cardsService;
    private static bool _firstLoad = true;
    private bool _cardDataReset;
    private bool _newCardsToShow = false;

    public MainViewModel(INavigationService navigationService, ICardsService cardsService, IDialogService dialogService) : base(navigationService, dialogService)
    {
        _cardsService = cardsService;

        VersionDisplay = "v" + App.VersionName;
        Title = App.APP_NAME;

        cardsService.DataUpdateAvailable += CardsService_DataUpdateAvailable;
        cardsService.DataUpdated += CardsService_DataUpdated;
    }

    public override async Task OnAppearing()
    {
        await base.OnAppearing();

        if (_firstLoad && Settings.EnableAutoUpdate)
        {
            try
            {
                NewCardsEnabled = false;
                _firstLoad = false;
                IsBusy = true;
                await Task.Delay(500);
                var toast = Toast.Make("Checking for updates...", ToastDuration.Long);
                toast.Show();
                await _cardsService.CheckForUpdatesAsync();
                var newCards = await CheckForNewCardsAsync();
                NewCardsEnabled = newCards;
                toast.Dismiss();
            }
            catch (Exception) { }
            finally
            {
                IsBusy = false;
            }
        }
    }

    #region Properties
    
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _versionDisplay = "";
    [ObservableProperty] private bool _newCardsEnabled = false;
    [ObservableProperty] private string _newCardsText = NEW_CARDS_DEFAULT_TEXT;

    #endregion

    #region Commands
    [RelayCommand]
    private Task NavigateToAbout()
        => _navigationService.Push<AboutViewModel>();

    [RelayCommand]
    private async Task NavigateToCards()
    {
        try
        {
            IsBusy = true;
            await Task.Delay(10);
            await _navigationService.Push<CardsViewModel>(vm => vm.Initialize());
        }
        catch (Exception ex)
        {
            _=_dialogService.ShowError(ex, "Error", "Ok");
        }
        IsBusy = false;
    }
    
    [RelayCommand]
    private async Task NavigateToNewCards(string searchText)
    {
        try
        {
            IsBusy = true;
            await Task.Delay(10);
            await _navigationService.Push<CardsViewModel>(vm=>vm.Initialize(searchText:_newCardsToShow ? "NEXT" : searchText));
        }
        catch (Exception ex)
        {
            _=_dialogService.ShowError(ex, "Error", "Ok");
        }
        IsBusy = false;
    }
    
    [RelayCommand]
    private Task NavigateToSettings()
        => _navigationService.Push<SettingsViewModel>();
    
    [RelayCommand]
    private async Task NavigateToDecks()
    {
        try
        {
            IsBusy = true;
            await Task.Delay(10);
            await _navigationService.Push<DecksViewModel>();
        }
        catch (Exception ex)
        {
            _=_dialogService.ShowError(ex, "Error", "Ok");
        }
        IsBusy = false;
    }

    #endregion

    public async Task<bool> CheckForNewCardsAsync()
    {
        var cards = await _cardsService.GetAllCardsAsync();
        _newCardsToShow = cards.Any(c => c.CardSet == CardSet.NEXT);
        if (_newCardsToShow) NewCardsText = "New Cards!";
        else NewCardsText = NEW_CARDS_DEFAULT_TEXT;
        
        return _newCardsToShow;
    }
    
    private void CardsService_DataUpdated(object sender, Cards e)
    {
        _cardDataReset = Settings.ActiveCardDataVersion == Info.Current.CardDataVersion;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            Toast.Make("Loaded Card Data v" + (Settings.ActiveCardDataVersion ?? Info.Current.CardDataVersion)).Show();
            await CheckForNewCardsAsync();
        });
    }

    private void CardsService_DataUpdateAvailable(object sender, Info e)
    {
        if (_cardDataReset || e.CardDataStatus == DataStatus.UNKNOWN) return;
        else if (!Settings.EnableAutoUpdate && e.CardDataStatus == DataStatus.RELEASE)
        {
            if (e.CardDataVersion > Settings.HighestNotifiedCardDataVersion)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Toast.Make("New Card Data available in settings.").Show();
                    Settings.HighestNotifiedCardDataVersion = e.CardDataVersion;
                });
            }
        }
        else if (e.CardDataStatus == DataStatus.PREVIEW)
        {
            if (e.CardDataVersion > Settings.HighestNotifiedCardDataVersion)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Toast.Make("New Preview Card Data available in settings.").Show();
                    Settings.HighestNotifiedCardDataVersion = e.CardDataVersion;
                });
            }
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Toast.Make("Updating Card Data").Show();
            });
            Settings.HighestNotifiedCardDataVersion = e.CardDataVersion;
            _cardsService.UpdateCardDataAsync();
        }
    }
}