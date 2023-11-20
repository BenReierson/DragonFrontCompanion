using DragonFrontDb.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DragonFrontCompanion.Helpers
{
    public class RarityImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Rarity)) return null;

            Rarity rarity = (Rarity)value;

            switch (rarity)
            {
                case Rarity.INVALID:
                    return null;
                case Rarity.BASIC:
                    return "iconraritybasic.png";
                case Rarity.COMMON:
                    return "iconraritycommon.png";
                case Rarity.RARE:
                    return "iconrarityrare.png";
                case Rarity.EPIC:
                    return "iconrarityepic.png";
                case Rarity.CHAMPION:
                    return "iconraritychampion.png";
                case Rarity.TOKEN:
                    return "icontoken.png";
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
