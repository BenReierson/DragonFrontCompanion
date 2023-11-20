using DragonFrontDb.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DragonFrontCompanion.Helpers
{
    public class ValueToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return false;
            if (value is 0) return false;
            if (string.IsNullOrEmpty(value.ToString())) return false;
            if (value is Array array) return array.Length > 0;
            if (value is CardType and (CardType.SPELL or CardType.INVALID)) return false;
            if (value is Race.NONE) return false;
            if (value is ICollection collection) return collection.Count > 0;

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
