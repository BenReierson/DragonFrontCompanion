using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using DragonFrontCompanion.Data;
using DragonFrontCompanion.Ioc;
using DragonFrontCompanion.ViewModels;
using Microsoft.AppCenter.Crashes;

namespace DragonFrontCompanion;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleInstance, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
[IntentFilter(new[] { Intent.ActionView},
        AutoVerify = true,
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable},
        DataScheme = App.AppDataScheme)]
[IntentFilter(new[]{ Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataMimeType = "application/octet-stream",
        DataPathSuffix = ".dfd")]
[IntentFilter(new string[] { Intent.ActionSend },
        Categories = new string[] { Intent.CategoryDefault, Intent.CategoryBrowsable},
        DataMimeType = "application/octet-stream",
        DataPathSuffix = ".dfd")]
public class MainActivity : MauiAppCompatActivity
{
    
    private Tuple<string, DateTime> _lastOpened = new Tuple<string, DateTime>("", DateTime.MinValue);

    protected override void OnCreate(Bundle savedInstanceState)
    {
        // Need this handling here because OnNewIntent seems to not get called
        // when the app isn't already running in the background
        CheckForLink(Intent);

        base.OnCreate(savedInstanceState);
    }

    protected override void OnNewIntent(Intent intent)
    {
        // Need this handling here because OnCreate doesn't get called when the
        // app is already running in the background, but OnNewIntent does get called
        CheckForLink(intent);
    }

    private async void CheckForLink(Intent intent)
    {
        try
        {
            if (intent != null && !intent.GetBooleanExtra("handled", false)
                && (intent.Action == Intent.ActionView || intent.Action == Intent.ActionSend))
            {
                intent.PutExtra("handled", true);

                if (intent.Data.Host == App.AppDeckCodeHost)
                {//process path as deck code
                    await Task.Delay(1000);//wait for app to finish launching
                    Microsoft.Maui.Controls.Application.Current.SendOnAppLinkRequestReceived(new Uri(intent.Data.ToString()));
                }
                else
                {//assume incoming data is a deck file
                    var data = intent.GetParcelableExtra(Intent.ExtraStream);
                    var fileStream =
                        new StreamReader(ContentResolver.OpenInputStream((data as Android.Net.Uri) ?? intent.Data));
                    var filetext = fileStream.ReadToEnd();
                    await OpenDeckDataInApp(filetext);
                }
            }
        }
        catch (Exception ex)
        {
            Crashes.TrackError(ex);
            Toast.MakeText(this.ApplicationContext, "Failed to open deck.", ToastLength.Long).Show();
        }
    }

    private async Task OpenDeckDataInApp(string deckData)
    {
        if (_lastOpened?.Item1 == deckData && _lastOpened?.Item2 > DateTime.Now - TimeSpan.FromSeconds(2)) return; //don't process the same data twice in rapid succession

        var deckService = SimpleIoc.Default.GetInstance<IDeckService>();
        var navService = SimpleIoc.Default.GetInstance<INavigationService>();

        if (deckService is null || navService is null) return;

        Toast.MakeText(this.ApplicationContext, "Opening Deck...", ToastLength.Long).Show();

        //open the file
        var deck = await deckService?.OpenDeckDataAsync(deckData);

        if (deck != null)
        {
            await navService.Push<DeckViewModel>(vm => vm.Initialize(deck));
        }
        else Toast.MakeText(this.ApplicationContext, "Failed to open deck. The data may be invalid or corrupt.", ToastLength.Long).Show();

        _lastOpened = new Tuple<string, DateTime>(deckData, DateTime.Now);
    }
}