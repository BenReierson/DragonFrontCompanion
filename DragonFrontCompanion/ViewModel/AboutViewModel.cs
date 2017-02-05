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


        private string _hvs = "Dragon Front is a trademark or registered trademark of High Voltage Software, Inc. in the U.S. and/or other countries. Card images Copyright © 2015-2016, High Voltage Software, Inc. All Rights Reserved. All other trademarks listed are the property of their respective owners.";
        public string HvsText
        {
            get { return _hvs; }
            set { Set(ref _hvs, value); }
        }
        #endregion
    }
}
