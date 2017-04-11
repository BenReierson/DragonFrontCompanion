using SlideOverKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace DragonFrontCompanion.Controls
{
    public partial class FactionPicker : SlideMenuView
    {
        public FactionPicker()
        {
            InitializeComponent();

            this.HeightRequest = 500;
            this.IsFullScreen = true;
            this.MenuOrientations = MenuOrientation.TopToBottom;

            this.BackgroundColor = Color.Transparent;
            this.BackgroundViewColor = Color.Transparent;

            if (App.RuntimePlatform == App.Device.Android)
                this.HeightRequest += 50;
        }
    }
}
