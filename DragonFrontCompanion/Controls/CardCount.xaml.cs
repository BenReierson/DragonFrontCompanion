using DragonFrontDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using static DragonFrontCompanion.Deck;

namespace DragonFrontCompanion.Controls
{
    public partial class CardCount : ContentView
    {
        public CardCount()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty CardGroupsProperty = BindableProperty.Create(nameof(CardGroups), typeof(List<Deck.CardGroup>), typeof(CardCount), propertyChanged: OnCardGroupsChanged);

        private static void OnCardGroupsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var instance = bindable as CardCount;
            if (instance == null || newValue == null) return;

            if (instance.Card == null) instance.CountIcon.Source = "";
            else
            {
                var group = ((List<Deck.CardGroup>)newValue).FirstOrDefault(g => g.Card == instance.Card);
                
                if (group == null) instance.CountIcon.Source = "";
                else if (group.Count == 1) instance.CountIcon.Source = "IconOne.png";
                else if (group.Count == 2) instance.CountIcon.Source = "IconTwo.png";
            }
        }

        public List<Deck.CardGroup> CardGroups
        {
            get { return (List<Deck.CardGroup>)GetValue(CardGroupsProperty); }
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
            if (instance == null || newValue == null) return;

            if (instance.Card == null) instance.CountIcon.Source = "";
            else
            {
                var group = instance.CardGroups?.FirstOrDefault(g => g.Card == instance.Card);

                if (group == null) instance.CountIcon.Source = "";
                else if (group.Count == 1) instance.CountIcon.Source = "IconOne.png";
                else if (group.Count == 2) instance.CountIcon.Source = "IconTwo.png";
            }
        }
    }
}
