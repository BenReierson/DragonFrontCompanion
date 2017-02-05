﻿using DragonFrontDb.Enums;
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
            if (!(value is Traits[]) || ((Traits[])value).Length == 0) return "";

           	var traitsString = new StringBuilder("Traits: ");
            foreach (var trait in (Traits[])value)
            {
                traitsString.Append(trait.ToString().Trim('_'));
                traitsString.Append(", ");
            }

            var result = traitsString.ToString().Replace('_', ' ').Trim().Trim(',');
			if (Device.OS == TargetPlatform.iOS) result = "  " + result + "  ";

			return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
