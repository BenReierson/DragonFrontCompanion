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
    public class RarityColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //turn rarity into color
            Rarity rarity;
            var parsed = Enum.TryParse<Rarity>(value.ToString(), out rarity);

            if (parsed)
            {
                switch (rarity)
                {
                    case Rarity.INVALID:
                    case Rarity.BASIC:
                        return Color.Black;
                    case Rarity.COMMON:
                        return Color.FromHex("C8C9C9");
                    case Rarity.RARE:
                        return Color.FromHex("2D4ABC");
                    case Rarity.EPIC:
                        return Color.FromHex("955DB9");
                    case Rarity.CHAMPION:
                        return Color.FromHex("C24121");
                }
            }
            return Color.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
