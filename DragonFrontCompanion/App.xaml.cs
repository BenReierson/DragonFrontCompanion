using DragonFrontDb.Enums;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DragonFrontCompanion.Data;
using DragonFrontCompanion.Helpers;
using DragonFrontCompanion.ViewModel;
using DragonFrontCompanion.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace DragonFrontCompanion
{
    public partial class App : Application
    {
        public const string APP_NAME = "Dragon Front Companion";

        public class MESSAGES
        {
            public const string OPEN_DECK_FILE = "DeckFile";
            public const string OPEN_DECK_DATA = "DeckData";
            public const string NEW_DECK_SAVED = "NewDeckSaved";
            public const string SHARE_DECK = "Share";
            public const string EXPORT_DECK = "Export";
            public const string SHOW_TOAST = "Toast";
        }

        private static ViewModelLocator _locator;
        public static ViewModelLocator Locator
        {
            get
            {
                return _locator ?? (_locator = new ViewModelLocator());
            }
        }

        public static string VersionName = "NA";

        public Button FeedbackButton { get; private set; }
        
        private NavigationService _navService = null;
        private DialogService _dialog = null;
        public App(Button feedbackButton = null)
        {
            InitializeComponent();

            FeedbackButton = feedbackButton;

            if (SimpleIoc.Default.ContainsCreated<INavigationService>())
            {
                _navService = SimpleIoc.Default.GetInstance<INavigationService>() as NavigationService;
                _dialog = SimpleIoc.Default.GetInstance<IDialogService>() as DialogService;
            }
            else
            {
                // First time initialization
                _navService = new NavigationService();
                _navService.Configure(ViewModelLocator.MainPageKey, typeof(MainPage));
                _navService.Configure(ViewModelLocator.DecksPageKey, typeof(DecksPage));
                _navService.Configure(ViewModelLocator.DeckPageKey, typeof(DeckPage));
                _navService.Configure(ViewModelLocator.CardsPageKey, typeof(CardsPage));
                _navService.Configure(ViewModelLocator.AboutPageKey, typeof(AboutPage));
                _navService.Configure(ViewModelLocator.SettingsPageKey, typeof(SettingsPage));

                SimpleIoc.Default.Register<INavigationService>(() => _navService);
                _dialog = new DialogService();
                SimpleIoc.Default.Register<IDialogService>(() => _dialog);

                MessagingCenter.Subscribe<object, string>(this, MESSAGES.OPEN_DECK_FILE, OpenDeckFileAsync);
                MessagingCenter.Subscribe<object, string>(this, MESSAGES.OPEN_DECK_DATA, OpenDeckDataAsync);
            }

            var mainPage = new NavigationPage(new MainPage()) { BarBackgroundColor = Color.FromHex("#E1CA35"), BarTextColor = Color.FromHex("#123338") };
            _navService.Initialize(mainPage);
            _dialog.Initialize(mainPage);
            MainPage = mainPage;
        }

        private async void OpenDeckDataAsync(object o, string data)
        {
            try
            {
                var deck = await new LocalDeckService().OpenDeckDataAsync(data);
                if (deck != null)
                {
                    _navService?.NavigateTo(ViewModelLocator.DeckPageKey, deck);
                }
                else _dialog.ShowError("The data may be invalid or corrupt.", "Failed to open deck", "OK", null);
            }
            catch (Exception e)
            {
                _dialog.ShowError(e.Message, "Failed to open deck", "OK", null);
            }
        }

        private async void OpenDeckFileAsync(object o, string file)
        {
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
            if (status != PermissionStatus.Granted)
            {
                var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Storage });
                status = results[Permission.Storage];
            }
            if (status == PermissionStatus.Granted)
            {
                try
                {
                    //open the file
                    var deck = await new LocalDeckService().OpenDeckFileAsync(file);
               
                    if (deck != null)
                    {
                        _navService.NavigateTo(ViewModelLocator.DeckPageKey, deck);
                    }
                    else _dialog.ShowError("The data may be invalid or corrupt.", "Failed to open deck", "OK", null);
                }
                catch (Exception e)
                {
                    _dialog.ShowError(e.Message, "Failed to open deck", "OK", null);
                }
            }
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
