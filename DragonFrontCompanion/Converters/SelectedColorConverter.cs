using DragonFrontDb.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DragonFrontCompanion.Helpers
{
    public class SelectedColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value is bool && (bool)value) ||
                (value is int && (int)value == (int)parameter) ||
                ((value is Enum && parameter is int) && (int)value == (int)parameter) ||
                ((value is Enum && parameter is Enum) && ((Enum)value).Equals(parameter))) //enum == comparison wasn't working
            {
                return Color.FromHex("#E1CA35");
            }
            else if (parameter is Rarity)
            {
                return Colors.Transparent;
            }
            else
            {
                return Colors.White;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

	public class SelectedBorderColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if ((value is bool && (bool)value) ||
				(value is int && (int)value == (int)parameter) ||
                ((value is Enum && parameter is int) && (int)value == (int)parameter) ||
				((value is Enum && parameter is Enum) && ((Enum)value).Equals(parameter))) //enum == comparison wasn't working
			{
				return Color.FromHex("#E1CA35");
			}
			else if (parameter is Rarity)
			{
				return Colors.Transparent;
			}
			else
			{
				return Colors.Transparent;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
