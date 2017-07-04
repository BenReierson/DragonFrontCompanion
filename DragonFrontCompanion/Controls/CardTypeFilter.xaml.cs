using SlideOverKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace DragonFrontCompanion.Controls
{
    public partial class CardTypeFilter : SlideMenuView
    {
        public CardTypeFilter()
        {
            InitializeComponent();

            this.HeightRequest = 500;
            this.IsFullScreen = true;
            this.MenuOrientations = MenuOrientation.TopToBottom;

            this.BackgroundColor = Color.Transparent;
            this.BackgroundViewColor = Color.Transparent;

            if (App.RuntimePlatform == App.Device.Android)
                this.HeightRequest += 50;
            else if (App.RuntimePlatform == App.Device.iOS)
            {
                MainContent.Margin = new Thickness(10, 55, 10, 20);
                this.HeightRequest += 20;
            }
            else
            {
                this.HeightRequest += 100;
                this.LeftMargin = 20;
            }
        }
    }
}
