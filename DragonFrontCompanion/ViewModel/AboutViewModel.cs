using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonFrontCompanion.ViewModel
{
    public class AboutViewModel : ViewModelBase
    {
        private INavigationService _navigationService;

        public AboutViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            AppName = App.APP_NAME;
            Version = "v" + App.VersionName;

            License = "Copyright ©2016 " + AppName + " Team.\nAll Rights Reserved.";
            HvsText = HvsText += $"\n\n{AppName} is not affiliated with, endorsed, sponsored, or specifically approved by High Voltage Software, Inc. {AppName} may use the trademarks and other intellectual property of High Voltage Software, Inc., which is permitted under specific material use policy agreed upon with High Voltage Software, Inc.For more information about High Voltage Software or any of the HVS trademarks or other intellectual property, please visit their website at www.high-voltage.com.";
        }

        #region Properties

        private string _appName = "";
        public string AppName
        {
            get { return _appName; }
            set { Set(ref _appName, value); }
        }

        private string _version = "";
        public string Version
        {
            get { return _version; }
            set { Set(ref _version, value); }
        }


        private string _license = "";
        public string License
        {
            get { return _license; }
            set { Set(ref _license, value); }
        }


        private string _hvs = "©2016 High Voltage Software, Inc. High Voltage Software, the High Voltage Software logo, Dragon Frontand the Dragon Front logo are either registered trademarks or trademarks of High Voltage Software, Inc.";
        public string HvsText
        {
            get { return _hvs; }
            set { Set(ref _hvs, value); }
        }
        #endregion
    }
}
