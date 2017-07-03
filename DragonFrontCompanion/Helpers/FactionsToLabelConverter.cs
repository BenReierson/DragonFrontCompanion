using System;
using System.Globalization;
using Xamarin.Forms;
using DragonFrontDb.Enums;

namespace DragonFrontCompanion.Helpers
{
    public class FactionsToLabelConverter : IValueConverter
    {
		private static int _totalFactionCount = Enum.GetValues(typeof(Faction)).Length - 2;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var factions = value as Faction[];
            if (factions == null) return null;

            if (factions.Length == _totalFactionCount) return Faction.UNALIGNED.ToString();
            else
            {
                var factionsLabel = "";
                foreach (var f in factions)
                {
					factionsLabel += $"{f}\n";
                }
                return factionsLabel;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
