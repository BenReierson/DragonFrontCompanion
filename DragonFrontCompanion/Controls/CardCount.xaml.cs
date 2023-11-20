using DragonFrontDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static DragonFrontCompanion.Deck;

namespace DragonFrontCompanion.Controls
{
    public partial class CardCount : ContentView
    {
        private const string ICON_ONE = "iconone.png";
		private const string ICON_TWO = "icontwo.png";

		public CardCount()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty CardGroupsProperty = BindableProperty.Create(nameof(CardGroups), typeof(Dictionary<string, CardGroup>), typeof(CardCount), propertyChanged: OnCardGroupsChanged);

        private static void OnCardGroupsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var instance = bindable as CardCount;
            if (instance == null || newValue == null) return;

            if (instance.Card != null)
            {
                if (((Dictionary<string, CardGroup>)newValue).ContainsKey(instance.Card.ID))
                {
                    instance.CountIcon.IsVisible = true;
                    var group = ((Dictionary<string, CardGroup>)newValue)[instance.Card.ID];

                    if      (group.Count == 1) instance.CountIcon.Source = ICON_ONE;
                    else if (group.Count == 2) instance.CountIcon.Source = ICON_TWO;
                }
                else instance.CountIcon.IsVisible = false;
            }
            else instance.CountIcon.IsVisible = false;
        }

        public Dictionary<string, CardGroup>CardGroups
        {
            get { return (Dictionary<string, CardGroup>)GetValue(CardGroupsProperty); }
            set { SetValue(CardGroupsProperty, value); }
        }

        public static readonly BindableProperty CardProperty = BindableProperty.Create(nameof(Card), typeof(Card), typeof(CardCount), propertyChanged:OnCardChanged);

        public Card Card
        {
            get { return (Card)GetValue(CardProperty); }
            set { SetValue(CardProperty, value); }
        }

        private static void OnCardChanged(BindableObject bindable, object oldValue, object newValue)
        {
			var instance = bindable as CardCount;
			if (instance == null || newValue == null || instance.CardGroups == null) return;

			if (instance.Card != null)
			{
				if (instance.CardGroups.ContainsKey(instance.Card.ID))
				{
					instance.CountIcon.IsVisible = true;
					var group = instance.CardGroups[instance.Card.ID];

					if      (group.Count == 1) instance.CountIcon.Source = ICON_ONE;
					else if (group.Count == 2) instance.CountIcon.Source = ICON_TWO;
				}
				else instance.CountIcon.IsVisible = false;
			}
			else instance.CountIcon.IsVisible = false;
        }
    }
}
