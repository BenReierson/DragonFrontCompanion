using DragonFrontDb.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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
                    return "iconeclipse.png";
                case Faction.SCALES:
                    return "iconscales.png";
                case Faction.STRIFE:
                    return "iconstrife.png";
                case Faction.THORNS:
                    return "iconthorns.png";
                case Faction.SILENCE:
                    return "iconsilence.png";
                case Faction.ESSENCE:
                    return "iconessence.png";
                case Faction.DELIRIUM:
                    return "icondelirium.png";
                case Faction.AEGIS:
                    return "iconaegis.png";
                case (Faction)10:
                    return "iconninth.png";
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
