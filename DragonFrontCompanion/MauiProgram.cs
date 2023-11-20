using System.Reflection;
using CommunityToolkit.Maui;
using DragonFrontCompanion.Data;
using DragonFrontCompanion.Data.Services;
using DragonFrontCompanion.Helpers;
using DragonFrontCompanion.Ioc;
using FFImageLoading.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Handlers;

namespace DragonFrontCompanion;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
           .UseMauiApp<App>()
           .UseMauiCommunityToolkit()
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
            });

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