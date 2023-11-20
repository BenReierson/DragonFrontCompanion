using DragonFrontCompanion.Helpers;
using DragonFrontCompanion.Ioc;
using DragonFrontCompanion.Views;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
namespace DragonFrontCompanion;

public partial class App : Application
{
    public const string APP_NAME = "Dragon Front Companion";
    public static string VersionName = "NA";
    
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
}
