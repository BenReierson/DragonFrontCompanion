using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DragonFrontCompanion.Helpers;
namespace DragonFrontCompanion.ViewModels;

public partial class AboutViewModel : BaseViewModel
{
    public AboutViewModel(INavigationService navigationService, IDialogService dialogService) : base(navigationService, dialogService)
    {
        AppName = App.APP_NAME;
        Version = "v" + App.VersionName;

        License = "Copyright ©2023 " + AppName + " Team.\nAll Rights Reserved.";
        HvsText = HvsText += $"\n\n{AppName} is not affiliated with, endorsed, sponsored, or specifically approved by High Voltage Software, Inc. {AppName} may use the trademarks and other intellectual property of High Voltage Software, Inc., which is permitted under specific material use policy agreed upon with High Voltage Software, Inc.For more information about High Voltage Software or any of the HVS trademarks or other intellectual property, please visit their website at www.high-voltage.com.";
    }
    
    
    #region Properties

    [ObservableProperty] private string _appName = "";
    [ObservableProperty] private string _version = "";
    [ObservableProperty] private string _license = "";
    [ObservableProperty] private string _hvsText = "©2023 High Voltage Software, Inc. High Voltage Software, the High Voltage Software logo, Dragon Front® and the Dragon Front® logo are either registered trademarks or trademarks of High Voltage Software, Inc.";

    #endregion
}