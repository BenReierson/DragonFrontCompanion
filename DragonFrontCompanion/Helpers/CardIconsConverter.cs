using System;
using System.Globalization;
using DragonFrontDb;
using Xamarin.Forms;
using DragonFrontDb.Enums;

namespace DragonFrontCompanion.Helpers
{
    /// <summary>
    /// Convert card to icons representing that card based on input parameter index
    /// </summary>
    public class CardIconsConverter : IValueConverter
    {
		private const string CHAMP_IMAGE = "IconChamp.png";
		private const string SPELL_IMAGE = "IconSpell.png";
		private const string FORT_IMAGE = "IconFort.png";
		private const string UNIT_IMAGE = "IconUnit.png";
		private const string UNALIGNED_IMAGE = "IconUnaligned_1.png";
        private const string TOKEN_IMAGE = "IconToken.png";

        private const string FACTION_TEMPLATE = "Icon{0}_1.png";

        private static int _totalFactionCount = Enum.GetValues(typeof(Faction)).Length - 2;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var card = value as Card;
            if (card == null || parameter == null) return null;

            int index = -1;
            int.TryParse(parameter.ToString(), out index);

            //Show type then valid factions
            if (index == 0) return GetTypeIcon(card.Type);
            else if (card.ValidFactions.Length == _totalFactionCount) return index == 1 ? UNALIGNED_IMAGE : null;
            else return card.ValidFactions.Length > index-1 ? string.Format(FACTION_TEMPLATE, card.ValidFactions[index-1]) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string GetTypeIcon(CardType cardType)
        {
            switch (cardType)
            {
                case CardType.FORT:
                    return FORT_IMAGE;
                case CardType.CHAMPION:
                    return CHAMP_IMAGE;
                case CardType.SPELL:
                    return SPELL_IMAGE;
                case CardType.UNIT:
                    return UNIT_IMAGE;
                default:
                    return UNALIGNED_IMAGE;
            }
        }
    }
}
