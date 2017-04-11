using DragonFrontDb;
using DragonFrontDb.Enums;
using DragonFrontCompanion.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace DragonFrontCompanion.Controls
{
    public partial class CardControl : ContentView
    {
        public CardControl()
        {
            InitializeComponent();

            if (App.RuntimePlatform == App.Device.Windows || App.RuntimePlatform == App.Device.WinPhone)
            {
                TypeLabel.FontSize = 12;
                FactionLabel.FontSize = 12;
            }
        }
    }
}
