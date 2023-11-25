using System.Reflection;
using CommunityToolkit.Maui;
using DragonFrontCompanion.Data;
using DragonFrontCompanion.Data.Services;
using DragonFrontCompanion.Helpers;
using DragonFrontCompanion.Ioc;
using FFImageLoading.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.LifecycleEvents;
using ZXing.Net.Maui.Controls;

namespace DragonFrontCompanion;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
           .UseMauiApp<App>()
           .UseMauiCommunityToolkit()
           .UseBarcodeReader()
           .UseFFImageLoading()
           .RegisterAppServices()
           .RegisterViewModels()
           .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
           .ConfigureMauiHandlers(handlers =>
            {
                bool fixing = false;
                ToolbarHandler.Mapper.ModifyMapping("ToolbarItems", (handler, toolbar, action) =>
                {
                    if (Application.Current.MainPage is null)
                    {
                        action.Invoke(handler, toolbar);
                        return;
                    }

                    if (fixing)
                    {
                        action?.Invoke(handler, toolbar);
                    }
                    else
                    {
                        fixing = true;
                        //var bar = (Application.Current.MainPage as IToolbarElement).Toolbar;
                        toolbar
                            .GetType()
                            .GetMethod("ApplyChanges", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                            .Invoke(toolbar, new[] { Application.Current.MainPage });
                        fixing = false;
                    }
                });
            })
           .ConfigureEssentials(essentials =>
            {
                essentials.UseVersionTracking();
            })
           .ConfigureLifecycleEvents(lifecycle =>
            {
#if IOS
                lifecycle.AddiOS(ios =>
                {
                    ios.OpenUrl((app, url, opion) =>
                    {
                        HandleAppLinkString(url.ToString());
                        return true;
                    });

                    ios.FinishedLaunching((app, data)
                            => HandleAppLink(app.UserActivity));

                    ios.ContinueUserActivity((app, userActivity, handler)
                        => HandleAppLink(userActivity));
                });

#endif
            });

#if IOS || MACCATALYST
		static bool HandleAppLink(Foundation.NSUserActivity? userActivity)
		{
			if (userActivity is not null && userActivity.ActivityType == Foundation.NSUserActivityType.BrowsingWeb && userActivity.WebPageUrl is not null)
			{
				HandleAppLinkString(userActivity.WebPageUrl.ToString());
				return true;
			}
			return false;
		}
#endif

        static void HandleAppLinkString(string url)
        {
            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
            {
                Application.Current?.SendOnAppLinkRequestReceived(uri);
            }
        }

        App.VersionName = VersionTracking.Default.CurrentVersion;

#if DEBUG
        builder.Logging.AddDebug();
#endif


        return builder.Build();
    }

    public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder mauiAppBuilder)
    {
        SimpleIoc.Default.Register<INavigationService, NavigationService>();
        NavigationService.RegisterViewModels();
        
        return mauiAppBuilder;
    }
    
    public static MauiAppBuilder RegisterAppServices(this MauiAppBuilder mauiAppBuilder)
    {
        SimpleIoc.Default.Register<ICardsService, CardsService>();
        SimpleIoc.Default.Register<IDeckService, LocalDeckService>();
        SimpleIoc.Default.Register<IDialogService, DialogService>();
        
        return mauiAppBuilder;
    }
}