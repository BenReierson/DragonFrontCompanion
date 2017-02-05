using DragonFrontDb;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DragonFrontCompanion.Helpers
{
    public class CardImageConverter : IValueConverter
    {
        private const string imagePath = "Cards/";
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string id = null;

            if (value is Card) id = ((Card)value).ID;
            else id = value as string;

            //if (id != null && id != "")
            //{
            //    return imagePath + id + (parameter != null ? ".jpg" : "_100.jpg");
            //}
            if (value is Card)
            {
                switch (((Card)value).Type)
                {
                    case DragonFrontDb.Enums.CardType.CHAMPION:
                        return "IconChamp.png";
                    case DragonFrontDb.Enums.CardType.SPELL:
                        return "IconSpell.png";
                    case DragonFrontDb.Enums.CardType.FORT:
                        return "IconFort.png";
                    case DragonFrontDb.Enums.CardType.UNIT:
                        return "IconUnit.png";
                    default:
                        return "IconUnaligned.png";
                }
            }
            return "IconChamp.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
