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
    public class ValueToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return false;
            if (value is int && (int)value == 0) return false;
            if (string.IsNullOrEmpty(value.ToString())) return false;
            if (value is Array && ((Array)value).Length == 0) return false;
            if (value is CardType && ((CardType)value == CardType.SPELL || (CardType)value == CardType.INVALID)) return false;
            if (value is Race && (Race)value == Race.NONE) return false;

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
