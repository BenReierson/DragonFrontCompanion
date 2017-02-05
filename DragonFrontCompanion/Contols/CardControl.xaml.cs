using DragonFrontDb;
using DragonFrontDb.Enums;
using DragonFrontCompanion.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace DragonFrontCompanion.Contols
{
    public partial class CardControl : ContentView
    {
        public CardControl()
        {
            InitializeComponent();

            if (Device.OS == TargetPlatform.Windows || Device.OS == TargetPlatform.WinPhone)
            {
                TypeLabel.FontSize = 12;
                FactionLabel.FontSize = 12;
            }
        }
    }
}
