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
                    return "IconRarityBasic.png";
                case Rarity.COMMON:
                    return "IconRarityCommon.png";
                case Rarity.RARE:
                    return "IconRarityRare.png";
                case Rarity.EPIC:
                    return "IconRarityEpic.png";
                case Rarity.CHAMPION:
                    return "IconRarityChampion.png";
                case Rarity.TOKEN:
                    return "IconToken.png";
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
