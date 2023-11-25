using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using DragonFrontCompanion.Data;
using DragonFrontCompanion.Helpers;
using DragonFrontCompanion.Ioc;
using DragonFrontCompanion.ViewModels;
using DragonFrontCompanion.Views;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
namespace DragonFrontCompanion;

public partial class App : Application
{
    public const string APP_NAME = "DF Companion";

    public const string AppDataScheme = "dfdeck";
    public const string AppDeckCodeHost = "code";

    public static string VersionName = "NA";

    public static string GetApplinkFromDeckCode(string deckCode)
        => $"{App.AppDataScheme}://{App.AppDeckCodeHost}/{deckCode}";

    IDialogService _dialogService;
    
    public bool IsActive { get; private set; }

    public App()
    {
        InitializeComponent();

        MainPage = DeviceInfo.Platform == DevicePlatform.iOS || DeviceInfo.Platform == DevicePlatform.Android
            ? InitializeMainPage() 
            : new ContentPage();//placeholder to speed startup, it will be swapped out in OnStart()

    }

    private Page InitializeMainPage()
        => new NavigationPage(new MainPage())
        {
            BarBackgroundColor = Color.FromArgb("#E1CA35"),
            BarTextColor = Color.FromArgb("#123338")
        };

    private async Task InitializeAppCenter()
    {
        AppCenter.Start("ios=6f75c0fb-c19f-49a3-b0d1-e7c3fdf49b44;" +
                        "android=e03dd204-6d05-4ea4-9513-a12385944120;" +
                        "uwp=6013a5e1-18f0-4e45-95b8-bb7ce1411d6c;" ,
                        typeof(Analytics), typeof(Crashes));
        
        await Analytics.SetEnabledAsync(true);
    }

    protected override async void OnStart()
    {
        base.OnStart();
        
        IsActive = true;

        if (DeviceInfo.Platform != DevicePlatform.MacCatalyst)
            await InitializeAppCenter();

        var mainPage = MainPage is NavigationPage ? MainPage : InitializeMainPage();
        
        var dialog = SimpleIoc.Default.GetInstance<IDialogService>() as DialogService;
        dialog?.Initialize(mainPage);
        _dialogService = dialog;
        
        MainPage = mainPage;
    }
    
    protected override void OnSleep()
    {
        base.OnSleep();
        IsActive = false;
    }

    protected override void OnResume()
    {
        base.OnResume();
        IsActive = true;
    }

    protected override async void OnAppLinkRequestReceived(Uri uri)
    {
        base.OnAppLinkRequestReceived(uri);

        try
        {
            var deckService = SimpleIoc.Default.GetInstance<IDeckService>();
            var navService = SimpleIoc.Default.GetInstance<INavigationService>();

            if (deckService is null || navService is null) return;

            _=Toast.Make("Opening Deck...", ToastDuration.Long).Show();

            if (uri.Host == AppDeckCodeHost && uri.Segments?.LastOrDefault() is { } deckCode)
            {//process path as deck code
                await OpenDeckCodeInApp(deckCode);
            }
            else
            {
                //open the file
                var deck = await deckService?.OpenDeckFileAsync(uri.LocalPath);

                if (deck != null)
                {
                    await navService.Push<DeckViewModel>(vm => vm.Initialize(deck));
                }
                else _dialogService?.ShowError("The data may be invalid or corrupt.", "Failed to open deck", "OK");
            }
        }
        catch (Exception e)
        {
            _dialogService?.ShowError(e.Message, "Failed to open deck", "OK");
        }
    }

    private async Task OpenDeckCodeInApp(string deckCode)
    {
        var deckService = SimpleIoc.Default.GetInstance<IDeckService>();
        var navService = SimpleIoc.Default.GetInstance<INavigationService>();

        if (deckService is null || navService is null) return;

        _=Toast.Make("Opening Deck...", ToastDuration.Long).Show();

        //open the file
        var deck = deckService?.DeserializeDeckString(deckCode);

        if (deck != null)
            await navService.Push<DeckViewModel>(vm => vm.Initialize(deck));
        else
            _=Toast.Make("Failed to open deck. The data may be invalid or corrupt.", ToastDuration.Long).Show();
    }

#if WINDOWS
    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);

        const int newWidth = 600;
        const int newHeight = 800;

        window.Width = newWidth;
        window.Height = newHeight;

        return window;
    }
#endif
}
