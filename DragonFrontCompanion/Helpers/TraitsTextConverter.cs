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
    public class TraitsTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is List<string>) || !((List<string>)value).Any()) return "";

            var traitsString = new StringBuilder("Traits: ");
            foreach (var trait in (List<string>)value)
            {
                traitsString.Append(trait.Trim('_'));
                traitsString.Append(", ");
            }

            var result = traitsString.ToString().Replace('_', ' ').Trim().Trim(',');
            if (App.RuntimePlatform == App.Device.iOS) result = "  " + result + "  ";

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
