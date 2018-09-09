using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using DragonFrontCompanion.Data;
using Microsoft.Toolkit.Uwp.Notifications;
using GalaSoft.MvvmLight.Ioc;
using Windows.UI.Notifications;

namespace DragonFrontCompanion.UWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private bool _launched = false;
        private Deck _deckToShare;

        public ToastNotification Toast { get; private set; }
        public ToastNotifier Toaster { get; private set; } = ToastNotificationManager.CreateToastNotifier();

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            if (_launched) return;
            else _launched = true;
            
            InitializeApp(e);

            Frame rootFrame = Window.Current.Content as Frame;

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        private void InitializeApp(IActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                List<Assembly> assembliesToInclude = new List<Assembly>();
                assembliesToInclude.Add(typeof(Rg.Plugins.Popup.Windows.Renderers.PopupPageRenderer).GetTypeInfo().Assembly);
                assembliesToInclude.Add(typeof(SlideOverKit.MenuContainerPage).GetTypeInfo().Assembly);
                assembliesToInclude.Add(typeof(SlideOverKit.UWP.MenuContainerPageUWPRenderer).GetTypeInfo().Assembly);
                assembliesToInclude.Add(typeof(FFImageLoading.Forms.WinUWP.CachedImageRenderer).GetTypeInfo().Assembly);
               // assembliesToInclude.Add(typeof(FFImageLoading.FFImage).GetTypeInfo().Assembly);
                assembliesToInclude.Add(typeof(FFImageLoading.Transformations.TransformationBase).GetTypeInfo().Assembly);
                assembliesToInclude.Add(typeof(Plugin.Settings.CrossSettings).GetTypeInfo().Assembly);
                assembliesToInclude.Add(typeof(Plugin.Permissions.CrossPermissions).GetTypeInfo().Assembly);
                assembliesToInclude.Add(typeof(Plugin.DeviceInfo.CrossDeviceInfo).GetTypeInfo().Assembly);
                assembliesToInclude.Add(typeof(PCLStorage.FileSystem).GetTypeInfo().Assembly);
                assembliesToInclude.Add(typeof(Newtonsoft.Json.JsonConvert).GetTypeInfo().Assembly);
                assembliesToInclude.Add(typeof(Xamarin.Forms.Forms).GetTypeInfo().Assembly);

                Rg.Plugins.Popup.Popup.Init();

                FFImageLoading.Forms.Platform.CachedImageRenderer.Init();

                Xamarin.Forms.Forms.Init(args, assembliesToInclude);

                global::Xamarin.Forms.MessagingCenter.Subscribe<Deck>(this, DragonFrontCompanion.App.MESSAGES.SHARE_DECK, ShareDeck, null);
                global::Xamarin.Forms.MessagingCenter.Subscribe<Deck>(this, DragonFrontCompanion.App.MESSAGES.EXPORT_DECK, ExportDeck, null);
                global::Xamarin.Forms.MessagingCenter.Subscribe<string>(this, DragonFrontCompanion.App.MESSAGES.SHOW_TOAST, (message)=>ShowToast(message));

                Windows.ApplicationModel.DataTransfer.DataTransferManager.GetForCurrentView().DataRequested += App_DataRequested;

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }
        }

        private async void ExportDeck(Deck deck)
        {
            _deckToShare = deck;
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;

            savePicker.FileTypeChoices.Add("Dragon Front Deck", new List<string>() { ".dfd" });
            savePicker.SuggestedFileName = deck.Name;

            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                if (string.IsNullOrEmpty(_deckToShare.FilePath))
                {
                    _deckToShare = await SimpleIoc.Default.GetInstance<IDeckService>().SaveDeckAsync(_deckToShare);
                    if (_deckToShare == null) return;
                }

                Windows.Storage.CachedFileManager.DeferUpdates(file);
                var deckText = await FileIO.ReadTextAsync(await StorageFile.GetFileFromPathAsync(_deckToShare.FilePath));
                await Windows.Storage.FileIO.WriteTextAsync(file, deckText);

                Windows.Storage.Provider.FileUpdateStatus status = await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
            }
        }
        
        private void ShareDeck(Deck deck)
        {
            _deckToShare = deck;
            Windows.ApplicationModel.DataTransfer.DataTransferManager.ShowShareUI();
        }

        private async void App_DataRequested(Windows.ApplicationModel.DataTransfer.DataTransferManager sender, Windows.ApplicationModel.DataTransfer.DataRequestedEventArgs args)
        {
            if (_deckToShare != null)
            {
                args.Request.Data.Properties.Title = "Sharing Dragon Front Deck";
                args.Request.Data.Properties.Description = _deckToShare.Name;

                DataRequestDeferral deferral = args.Request.GetDeferral();

                try
                {
                    if (string.IsNullOrEmpty(_deckToShare.FilePath))
                    {
                        _deckToShare = await SimpleIoc.Default.GetInstance<IDeckService>().SaveDeckAsync(_deckToShare);
                        if (_deckToShare == null) return;
                    }

                    var file = await StorageFile.GetFileFromPathAsync(_deckToShare.FilePath);
                    var list = new List<StorageFile>();
                    list.Add(file);
                    args.Request.Data.SetStorageItems(list);
                }
                catch (Exception) { }
                finally
                {
                    deferral.Complete();
                }
            }
        }

        private void ShowToast(string message)
        {
            ToastContent content = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            //new AdaptiveText()
                            //{
                            //    Text = "Dragon Front Companion",
                            //    HintMaxLines = 1
                            //},
                            new AdaptiveText()
                            {
                                Text = message,
                                HintMaxLines = 2
                            }
                        },
                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = @"Assets\FileLogo.png",
                            HintCrop = ToastGenericAppLogoCrop.Default
                        }
                    }
                }
            };

            if (Toast != null) Toaster.Hide(Toast);
            Toast = new ToastNotification(content.GetXml()) { ExpirationTime = DateTime.Now + TimeSpan.FromSeconds(5) };
            Toaster.Show(Toast);
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        protected async override void OnFileActivated(FileActivatedEventArgs args)
        {
            if (!_launched)
            {
                InitializeApp(args);
                Frame rootFrame = Window.Current.Content as Frame;
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(MainPage), null);
                }
                Window.Current.Activate();
            }
            _launched = true;

            if (args.Files.Count > 0)
            {
                var file = args.Files[0] as StorageFile;
                var deckText = await FileIO.ReadTextAsync(file);

                global::Xamarin.Forms.MessagingCenter.Send<object, string>(this, DragonFrontCompanion.App.MESSAGES.OPEN_DECK_DATA, deckText);
            }

            base.OnFileActivated(args);
        }

    }
}
