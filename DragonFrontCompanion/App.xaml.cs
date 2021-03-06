﻿using DragonFrontDb.Enums;
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
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System.Runtime.CompilerServices;
using System.Diagnostics;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace DragonFrontCompanion
{
    public partial class App : Application
    {
        public const string APP_NAME = "Dragon Front Companion";

        public class Device
        {
            public const string Android = "Android";
            public const string iOS = "iOS";
            public const string UWP = "UWP";
            public const string Test = "Test";
        }

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

        public static string RuntimePlatform { get; set; } = Device.Test;

        public bool IsActive { get; set; }
        private NavigationService _navService = null;
        private DialogService _dialog = null;
        public App()
        {
            InitializeComponent();

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

        public static void LogError(Exception ex, [CallerMemberName]string sourceMethod = "")
        {
            try
            {
                if (!AppCenter.Configured) return;

                var properties = new Dictionary<string, string> { { "Method", sourceMethod }, { "Message", ex.Message }, };

                Analytics.TrackEvent("ERROR_SILENT", properties);
                Crashes.TrackError(ex, properties);

                Debug.WriteLine("ERROR_SILENT" + string.Join(";", properties));
            }
            catch (Exception) { }
        }

        private async void OpenDeckDataAsync(object o, string data)
        {
            try
            {
                var deckservice = SimpleIoc.Default.GetInstance<IDeckService>();
                var deck = await deckservice.OpenDeckDataAsync(data);
                if (deck != null)
                {
                    _navService?.NavigateTo(ViewModelLocator.DeckPageKey, deck);
                }
                else
                {
                    Version deckVersion;
                    var parsedDeckVersion = Version.TryParse(deckservice.GetDeckVersionFromDeckJson(data), out deckVersion);
                    Version CurrenVersion;
                    var parsedCurrentVersion = Version.TryParse(VersionName, out CurrenVersion);
                    if (parsedDeckVersion && parsedCurrentVersion && deckVersion > CurrenVersion)
                    {
                        _dialog.ShowError($"Failed to open deck made with app version {deckVersion}. You have version {CurrenVersion}. You may need to update the app.", "Failed to open deck", "OK", null);
                    }
                    else _dialog.ShowError("The data may be invalid or corrupt.", "Failed to open deck", "OK", null);
                }
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
                    var deckservice = SimpleIoc.Default.GetInstance<IDeckService>();
                    var deck = await deckservice.OpenDeckFileAsync(file);

                    if (deck != null)
                    {
                        _navService.NavigateTo(ViewModelLocator.DeckPageKey, deck);
                    }
                    else _dialog.ShowError("The data may be from a newer version of the app or corrupt.", "Failed to open deck", "OK", null);
                }
                catch (Exception e)
                {
                    _dialog.ShowError(e.Message, "Failed to open deck", "OK", null);
                }
            }
        }

        protected override void OnStart()
        {
            IsActive = true;

            // Handle when your app starts
            AppCenter.Start("ios=6f75c0fb-c19f-49a3-b0d1-e7c3fdf49b44;" +
                            "android=e03dd204-6d05-4ea4-9513-a12385944120;" +
                            "uwp=6013a5e1-18f0-4e45-95b8-bb7ce1411d6c;" ,
                            typeof(Analytics), typeof(Crashes));

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
}
