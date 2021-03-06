/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:DragonFrontCompanion"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using CommonServiceLocator;
using DragonFrontCompanion.Data;
using DragonFrontCompanion.Design;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;

namespace DragonFrontCompanion.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        #region PageKeys
        public const string MainPageKey = "MainPage";
        public const string DecksPageKey = "DecksPage";
        public const string DeckPageKey = "DeckPage";
        public const string CardsPageKey = "CardsPage";
        public const string AboutPageKey = "AboutPage";
        public const string SettingsPageKey = "SettingsPage";
        #endregion

        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            SimpleIoc.Default.Register<ICardsService, CardsService>();

            if (ViewModelBase.IsInDesignModeStatic)
            {
				SimpleIoc.Default.Register<INavigationService, NavigationService>();
                // Create design time view services and models
                SimpleIoc.Default.Register<IDeckService, DesignDeckService>();
            }
            else
            {
                LocalDeckService.AppVersion = App.VersionName;
                // Create run time view services and models
                SimpleIoc.Default.Register<IDeckService, LocalDeckService>();
            }

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<DecksViewModel>();
            SimpleIoc.Default.Register<DeckViewModel>();
            SimpleIoc.Default.Register<CardsViewModel>();
            SimpleIoc.Default.Register<AboutViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
        }

        public MainViewModel Main => SimpleIoc.Default.GetInstance<MainViewModel>();
        public DecksViewModel Decks => SimpleIoc.Default.GetInstance<DecksViewModel>();
        public DeckViewModel Deck => SimpleIoc.Default.GetInstance<DeckViewModel>();
        public CardsViewModel Cards => SimpleIoc.Default.GetInstance<CardsViewModel>();
        public AboutViewModel About => SimpleIoc.Default.GetInstance<AboutViewModel>();
        public SettingsViewModel Settings => SimpleIoc.Default.GetInstance<SettingsViewModel>();

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}