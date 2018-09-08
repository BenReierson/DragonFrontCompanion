using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content;
using DragonFrontCompanion;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using Android;
using Java.Util.Concurrent.Atomic;
using System.Collections.Generic;
using System.Reflection;
using Xamarin.Forms;
using System.Net.Http;
using Java.IO;

namespace DragonFrontCompanion.Droid
{

    [Activity(Label = "DF Companion", 
        Icon = "@drawable/icon", Theme = "@style/splashscreen", 
        MainLauncher = true, 
        LaunchMode = LaunchMode.SingleInstance,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    [IntentFilter(new string[] { Intent.ActionView },
        Categories = new string[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "file",
        DataHost = "*",
        DataMimeType = "*/*",
        DataPathPattern = ".*.dfd")]
    [IntentFilter(new string[] { Intent.ActionView },
        Categories = new string[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "file",
        DataHost = "*",
        DataMimeType = "*/*",
        DataPathPattern = ".*\\.dfd")]
    [IntentFilter(new string[] { Intent.ActionView },
        Categories = new string[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "file",
        DataHost = "*",
        DataMimeType = "*/*",
        DataPathPattern = ".*\\\\.dfd")]
    [IntentFilter(new string[] { Intent.ActionView },
        Categories = new string[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "file",
        DataHost = "*",
        DataMimeType = "*/*",
        DataPathPattern = ".*\\\\..*\\\\.dfd")]
    [IntentFilter(new string[] { Intent.ActionView },
        Categories = new string[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "file",
        DataHost = "*",
        DataMimeType = "*/*",
        DataPathPattern = ".*\\\\..*\\\\..*\\\\.dfd")]
    [IntentFilter(new string[] { Intent.ActionView },
        Categories = new string[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "file",
        DataHost = "*",
        DataMimeType = "*/*",
        DataPathPattern = ".*\\\\..*\\\\..*\\\\..*\\\\.dfd")]
    [IntentFilter(new string[] { Intent.ActionView },
        Categories = new string[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "file",
        DataHost = "*",
        DataMimeType = "*/*",
        DataPathPattern = ".*\\\\..*\\\\..*\\\\..*\\\\..*\\\\.dfd")]
    [IntentFilter(new string[] { Intent.ActionView },
        Categories = new string[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "content",
        DataHost = "*",
        DataMimeType = "application/octet-stream",
        DataPathPattern = ".*\\\\.dfd")]
    [IntentFilter(new string[] { Intent.ActionView },
        Categories = new string[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "content",
        DataHost = "*",
        DataMimeType = "application/dfd",
        DataPathPattern = ".*\\\\.dfd")]
    [IntentFilter(new string[] { Intent.ActionSend },
        Categories = new string[] { Intent.CategoryDefault},
        DataMimeType = "*/dfd")]
    [IntentFilter(new string[] { Intent.ActionSend },
        Categories = new string[] { Intent.CategoryDefault },
        DataMimeType = "*/*")]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {

        static Intent _handledIntent;

        protected override void OnCreate(Bundle bundle)
        {
            base.SetTheme(Resource.Style.MainTheme);

            base.OnCreate(bundle);
            
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            global::Xamarin.Forms.Forms.Init(this, bundle);
            FFImageLoading.Forms.Droid.CachedImageRenderer.Init(false);

            Context context = global::Xamarin.Forms.Forms.Context;
            var version = context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionName;

            MethodInfo dynMethod = typeof(MessagingCenter).GetMethod("ClearSubscribers", BindingFlags.NonPublic | BindingFlags.Static);
            dynMethod.Invoke(null, null);

            App.VersionName = version;
            App.RuntimePlatform = Device.RuntimePlatform;
            LoadApplication(new App());

            if (Intent != null && !Intent.GetBooleanExtra("handled", false) && Intent != _handledIntent)
            {
                Intent.PutExtra("handled", true);
                _handledIntent = Intent;

                if (Intent.DataString != null) OpenDeckFileInApp(Intent.Data.Path);
                else if (Intent.Action == Intent.ActionSend)
                {
                    var data = Intent.GetParcelableExtra(Intent.ExtraStream);
                    var fileStream = new System.IO.StreamReader(ContentResolver.OpenInputStream((Android.Net.Uri)data));
                    var filetext = fileStream.ReadToEnd();//TODO:Do this async
                    OpenDeckDataInApp(filetext);
                }
            }

            global::Xamarin.Forms.MessagingCenter.Subscribe<Deck>(this, App.MESSAGES.SHARE_DECK, ShareDeck, null);
            global::Xamarin.Forms.MessagingCenter.Subscribe<string>(this, App.MESSAGES.SHOW_TOAST, (message) => 
                                                            {Toast.MakeText(this.ApplicationContext, message, ToastLength.Short).Show();});

        }


        protected override async void OnNewIntent(Intent intent)
        {
            if (intent != null && !intent.GetBooleanExtra("handled", false))
            {
                if (intent.Action == Intent.ActionSend)
                {
                    intent.PutExtra("handled", true);
                    var data = intent.GetParcelableExtra(Intent.ExtraStream);
                    if (data == null)
                    { //Open from url
                        data = intent.GetStringExtra(Intent.ExtraText);
                        using (var client = new HttpClient())
                        {
                            var remotedata = await client.GetAsync(data.ToString());
                            if (remotedata != null && remotedata.IsSuccessStatusCode) OpenDeckDataInApp(await remotedata.Content.ReadAsStringAsync());
                        }
                    }
                    else
                    {
                        var fileStream = new System.IO.StreamReader(ContentResolver.OpenInputStream((Android.Net.Uri)data));
                        var filetext = fileStream.ReadToEnd();
                        OpenDeckDataInApp(filetext);
                    }

                }
                else if (intent.Action == Intent.ActionView)
                {
                    intent.PutExtra("handled", true);
                    if (intent.Data.Scheme == "content")
                    {
                        var stream = ContentResolver.OpenInputStream(intent.Data);
                        var reader = new BufferedReader(new InputStreamReader(stream));
                        var deckData = new StringBuilder();
                        while (reader.Ready()) deckData.Append(await reader.ReadLineAsync());
                        OpenDeckDataInApp(deckData.ToString());
                    }
                    else OpenDeckFileInApp(intent.Data.Path);
                }
            }

        }

        private Tuple<string, DateTime> _lastOpened = new Tuple<string, DateTime>("", DateTime.MinValue);

        private void OpenDeckFileInApp(string filePath)
        {
            if (_lastOpened?.Item1 == filePath && _lastOpened?.Item2 > DateTime.Now - TimeSpan.FromSeconds(2)) return; //don't process the same file twice in rapid succession

            global::Xamarin.Forms.MessagingCenter.Send<object, string>(this, App.MESSAGES.OPEN_DECK_FILE, filePath);
            _lastOpened = new Tuple<string, DateTime>(filePath, DateTime.Now);
        }

        private void OpenDeckDataInApp(string deckData)
        {
            if (_lastOpened?.Item1 == deckData && _lastOpened?.Item2 > DateTime.Now - TimeSpan.FromSeconds(2)) return; //don't process the same data twice in rapid succession

            global::Xamarin.Forms.MessagingCenter.Send<object, string>(this, App.MESSAGES.OPEN_DECK_DATA, deckData);
            _lastOpened = new Tuple<string, DateTime>(deckData, DateTime.Now);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }


        private void ShareDeck(Deck deck)
        {
            var intentChooser = GetChooserExcludingCurrentApp(this.ApplicationContext, deck);

            if (intentChooser != null)
                StartActivityForResult(intentChooser, 1000);
        }

        private static Intent GetChooserExcludingCurrentApp(Context ctx, Deck deck)
        {
            var targetedShareIntents = new List<Intent>();
            Intent share = new Intent(Intent.ActionSend);
            share.SetType("*/*");

            var deckJson = JsonConvert.SerializeObject(deck, Formatting.Indented);
            var file = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads + Java.IO.File.Separator + deck.Name + ".dfd");
            using (var os = new System.IO.FileStream(file.AbsolutePath, System.IO.FileMode.Create))
            {
                var bytes = Encoding.ASCII.GetBytes(deckJson);
                os.Write(bytes, 0, bytes.Length);
            }

            var resInfo = ctx.PackageManager.QueryIntentActivities(createShareIntent(file), 0);
            if (resInfo.Count > 0)
            {
                var addedPackages = new List<string>();
                foreach (var info in resInfo)
                {
                    var targetedShare = createShareIntent(file);

                    if (!info.ActivityInfo.PackageName.Equals(ctx.PackageName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (!addedPackages.Contains(info.ActivityInfo.PackageName))
                        {
                            targetedShare.SetPackage(info.ActivityInfo.PackageName);
                            targetedShareIntents.Add(targetedShare);
                            addedPackages.Add(info.ActivityInfo.PackageName);
                        }
                    }
                }

                if (targetedShareIntents.Count > 0)
                {
                    var firstIntent = targetedShareIntents[0];
                    targetedShareIntents.Remove(firstIntent);
                    Intent chooserIntent = Intent.CreateChooser(firstIntent, "Select app to share");
                    chooserIntent.PutExtra(Intent.ExtraInitialIntents, targetedShareIntents.ToArray());
                    return chooserIntent;
                }
            }
            return null;
        }

        private static Intent createShareIntent(Java.IO.File file)
        {
            Intent share = new Intent(Intent.ActionSend);
            share.SetType("*/*");
            share.PutExtra(Intent.ExtraStream, Android.Net.Uri.FromFile(file));

            return share;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
        }
    }
}

