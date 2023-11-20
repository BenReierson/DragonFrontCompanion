using DragonFrontDb;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DragonFrontCompanion.Helpers
{
    public class CardImageConverter : IValueConverter
    {
		private const string imagePath = "";//"cards/";
        private const string fullQualityExtension = "_c.jpg";
        private const string thumbnailExtension = "_thumb.jpg";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string id = null;

			if (value is null) return null;

            if (value is Card) id = ((Card)value).ID;
            else id = value as string;

            var image = $"{imagePath}{id?.ToLower()}{(parameter != null ? fullQualityExtension : thumbnailExtension)}";

			return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
    }

	public class CardImagePlaceholderConverter : IValueConverter
	{
		private const string CHAMP_IMAGE = "iconchamp_thumb.png";
		private const string SPELL_IMAGE = "iconspell_thumb.png";
		private const string FORT_IMAGE = "iconfort_thumb.png";
		private const string UNIT_IMAGE = "iconunit_thumb.png";
		private const string UNALIGNED_IMAGE = "iconunaligned_thumb.png";

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
