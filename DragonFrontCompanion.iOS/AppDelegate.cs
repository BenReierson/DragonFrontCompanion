using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using UIKit;
using DragonFrontCompanion;
using Newtonsoft.Json;
using CoreGraphics;
using System.Drawing;
using Xamarin.Forms.Platform.iOS;
using System.IO;

namespace DragonFrontCompanion.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
		App _app;
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            var version = NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"];

            global::Xamarin.Forms.Forms.Init();
            
            FFImageLoading.Forms.Touch.CachedImageRenderer.Init();
            SlideOverKit.iOS.SlideOverKit.Init();

            App.VersionName = version.ToString();
			_app = new App();
            LoadApplication(_app);

            global::Xamarin.Forms.MessagingCenter.Subscribe<Deck>(this, App.MESSAGES.SHARE_DECK, ShareDeck, null);

            return base.FinishedLaunching(app, options);
        }

		public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
		{
			global::Xamarin.Forms.MessagingCenter.Send<object, string>(this, App.MESSAGES.OPEN_DECK_FILE, url.Path);
			return true;
		}

		public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
		{
			global::Xamarin.Forms.MessagingCenter.Send<object, string>(this, App.MESSAGES.OPEN_DECK_FILE, url.Path);
			return true;
		}

        UIDocumentInteractionController uidic;
        UIViewController viewCtrl;
		bool result;
        private void ShareDeck(Deck deck)
        {

            UIViewController topCtrl = UIApplication.SharedApplication.KeyWindow.RootViewController;

			//save deck in temp location with name
			var tempdir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			var file = Path.Combine(tempdir, deck.Name + ".dfd");
			var deckJson = JsonConvert.SerializeObject(deck, Formatting.Indented);
			File.WriteAllText(file, deckJson);

            var navCtrl = topCtrl.ChildViewControllers.OfType<NavigationRenderer>().FirstOrDefault();
            if (navCtrl != null)
            {
                viewCtrl = navCtrl.TopViewController;

                var allButtons = viewCtrl.NavigationItem.RightBarButtonItems;
                var button = allButtons[0];

                uidic = UIDocumentInteractionController.FromUrl(new NSUrl(file, true));
                uidic.Name = "Share Deck";
                uidic.Uti = "com.benreierson.deck";

                result = uidic.PresentOptionsMenu(button, true);
            }
        }
     
    }
}
