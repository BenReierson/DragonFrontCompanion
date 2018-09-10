using DragonFrontDb;
using DragonFrontDb.Enums;
using DragonFrontCompanion.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using FFImageLoading.Transformations;

namespace DragonFrontCompanion.Controls
{
    public partial class CardControl : ContentView
    {
        public CardControl()
        {
            InitializeComponent();

            if (App.RuntimePlatform == App.Device.UWP)
            {
                SetLabel.FontSize = 12;
            }
            else
            {//Rounded transformation is not working on UWP
                CardImage.Transformations.Add(new RoundedTransformation(60, 0, 0, 10, "#80000000"));
            }
        }
    }
}
