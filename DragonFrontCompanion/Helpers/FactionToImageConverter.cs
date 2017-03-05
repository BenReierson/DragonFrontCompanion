using DragonFrontDb.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DragonFrontCompanion.Helpers
{
    public class FactionToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var faction = (Faction)value;
            switch (faction)
            {
                case Faction.ECLIPSE:
                    return "IconEclipse.png";
                case Faction.SCALES:
                    return "IconScales.png";
                case Faction.STRIFE:
                    return "IconStrife.png";
                case Faction.THORNS:
                    return "IconThorns.png";
                case Faction.SILENCE:
                    return "IconSilence.png";
                case Faction.ESSENCE:
                    return "IconEssence.png";
                case Faction.DELIRIUM:
                    return "IconDelirium.png";
                default:
                    return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
