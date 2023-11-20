using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DragonFrontCompanion.Helpers
{
    public class ManaFragmentImageConverter : IValueConverter
    {
        const string manaFragmentNameFormat = "iconmanafragment_{0}.png";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int) || value == null || (int)value == 0) return null;

            int count = (int)value;

            switch (count)
            {
                case 1:
                    return string.Format(manaFragmentNameFormat, "one");
                case 2:
                    return string.Format(manaFragmentNameFormat, "two");
                case 3:
                    return string.Format(manaFragmentNameFormat, "three");
                case 4:
                    return string.Format(manaFragmentNameFormat, "four");
                case 5:
                    return string.Format(manaFragmentNameFormat, "five");
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
