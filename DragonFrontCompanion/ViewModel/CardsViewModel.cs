
using DragonFrontDb;
using DragonFrontDb.Enums;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DragonFrontCompanion.Data;
using Xamarin.Forms;

namespace DragonFrontCompanion.ViewModel
{
    public class CardsViewModel : ViewModelBase
    {
        public const int MAX_COSTS_FILTER = Deck.MAX_DISTRIBUTION_LEVEL + 1;
        private static int FACTION_COUNT = Enum.GetNames(typeof(Faction)).Count() - 2;

        private static bool _firstLoad = true;

        private ReadOnlyCollection<Card> _unfilteredCards = null;

        private INavigationService _navigationService;
        private ICardsService _cardsService;
        private IDialogService _dialog;
        private IDeckService _deckService;

        private bool _suspendFilters = false;
        private Card _lastActionedCard = null;

        public CardsViewModel(INavigationService navigationService, ICardsService cardsService, IDialogService dialog, IDeckService deckService)
        {
            _navigationService = navigationService;
            _cardsService = cardsService;
            _dialog = dialog;
            _deckService = deckService;

            _cardsService.DataUpdated += (o, e) =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await InitializeAsync(CurrentDeck);
                    await ApplyFilters();
                });
            };
        }

        public async Task InitializeAsync(Deck deck = null, string searchText = null)
        {
			IsBusy = true;

			if (_firstLoad)
            {//Wait longer for the ui to load
                _firstLoad = false;
                await Task.Delay(1500);
            }
            else await Task.Delay(500);
                
            var freshCards = await _cardsService.GetAllCardsAsync();
            if (_unfilteredCards != freshCards)
            {
                _unfilteredCards = freshCards;
                RaisePropertyChanged(nameof(EigthFactionEnabled));
                AllCards = _unfilteredCards.ToList(); 
            }

            _suspendFilters = searchText != null;

            if (deck == null || deck != CurrentDeck) await DeckInitialize(deck);

            if (searchText != null)
            {
                IsBusy = true;
				await Task.Delay(500);//let the ui settle
                _suspendFilters = false;
				if (CardSets.Contains(searchText))
                {
                    CardSetFilter = searchText;
                    MessagingCenter.Send<string>($"Card Set: {searchText}", App.MESSAGES.SHOW_TOAST);
                }
                else SearchText = searchText;
            }

            IsBusy = false;
        }

        private async Task DeckInitialize(Deck deck)
        {
			CurrentDeck = deck;
			IsChooser = CurrentDeck != null;
			if (!IsChooser) ChooserFilterText = "";
			Message = "";

			if (IsChooser)
            {
                //filter cards according to deck
                AllCards = _unfilteredCards.Where((c) => (c.ValidFactions.Contains(CurrentDeck.DeckFaction) && c.Rarity != Rarity.TOKEN)).ToList();

                ChooserFilterText = "IconDeckFilter.png";
                CanFilterByEclipse = CurrentDeck.DeckFaction == Faction.ECLIPSE;
                CanFilterByScales = CurrentDeck.DeckFaction == Faction.SCALES;
                CanFilterByStrife = CurrentDeck.DeckFaction == Faction.STRIFE;
                CanFilterByThorns = CurrentDeck.DeckFaction == Faction.THORNS;
                CanFilterBySilence = CurrentDeck.DeckFaction == Faction.SILENCE;
                CanFilterByEssence = CurrentDeck.DeckFaction == Faction.ESSENCE;
                CanFilterByDelirium = CurrentDeck.DeckFaction == Faction.DELIRIUM;
                CanFilterByEigth = (int)CurrentDeck.DeckFaction == 9;

                if (ResetFiltersCommand.CanExecute(null)) ResetFiltersCommand.Execute(null);
                else await ApplyFilters();

                RaisePropertyChanged(nameof(CanFilterByEclipse));
                RaisePropertyChanged(nameof(CanFilterByScales));
                RaisePropertyChanged(nameof(CanFilterByStrife));
                RaisePropertyChanged(nameof(CanFilterByThorns));
                RaisePropertyChanged(nameof(CanFilterBySilence));
                RaisePropertyChanged(nameof(CanFilterByEssence));
                RaisePropertyChanged(nameof(CanFilterByDelirium));
                RaisePropertyChanged(nameof(CanFilterByEigth));

            }
            else
            {
                AllCards = _unfilteredCards?.ToList();
                FilteredCards = AllCards;

                CanFilterByEclipse = true;
                CanFilterByScales = true;
                CanFilterByStrife = true;
                CanFilterByThorns = true;
                CanFilterBySilence = true;
                CanFilterByEssence = true;
                CanFilterByDelirium = true;
                CanFilterByEigth = true;
                FilterByDeck = false;

                if (ResetFiltersCommand.CanExecute(null)) ResetFiltersCommand.Execute(null);
                else await ApplyFilters();
            }
        }

        private async Task ApplyFilters()
        {
			if (AllCards != null && !_suspendFilters)
            {
                var filtered =
                from c in AllCards
                where (!FilteredByFaction || (FactionFilter != Faction.UNALIGNED && c.ValidFactions.Contains(FactionFilter) && c.ValidFactions.Count() != FACTION_COUNT) || 
                                             (FactionFilter == Faction.UNALIGNED && c.Faction == Faction.UNALIGNED && c.ValidFactions.Count() == FACTION_COUNT)) &&
                      (!FilteredByType || c.Type == TypeFilter) &&
                      (CostFilter == MAX_COSTS_FILTER || ((CostFilter == MAX_COSTS_FILTER - 1 && c.Cost >= CostFilter) || c.Cost == CostFilter)) &&
                      (RarityFilter == 0 || (int)c.Rarity == RarityFilter) &&
                      ((TraitFilter == null || !TraitFilter.Any()) || TraitFilter.Intersect(c.Traits).Any()) &&
                      (CurrentDeck == null || !FilterByDeck || CurrentDeck.Contains(c) || CurrentDeck.Champion == c) &&
                      (string.IsNullOrEmpty(CardSetFilter) || CardSetFilter == _CARD_SET_FILTER_DEFAULT || c.CardSet == (CardSet)Enum.Parse(typeof(CardSet), CardSetFilter)) &&
                      (string.IsNullOrEmpty(SearchText) ||
                       c.Text.ToLower().Contains(SearchText.ToLower()) ||
                       c.Name.ToLower().Contains(SearchText.ToLower()) ||
                       c.Race.ToString().ToLower().Contains(SearchText.ToLower()) ||
                       SearchText.ToUpper().Contains(c.CardSet.ToString()))
                orderby c.Rarity == Rarity.TOKEN, c.Type == CardType.CHAMPION descending, c.Faction descending, c.Cost, c.Name
                select c;

                IsBusy = true;
                var newList = await Task.Run(() => filtered.ToList());
                FilteredCards = newList;
                IsBusy = false;
                UpdateStatus();

                ResetFiltersCommand.RaiseCanExecuteChanged();
            }
        }

        private void UpdateStatus()
        {
            CardsTitle = $"Cards ({FilteredCards.Count})";

            if (IsChooser) DeckStatus = "Deck    " + CurrentDeck.Count + " / 30    " +
                             (CurrentDeck.Champion != null ? "1C | " : "0C | ") +
                             CurrentDeck.UnitCount + "U | " +
                             CurrentDeck.FortCount + "F | " +
                             CurrentDeck.SpellCount + "S    " +
                             (CurrentDeck.IsValid ? "Ready To Use" : "Not Ready");
        }

        #region Properties
        private List<string> _cardSets;
        public List<string> CardSets
        {
            get
            {
                if (_cardSets != null) return _cardSets;
                else
                {
                    _cardSets = Enum.GetValues(typeof(CardSet)).Cast<CardSet>().Skip(2).AsQueryable().Select(cs => cs.ToString()).ToList();
                    _cardSets.Insert(0, _CARD_SET_FILTER_DEFAULT);
                    CardSetFilter = _CARD_SET_FILTER_DEFAULT;
                    return _cardSets;
                }
            }
        }

        private bool _isBusy = false;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { Set(ref _isBusy, value); }
        }


        private string _deckStatus = "";
        public string DeckStatus
        {
            get { return _deckStatus; }
            set { Set(ref _deckStatus, value); }
        }

        private List<Card> _allCards = null;
        public List<Card> AllCards
        {
            get { return _allCards; }
            set { Set(ref _allCards, value); }
        }

        private Card _selectedCard = null;
        public Card SelectedCard
        {
            get { return _selectedCard; }
            set
            {
                if ((App.RuntimePlatform == App.Device.Windows || App.RuntimePlatform == App.Device.WinPhone)
                    && _lastActionedCard != null && value == _lastActionedCard)
                {
                    _lastActionedCard = null;
                    return;
                }

                Set(ref _selectedCard, value);
                PreviousCardCommand.RaiseCanExecuteChanged();
                NextCardCommand.RaiseCanExecuteChanged();
                if (value != null) _lastActionedCard = null;
            }
        }

        private List<Card> _filteredCards = null;
        public List<Card> FilteredCards
        {
            get { return _filteredCards; }
            set
            {
                SelectedCard = null;
                Set(ref _filteredCards, value);
            }
        }

        private Deck _deck = null;
        public Deck CurrentDeck
        {
            get { return _deck; }
            set {Set(ref _deck, value);}
        }

        private string _cardsTitle = "Cards";
        public string CardsTitle
        {
            get { return _cardsTitle; }
            set {Set(ref _cardsTitle, value);}
        }

        private bool _isChooser = false;
        public bool IsChooser
        {
            get { return _isChooser; }
            set { Set(ref _isChooser, value); }
        }


        private string _chooserFilter = "";
        public string ChooserFilterText
        {
            get { return _chooserFilter; }
            set { Set(ref _chooserFilter, value); }
        }

        private string _resetFilterIcon = "";
        public string ResetFilterIcon
        {
            get { return _resetFilterIcon; }
            set { Set(ref _resetFilterIcon, value); }
        }

        private string _resetFilterText = "";
        public string ResetFilterText
        {
            get { return _resetFilterText; }
            set { Set(ref _resetFilterText, value); }
        }

        private string _msg = "";
        public string Message
        {
            get { return _msg; }
            set
            {
                Set(ref _msg, value);
                if (App.RuntimePlatform == App.Device.Android &&
                    !string.IsNullOrEmpty(value)) MessagingCenter.Send<string>(value, App.MESSAGES.SHOW_TOAST);
            }
        }


        private int _costFilter = MAX_COSTS_FILTER;
        public int CostFilter
        {
            get { return _costFilter; }
            set
            {
                if (_costFilter == value) return;

                Set(ref _costFilter, value);
                if (value == MAX_COSTS_FILTER) CostFilterText = "*";
                else if (value == MAX_COSTS_FILTER - 1) CostFilterText = (MAX_COSTS_FILTER - 1) + "+";
                else CostFilterText = value.ToString();
                ApplyFilters();
            }
        }

        private string _costFilterText = "*";
        public string CostFilterText
        {
            get { return _costFilterText; }
            set { Set(ref _costFilterText, value); }
        }

        private int _RarityFilter = 0;
        public int RarityFilter
        {
            get { return _RarityFilter; }
            set
            {
                if (_RarityFilter == value) return;

                Set(ref _RarityFilter, value);
                if (value == 0) RarityFilterText = "All";
                else RarityFilterText = ((Rarity)value).ToString();
                ApplyFilters();
            }
        }

        private string _RarityFilterText = "All";
        public string RarityFilterText
        {
            get { return _RarityFilterText; }
            set { Set(ref _RarityFilterText, value); }
        }


        private bool _filterByDeck = false;
        public bool FilterByDeck
        {
            get { return _filterByDeck; }
            private set { Set(ref _filterByDeck, value); }
        }

        private bool _filterByType = false;
        public bool FilteredByType
        {
            get { return _filterByType; }
            set { Set(ref _filterByType, value); }
        }

        private CardType _typeFilter = CardType.INVALID;
        public CardType TypeFilter
        {
            get { return _typeFilter; }
            private set
            {
                Set(ref _typeFilter, value);
                if (App.RuntimePlatform == App.Device.iOS)
                {
                    TypeFilterText = _typeFilter == CardType.INVALID ? "ALL" : _typeFilter.ToString();
                }
            }
        }

        private string _typeFilterText = "Type";
        public string TypeFilterText
        {
            get { return _typeFilterText; }
            set { Set(ref _typeFilterText, value); }
        }

        private bool _filterByFaction = false;
        public bool FilteredByFaction
        {
            get { return _filterByFaction; }
            set { Set(ref _filterByFaction, value); }
        }

        private Faction _factionFilter = Faction.INVALID;
        public Faction FactionFilter
        {
            get { return _factionFilter; }
            set
            {
                Set(ref _factionFilter, value);

                if (App.RuntimePlatform == App.Device.iOS)
                {
                    FactionFilterText = _factionFilter == Faction.INVALID ? "ALL" : _factionFilter.ToString();
                }
            }
        }

        private string _factionFilterText = "Faction";
        public string FactionFilterText
        {
            get { return _factionFilterText; }
            set { Set(ref _factionFilterText, value); }
        }

        private bool _eclipseFilter = true;
        public bool CanFilterByEclipse
        {
            get { return _eclipseFilter; }
            set { Set(ref _eclipseFilter, value); }
        }

        private bool _scalesFilter = true;
        public bool CanFilterByScales
        {
            get { return _scalesFilter; }
            set { Set(ref _scalesFilter, value); }
        }

        private bool _strifeFilter = true;
        public bool CanFilterByStrife
        {
            get { return _strifeFilter; }
            set { Set(ref _strifeFilter, value); }
        }

        private bool _thornsFilter = true;
        public bool CanFilterByThorns
        {
            get { return _thornsFilter; }
            set { Set(ref _thornsFilter, value); }
        }

        private bool _silenceFilter = true;
        public bool CanFilterBySilence
        {
            get { return _silenceFilter; }
            set { Set(ref _silenceFilter, value); }
        }

        private bool _essenceFilter = true;
        public bool CanFilterByEssence
        {
            get { return _essenceFilter; }
            set { Set(ref _essenceFilter, value); }
        }
        private bool _essenceDelirium = true;
        public bool CanFilterByDelirium
        {
            get { return _essenceDelirium; }
            set { Set(ref _essenceDelirium, value); }
        }

        private bool _eigthFilter = true;
        public bool CanFilterByEigth
        {
            get { return _eigthFilter; }
            set { Set(ref _eigthFilter, value); }
        }

        public bool EigthFactionEnabled => _unfilteredCards != null ? _unfilteredCards.Any(c => (int)c.Faction == 9) : false;
        public string EigthFactionText => Enum.TryParse("9", out Faction faction) ? faction.ToString() : "";

        private List<string> _traitFilter;
        public List<string> TraitFilter
        {
            get { return _traitFilter; }
            private set
            {
                Set(ref _traitFilter, value);
                if (value != null && value.Count() > 0)
                {
                    var display = new StringBuilder("Traits: ");
                    foreach (var trait in value)
                    {
                        display.Append(trait.ToString().Trim('_').Replace('_', ' '));
                        display.Append(", ");
                    }
                    TraitFilterDisplay = display.ToString().Trim().Trim(',');
                    MessagingCenter.Send<string>(TraitFilterDisplay, App.MESSAGES.SHOW_TOAST);
                }
                else TraitFilterDisplay = "";
            }
        }

        private string _traitFilterDisplay = "";
        public string TraitFilterDisplay
        {
            get { return _traitFilterDisplay; }
            set { Set(ref _traitFilterDisplay, value); }
        }

        private const string _CARD_SET_FILTER_DEFAULT = "ALL";
        private string _cardSetFilter;
        public string CardSetFilter
        {
            get { return _cardSetFilter; }
            set
            {
                Set(ref _cardSetFilter, value);
                ApplyFilters();
            }
        }

        private string _searchText = "";
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (_searchText == value) return;

                Set(ref _searchText, value);
                if (App.RuntimePlatform == App.Device.Windows)
                {//Delay search filter to help with performance on windows
                    if (!_suspendFilters)
                    {
                        _suspendFilters = true;
                        Task.Run(async () =>
                        {
                            await Task.Delay(750);
                            if (_suspendFilters)
                            {
                                _suspendFilters = false;
                                Device.BeginInvokeOnMainThread(() => ApplyFilters());
                            }
                        });
                    }
                }
                else ApplyFilters();
            }
        }
        #endregion

        #region Commands
        private RelayCommand<Card> _addCardToDeck;

        /// <summary>
        /// Gets the AddOneToDeckCommand.
        /// </summary>
        public RelayCommand<Card> AddOneToDeckCommand
        {
            get
            {
                return _addCardToDeck
                    ?? (_addCardToDeck = new RelayCommand<Card>(
                    p =>
                    {
                        try
                        {
                            if (CurrentDeck.CountCard(p) == Deck.CARD_DUPLICATE_LIMIT)
                            { Message = "Deck already contains " + Deck.CARD_DUPLICATE_LIMIT + " copies."; }
                            else
                            {
                                CurrentDeck?.Add(p);

                                Message = p.Name + " Added";
                                AddOneToDeckCommand.RaiseCanExecuteChanged();
                                RemoveCardCommand.RaiseCanExecuteChanged();
                            }
                            _lastActionedCard = p;
                            UpdateStatus();
                        }
                        catch (ArgumentException ex)
                        {
                            Message = ex.Message;
                        }
                    },
                    p => CurrentDeck != null && p != null &&
                         (CurrentDeck.Champion == null || !CurrentDeck.Champion.Equals(p)) &&
                         (CurrentDeck.CountCard(p) < Deck.CARD_DUPLICATE_LIMIT) &&
                         (CurrentDeck.CanOverload || (p.Type == CardType.CHAMPION || CurrentDeck.Count < Deck.MAX_CARD_COUNT))));
            }
        }

        private RelayCommand<Card> _removeCard;

        /// <summary>
        /// Gets the AddOneToDeckCommand.
        /// </summary>
        public RelayCommand<Card> RemoveCardCommand
        {
            get
            {
                return _removeCard
                    ?? (_removeCard = new RelayCommand<Card>(
                    p =>
                    {
                        try
                        {
                            if (p.Type == CardType.CHAMPION && CurrentDeck.Champion.Equals(p)) CurrentDeck.Champion = null;
                            else CurrentDeck.Remove(p);

                            Message = p.Name + " Removed";
                            AddOneToDeckCommand.RaiseCanExecuteChanged();
                            RemoveCardCommand.RaiseCanExecuteChanged();
                            _lastActionedCard = p;
                            UpdateStatus();
                        }
                        catch (ArgumentException ex)
                        {
                            Message = ex.Message;
                        }
                    },
                    p => (CurrentDeck != null && CurrentDeck.Contains(p)) ||
                         (p?.Type == CardType.CHAMPION && CurrentDeck?.Champion != null && CurrentDeck.Champion.Equals(p))
                         ));
            }
        }

        private RelayCommand _deckFilter;

        /// <summary>
        /// Gets the DeckFilterCommand.
        /// </summary>
        public RelayCommand DeckFilterCommand
        {
            get
            {
                return _deckFilter
                    ?? (_deckFilter = new RelayCommand(
                    () =>
                    {
                        if (!IsChooser) return;
                        FilterByDeck = !FilterByDeck;
                        ChooserFilterText = FilterByDeck ? "IconCardsAll.png" : "IconDeckFilter.png";

                        if (FilterByDeck) Message = "Showing current deck cards";
                        else Message = "Showing all valid cards";

                        ApplyFilters();
                    }));
            }
        }

        private RelayCommand<Faction> _toggleFaction;

        /// <summary>
        /// Gets the FilterFactionCommand.
        /// </summary>
        public RelayCommand<Faction> FilterFactionCommand
        {
            get
            {
                return _toggleFaction
                    ?? (_toggleFaction = new RelayCommand<Faction>(
                    p =>
                    {
                        FilteredByFaction = true;
                        FactionFilter = p;
                        ApplyFilters();
                    },
                    p => true));
            }
        }

        private RelayCommand<CardType> _toggleCardType;

        /// <summary>
        /// Gets the FilterCardTypeCommand.
        /// </summary>
        public RelayCommand<CardType> FilterCardTypeCommand
        {
            get
            {
                return _toggleCardType
                    ?? (_toggleCardType = new RelayCommand<CardType>(
                    p =>
                    {
                        FilteredByType = true;
                        TypeFilter = p;
                        ApplyFilters();
                    },
                    p => true));
            }
        }

        private RelayCommand<Rarity> _filterRarity;

        /// <summary>
        /// Gets the FilterByRarityCommand.
        /// </summary>
        public RelayCommand<Rarity> FilterByRarityCommand
        {
            get
            {
                return _filterRarity
                    ?? (_filterRarity = new RelayCommand<Rarity>(
                    p =>
                    {
                        RarityFilter = (int)p;
                    }));
            }
        }

        private RelayCommand _resetFactionFilter;

        /// <summary>
        /// Gets the ResetFactionFilters.
        /// </summary>
        public RelayCommand ResetFactionFiltersCommand
        {
            get
            {
                return _resetFactionFilter
                    ?? (_resetFactionFilter = new RelayCommand(
                    () =>
                    {
                        FilteredByFaction = false;
                        FactionFilter = Faction.INVALID;
                        ApplyFilters();
                    }));
            }
        }

        private RelayCommand _resetTypeFilter;

        /// <summary>
        /// Gets the ResetTypeFilterCommand.
        /// </summary>
        public RelayCommand ResetTypeFilterCommand
        {
            get
            {
                return _resetTypeFilter
                    ?? (_resetTypeFilter = new RelayCommand(
                    () =>
                    {
                        FilteredByType = false;
                        TypeFilter = CardType.INVALID;
                        ApplyFilters();
                    }));
            }
        }

        private RelayCommand _resetFilters;

        /// <summary>
        /// Gets the ResetFiltersCommand.
        /// </summary>
        public RelayCommand ResetFiltersCommand
        {
            get
            {
                return _resetFilters
                    ?? (_resetFilters = new RelayCommand(
                    () =>
                    {
                        _suspendFilters = true;

                        ResetFactionFiltersCommand.Execute(null);
                        ResetTypeFilterCommand.Execute(null);
                        if (IsChooser && FilterByDeck) DeckFilterCommand.Execute(null);
                        TraitFilter = null;
                        CostFilter = MAX_COSTS_FILTER;
                        RarityFilter = 0;
                        SearchText = "";
                        CardSetFilter = _CARD_SET_FILTER_DEFAULT;

                        _suspendFilters = false;

                        ApplyFilters();
                        ResetFiltersCommand.RaiseCanExecuteChanged();
                    },
                    () =>
                        {
                            var canExecute =
                                    FilteredByFaction ||
                                    FilteredByType ||
                                    CostFilter < MAX_COSTS_FILTER ||
                                    FilterByDeck ||
                                    TraitFilter != null ||
                                    RarityFilter != 0 ||
                                    (CardSetFilter != null && CardSetFilter != _CARD_SET_FILTER_DEFAULT) ||
                                    !string.IsNullOrEmpty(SearchText);

                            ResetFilterIcon = canExecute ? "IconFilterReset.png" : "";
                            ResetFilterText = canExecute ? "Reset Filters" : "";

                            return canExecute;
                        }
                    ));
            }
        }

        private RelayCommand<Card> _findSimilar;

        /// <summary>
        /// Gets the FindSimilarCommand.
        /// </summary>
        public RelayCommand<Card> FindSimilarCommand
        {
            get
            {
                return _findSimilar
                    ?? (_findSimilar = new RelayCommand<Card>(
                    p =>
                    {
                        if (p.Traits.Count() > 0)
                        {
                            //var flags = (Traits)p.Traits.Aggregate((i, t) => i | t);
                            TraitFilter = p.Traits;
                            ApplyFilters();
                        }

                    }));
            }
        }


        private RelayCommand _saveDeck;

        /// <summary>
        /// Gets the SaveDeckCommand.
        /// </summary>
        public RelayCommand SaveDeckCommand
        {
            get
            {
                return _saveDeck
                    ?? (_saveDeck = new RelayCommand(
                    async () =>
                    {
                        if (CurrentDeck.Type == DeckType.NORMAL_DECK || CurrentDeck.Type == DeckType.GENERATED_DECK)
                        {
                            await _deckService.SaveDeckAsync(this.CurrentDeck);
                        }
                        else if (CurrentDeck.Type == DeckType.EXTERNAL_DECK)
                        {
                            //ask user if they want to save locally
                            await _dialog.ShowMessage("Save this deck locally?", "Save " + CurrentDeck.Name, "Save", "Discard", async (answer) =>
                            {
                                if (answer)
                                {
                                    var savedDeck = await _deckService.SaveDeckAsync(this.CurrentDeck);
                                    MessagingCenter.Send<Deck>(savedDeck, App.MESSAGES.NEW_DECK_SAVED);
                                }
                            });
                        }
                    }));
            }
        }

        private RelayCommand _nextCard;

        /// <summary>
        /// Gets the NextCardCommand.
        /// </summary>
        public RelayCommand NextCardCommand
        {
            get
            {
                return _nextCard
                    ?? (_nextCard = new RelayCommand(
                    () =>
                    {
                        if (FilteredCards == null || SelectedCard == null) return;
                        var selectedIndex = FilteredCards.IndexOf(SelectedCard);
                        if (FilteredCards.Count > selectedIndex + 1) SelectedCard = FilteredCards[selectedIndex + 1];
                    },
                    () =>
                    {
                        if (FilteredCards == null || SelectedCard == null) return false;
                        var selectedIndex = FilteredCards.IndexOf(SelectedCard);
                        return FilteredCards.Count > selectedIndex + 1;
                    }));
            }
        }

        private RelayCommand _PreviousCard;

        /// <summary>
        /// Gets the PreviousCardCommand.
        /// </summary>
        public RelayCommand PreviousCardCommand
        {
            get
            {
                return _PreviousCard
                    ?? (_PreviousCard = new RelayCommand(
                    () =>
                    {
                        if (FilteredCards == null || SelectedCard == null) return;
                        var selectedIndex = FilteredCards.IndexOf(SelectedCard);
                        if (selectedIndex != 0) SelectedCard = FilteredCards[selectedIndex - 1];
                    },
                    () =>
                    {
                        if (FilteredCards == null || SelectedCard == null) return false;
                        var selectedIndex = FilteredCards.IndexOf(SelectedCard);
                        return selectedIndex != 0;
                    }));
            }
        }
        #endregion

    }
}
