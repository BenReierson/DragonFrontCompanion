using DragonFrontDb;
using DragonFrontDb.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DragonFrontCompanion
{
    [JsonObject]
    public class Deck : ObservableCollection<Card>
    {
        public const int MAX_CARD_COUNT = 30;
        public const int CARD_DUPLICATE_LIMIT = 2;
        public const int MAX_DISTRIBUTION_LEVEL = 7;

        public static string CurrentAppVersion = "";
        public static ReadOnlyDictionary<string, Card> CardDictionary = null;

        private bool _suppressEvents = false;

        public Deck(Faction deckFaction, string appVersion = "NA", DeckType type = DeckType.NORMAL_DECK)
        {
            if (!Enum.IsDefined(typeof(Faction), deckFaction) ||
                deckFaction == Faction.UNALIGNED ||
                deckFaction == Faction.INVALID)
            { throw new ArgumentException("Invalid deck class."); }

            if (CardDictionary == null) throw new ArgumentException("Deck.CardDictionary must be set before creating decks.");

            DeckFaction = deckFaction;
            ID = Guid.NewGuid();
            AppVersion = appVersion;
            Type = type;
            UpdateDistributions();
        }

        #region Classes
        public class CardGroup
        {
            public Card Card { get; set; }
            public int Count { get; set; }
        }

        public class CostVertical
        {
            public const int MAX_WEIGHT = 16;
            public int Cost { get; set; }
            public int Count { get; set; }
            public int Weight { get; set; }
        }
        #endregion  

        #region Properties

        private bool _allowOverload = false;
        [JsonIgnore]
        public bool CanOverload
        {
            get
            {
                if (Count > MAX_CARD_COUNT) _allowOverload = true; 
                return _allowOverload || Settings.AllowDeckOverload;
            }
            set { _allowOverload = value; OnPropertyChanged(); }
        }

        private bool _canUndo = false;
        [JsonIgnore]
        public bool CanUndo
        {
            get { return _canUndo; }
            internal set { _canUndo = value;  OnPropertyChanged(); }
        }

        [JsonProperty]
        public string AppVersion { get; private set; }

        [JsonProperty]
        public Guid ID { get; internal set; }

        private DateTime _lastModified = DateTime.Now;
        [JsonProperty]
        public DateTime LastModified
        {
            get { return _lastModified; }
            set { _lastModified = value; OnPropertyChanged(); }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public Faction DeckFaction { get; private set; }

        private DeckType _type = DeckType.NORMAL_DECK;
        [JsonConverter(typeof(StringEnumConverter))]
        public DeckType Type {
            get
            {
                return _type;
            }
            internal set
            {
                if (_type == DeckType.EXTERNAL_DECK && value == DeckType.NORMAL_DECK)
                {
                    ID = Guid.NewGuid();
                }
                _type = value;
            }
        } 

        private string _name;
        [JsonProperty]
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(); }
        }

        private string _description;
        [JsonProperty]
        public string Description
        {
            get { return _description; }
            set { _description = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Used for binding to UI and serialization
        /// </summary>
        private List<CardGroup> _distinctView;
        [JsonProperty("Cards")]
        public List<CardGroup> DistinctView
        {
            get
            {
                if (_distinctView != null) return _distinctView; 

                _distinctView = (from card in this.Distinct()
                                where card.Type != CardType.CHAMPION
                                orderby card.Cost, card.Name
                                select new CardGroup { Card = card, Count = CountCard(card) })?.ToList();

                if (Champion != null) _distinctView?.Insert(0, new CardGroup() { Card = Champion, Count = 1 });

                return _distinctView;
            }
            private set
            {//should only be called during deserialization
                _suppressEvents = true;

                var oldData = String.Compare(CurrentAppVersion, this.AppVersion) > 0;
                CanOverload = true; //allow overloading while deserializing

                foreach (var cardGroup in value)
                {
                    for (int i = 1; i <= cardGroup.Count && i <= CARD_DUPLICATE_LIMIT; i++)
                    {
                        if (CardDictionary != null && oldData && !CardDictionary.ContainsKey(cardGroup.Card.ID))
                        {//find old card by name in new data, this allows for ID udpates
                            var updatedCard = CardDictionary.FirstOrDefault(c => c.Value.Name == cardGroup.Card.Name);
                            this.Add(updatedCard.Value);
                        }
                        else this.Add(CardDictionary[cardGroup.Card.ID]);
                    }
                }
                this.AppVersion = CurrentAppVersion;

                CanOverload = Count > MAX_CARD_COUNT;

                _suppressEvents = false;

                OnCardsChanged();
            }
        }

        private List<CardGroup> _distinctFaction;
        [JsonIgnore]
        public List<CardGroup> DistinctFaction => _distinctFaction ?? (_distinctFaction = DistinctView.Where(c => c.Card.Faction == this.DeckFaction).ToList());

        private List<CardGroup> _distinctUnaligned;
        [JsonIgnore]
        public List<CardGroup> DistinctUnaligned => _distinctUnaligned ?? (_distinctUnaligned = DistinctView.Where(c => c.Card.Faction == Faction.UNALIGNED).ToList());


        private Card _champion = null;
        [JsonProperty]
        public Card Champion
        {
            get
            {
                return _champion;
            }
            set
            {
                if (value == null && _champion == null) return;
                if (value != null && value.Equals(_champion)) return;

                if (value == null)
                {
                    _champion = value;
                    OnPropertyChanged();
                    OnCardsChanged();
                    return;
                }
                if (value.Type != CardType.CHAMPION || !value.ValidFactions.Contains(DeckFaction))
                    throw new ArgumentException("Invalid Champion for this deck.");

                _champion = value;
                OnPropertyChanged();
                OnCardsChanged();
            }
        }

        private IEnumerable<CostVertical> _costs = new List<CostVertical>();
        [JsonIgnore]
        public IEnumerable<CostVertical> CostDistribution
        {
            get { return _costs; }
            private set
            {
                _costs = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public string FilePath { get; internal set; }

        [JsonIgnore]
        public int UnitCount => this.Count((c) => c.Type == CardType.UNIT);
        [JsonIgnore]
        public int SpellCount => this.Count((c) => c.Type == CardType.SPELL);
        [JsonIgnore]
        public int FortCount => this.Count((c) => c.Type == CardType.FORT);

        [JsonIgnore]
        public bool IsValid
        {
            get
            {
                if (String.IsNullOrEmpty(Name)) return false;
                if (DeckFaction == Faction.INVALID || !Enum.IsDefined(typeof(Faction), DeckFaction)) return false;
                if (this.Count != MAX_CARD_COUNT) return false;
                if (this.Champion == null) return false;

                var uniqueCards = this.Distinct();
                foreach (var card in uniqueCards)
                {
                    if (CountCard(card) > CARD_DUPLICATE_LIMIT) return false;
                }

                return true;
            }
        }

        [JsonIgnore]
        public int TotalScrapPrice => this.Sum(c => c.ForgePrice) + (Champion == null ? 0 : Champion.ForgePrice);

        #endregion

        #region Overrides
        public override string ToString()
        {
            return DeckFaction + " | " + Name + " - " + Description;
        }

        public override bool Equals(object obj)
        {
            var deck = obj as Deck;

            bool equal = deck != null &&
                ID == deck.ID &&
                Count == deck.Count &&
                Name == deck.Name &&
                Description == deck.Description &&
                DeckFaction == deck.DeckFaction &&
                Type == deck.Type;

            if (equal)
            {
                if (Champion != null) equal = Champion.Equals(deck.Champion);
                if (equal && deck.Champion != null) deck.Champion.Equals(Champion);
            }

            if (equal)
            {
                foreach (var card in this)
                {
                    equal = this.All(deck.Contains);
                    if (!equal) break;
                    equal = deck.All(this.Contains);
                    if (!equal) break;
                }
            }
            return equal;
        }

        protected override void InsertItem(int index, Card newCard)
        {
            var validCard = ValidateForDeck(newCard);

            if (validCard.Type == CardType.CHAMPION) Champion = validCard; //set champion instead of adding to collection
            else base.InsertItem(index, validCard);

            UpdateDistributions();
            OnCardsChanged();
        }


        protected override void RemoveItem(int index)
        {
            var item = this[index];

            base.RemoveItem(index);

            UpdateDistributions();
            OnCardsChanged();
        }

        protected override void SetItem(int index, Card newCard)
        {
            var validCard = ValidateForDeck(newCard);

            if (validCard.Type == CardType.CHAMPION) Champion = validCard; //set champion instead of adding to collection
            else base.SetItem(index, validCard);

            UpdateDistributions();
            OnCardsChanged();
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            Champion = null;
            UpdateDistributions();
            OnCardsChanged();
        }
        #endregion

        #region Methods

        private Card ValidateForDeck(Card newCard)
        {
            Card validCard;
            //validate
            if (CardDictionary != null && CardDictionary.ContainsKey(newCard.ID)) validCard = CardDictionary[newCard.ID];
            else throw new ArgumentException("Card is not recognized.");

            if (!validCard.ValidFactions.Contains(DeckFaction)) throw new ArgumentException("Card is the wrong faction for this deck.");
            if (CountCard(validCard) >= CARD_DUPLICATE_LIMIT) throw new ArgumentException("Deck is at capacity for this card.");
            if (validCard.Type != CardType.CHAMPION && !CanOverload && Count >= MAX_CARD_COUNT) throw new ArgumentException("Deck is at capacity.");

            return validCard;
        }

        public int CountCard(Card cardToCount)
        {
            if (cardToCount == null) return 0;

            if (Champion != null && Champion == cardToCount) return 1;

            int count = this.Count((c) => c.Equals(cardToCount));
            return count;
        }

        private void UpdateDistributions()
        {
            var newCostDist = new List<CostVertical>(MAX_DISTRIBUTION_LEVEL + 1);
            for (int i = 0; i < newCostDist.Capacity; i++)
            { newCostDist.Insert(i, new CostVertical() { Cost = i}); }

            int maxCount = 0;

            foreach (var card in this)
            {
                if (card.Cost <= MAX_DISTRIBUTION_LEVEL)
                {
                    newCostDist[card.Cost].Count++;
                    newCostDist[card.Cost].Weight++;
                    if (maxCount < newCostDist[card.Cost].Count) maxCount = newCostDist[card.Cost].Count;
                }
                else
                {
                    newCostDist[MAX_DISTRIBUTION_LEVEL].Count++;
                    newCostDist[MAX_DISTRIBUTION_LEVEL].Weight++;
                    if (maxCount < newCostDist[MAX_DISTRIBUTION_LEVEL].Count) maxCount = newCostDist[MAX_DISTRIBUTION_LEVEL].Count;
                }
            }
            
            if (maxCount > 0 && maxCount < CostVertical.MAX_WEIGHT)
            {
                double factor = CostVertical.MAX_WEIGHT / (double)maxCount;
                if (factor > 5) factor = 5;

                foreach (var cost in newCostDist)
                {
                    cost.Weight = Convert.ToInt32(cost.Count * factor);
                }
            }

            CostDistribution = newCostDist;
        }

        private void OnCardsChanged()
        {
            if (_suppressEvents) return;

            _distinctView = null;
            _distinctUnaligned = null;
            _distinctFaction = null;
            OnPropertyChanged(nameof(Deck.DistinctView));
            OnPropertyChanged(nameof(Deck.DistinctUnaligned));
            OnPropertyChanged(nameof(Deck.DistinctFaction));


            OnPropertyChanged(nameof(Deck.IsValid));
            OnPropertyChanged(nameof(UnitCount));
            OnPropertyChanged(nameof(SpellCount));
            OnPropertyChanged(nameof(FortCount));
            OnPropertyChanged(nameof(TotalScrapPrice));
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }


}
