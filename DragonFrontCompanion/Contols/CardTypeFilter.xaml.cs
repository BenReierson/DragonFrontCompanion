using SlideOverKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace DragonFrontCompanion.Contols
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

            if (Device.OS == TargetPlatform.Android)
                this.HeightRequest += 50;
            else if (Device.OS == TargetPlatform.iOS)
                MainContent.Margin = new Thickness(10, 55, 10, 20);
            else
            {
                this.HeightRequest += 100;
                this.LeftMargin = 20;
            }
        }
    }
}
