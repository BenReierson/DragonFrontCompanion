﻿using DragonFrontDb;
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
        private const string fullQualityExtension = ".jpg";
        private const string thumbnailExtension = "_1.jpg";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string id = null;

            if (value is Card) id = ((Card)value).ID;
            else id = value as string;

            return $"{imagePath}{id}{(parameter != null ? fullQualityExtension : thumbnailExtension)}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
    }

	public class CardImagePlaceholderConverter : IValueConverter
	{
		private const string CHAMP_IMAGE = "IconChamp_1.png";
		private const string SPELL_IMAGE = "IconSpell_1.png";
		private const string FORT_IMAGE = "IconFort_1.png";
		private const string UNIT_IMAGE = "IconUnit_1.png";
		private const string UNALIGNED_IMAGE = "IconUnaligned_1.png";

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Card)
			{
				switch (((Card)value).Type)
				{
					case DragonFrontDb.Enums.CardType.CHAMPION:
                        return CHAMP_IMAGE;
					case DragonFrontDb.Enums.CardType.SPELL:
                        return SPELL_IMAGE;
					case DragonFrontDb.Enums.CardType.FORT:
                        return FORT_IMAGE;
					case DragonFrontDb.Enums.CardType.UNIT:
                        return UNIT_IMAGE;
					default:
                        return UNALIGNED_IMAGE;
				}
			}
            return UNALIGNED_IMAGE;
		}

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
	}
}
