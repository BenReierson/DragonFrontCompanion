using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DragonFrontCompanion.Data;
using DragonFrontCompanion.Data.Services;
using DragonFrontCompanion.Helpers;
using DragonFrontDb;
using DragonFrontDb.Enums;
using Microsoft.AppCenter.Analytics;

namespace DragonFrontCompanion.ViewModels;

public partial class CardsViewModel : BaseViewModel, IRequiresInitialize
{
    public const int MAX_COSTS_FILTER = Deck.MAX_DISTRIBUTION_LEVEL + 1;
    private static int FACTION_COUNT = Enum.GetNames(typeof(Faction)).Count() - 2;

    private readonly ICardsService _cardsService;
    private readonly IDeckService _deckService;

    private Deck _initDeck = null;
    private string _initSearchText = null;
    private Faction? _initFactionFilter = null;

    public CardsViewModel(INavigationService navigationService, ICardsService cardsService, IDeckService deckService, IDialogService dialogService) : base(navigationService, dialogService)
    {
        _cardsService = cardsService;
        _deckService = deckService;

        _cardsService.DataUpdated += (o, e) =>
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DeckInitialize(CurrentDeck);
            });
        };
    }

    public async Task Initialize(Deck deck = null, string searchText = null, Faction? factionFilter = null)
    {
        Title = "Cards";
        _initSearchText = searchText;
        _initFactionFilter = factionFilter;
        _initDeck = deck;

        await Task.Delay(100);

        IsInitialized = true;
    }

    public override async Task OnAppearing()
    {
        await base.OnAppearing();

        IsBusy = true;

        await Task.Delay(500);

        var freshCards = await _cardsService.GetAllCardsAsync();
        if (_unfilteredCards != freshCards)
        {
            _unfilteredCards = freshCards;
            OnPropertyChanged(nameof(CardSets));
            OnPropertyChanged(nameof(AegisFactionEnabled));
            OnPropertyChanged(nameof(NinthFactionEnabled));
            OnPropertyChanged(nameof(NinthFactionText));
            AllCards = _unfilteredCards;
        }

        _suspendFilters = _initSearchText != null;

        if (_initDeck == null || _initDeck != CurrentDeck) await DeckInitialize(_initDeck);

        if (_initSearchText != null)
        {
            _suspendFilters = false;
            if (CardSets.Contains(_initSearchText))
            {
                CardSetFilter = _initSearchText;
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Task.Delay(750);
                    await Toast.Make($"Set: {CardSetFilter}", ToastDuration.Long).Show();
                });
            }
            else SearchText = _initSearchText;
        }

        if (_initFactionFilter != null)
        {
            await FilterFaction(_initFactionFilter);
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(750);
                await Toast.Make($"{FactionFilter} Filter Applied", ToastDuration.Long).Show();
            });
        }

        _ = ApplyFilters();

        _initSearchText = null;
        _initDeck = null;

        IsBusy = false;
    }

    public override async Task OnDisappearing()
    {
        SelectedCard = null;

        if (CurrentDeck != null) await SaveDeck();

        await base.OnDisappearing();
    }

    private async Task ApplyFilters()
    {
        if (AllCards == null || _suspendFilters)
            return;

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

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            IsBusy = true;
            FilteredCards = filtered.ToList();

            IsBusy = false;
            UpdateStatus();

            ResetFiltersCommand.NotifyCanExecuteChanged();
        });
    }

    private void UpdateStatus()
    {
        Title = $"{(IsChooser ? (FilterByDeck ? "Deck" : "") : "Cards")} ({FilteredCards.Count})";

        if (IsChooser)
            DeckStatus = "Deck    " + CurrentDeck.Count + " / 30    " +
                         (CurrentDeck.Champion != null ? "1C | " : "0C | ") +
                         CurrentDeck.UnitCount + "U | " +
                         CurrentDeck.FortCount + "F | " +
                         CurrentDeck.SpellCount + "S    " +
                         (CurrentDeck.IsValid ? "Ready To Use" : "Not Ready");
    }


    private async Task DeckInitialize(Deck deck)
    {
        CurrentDeck = deck;
        IsChooser = CurrentDeck != null;
        Message = "";

        if (IsChooser)
        {
            //filter cards according to deck
            AllCards = _unfilteredCards.Where((c) => (c.ValidFactions.Contains(CurrentDeck.DeckFaction) && c.Rarity != Rarity.TOKEN));

            ChooserFilterText = "[Deck]";
            CanFilterByEclipse = CurrentDeck.DeckFaction == Faction.ECLIPSE;
            CanFilterByScales = CurrentDeck.DeckFaction == Faction.SCALES;
            CanFilterByStrife = CurrentDeck.DeckFaction == Faction.STRIFE;
            CanFilterByThorns = CurrentDeck.DeckFaction == Faction.THORNS;
            CanFilterBySilence = CurrentDeck.DeckFaction == Faction.SILENCE;
            CanFilterByEssence = CurrentDeck.DeckFaction == Faction.ESSENCE;
            CanFilterByDelirium = CurrentDeck.DeckFaction == Faction.DELIRIUM;
            CanFilterByAegis = CurrentDeck.DeckFaction == Faction.AEGIS;

            if (ResetFiltersCommand.CanExecute(null)) ResetFiltersCommand.Execute(null);
            else await ApplyFilters();

            OnPropertyChanged(nameof(CanFilterByEclipse));
            OnPropertyChanged(nameof(CanFilterByScales));
            OnPropertyChanged(nameof(CanFilterByStrife));
            OnPropertyChanged(nameof(CanFilterByThorns));
            OnPropertyChanged(nameof(CanFilterBySilence));
            OnPropertyChanged(nameof(CanFilterByEssence));
            OnPropertyChanged(nameof(CanFilterByDelirium));
            OnPropertyChanged(nameof(CanFilterByAegis));
            OnPropertyChanged(nameof(AegisFactionEnabled));
            OnPropertyChanged(nameof(NinthFactionEnabled));
            OnPropertyChanged(nameof(NinthFactionText));

        }
        else
        {
            AllCards = _unfilteredCards;
            FilteredCards = AllCards.ToList();

            CanFilterByEclipse = true;
            CanFilterByScales = true;
            CanFilterByStrife = true;
            CanFilterByThorns = true;
            CanFilterBySilence = true;
            CanFilterByEssence = true;
            CanFilterByDelirium = true;
            CanFilterByAegis = true;
            FilterByDeck = false;

            OnPropertyChanged(nameof(AegisFactionEnabled));
            OnPropertyChanged(nameof(NinthFactionEnabled));
            OnPropertyChanged(nameof(NinthFactionText));

            if (ResetFiltersCommand.CanExecute(null)) ResetFiltersCommand.Execute(null);
            else await ApplyFilters();
        }
    }

    #region Commands

    [RelayCommand]
    private void ToggleFilters()
        => IsFiltersVisible = !IsFiltersVisible;

    [RelayCommand(CanExecute = nameof(CanResetFilters))]
    private async Task ResetFilters()
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

        await ApplyFilters();
        ResetFiltersCommand.NotifyCanExecuteChanged();


        Analytics.TrackEvent("ResetFilters");
    }

    private bool CanResetFilters()
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

        ResetFilterIcon = canExecute ? "iconfilterreset.png" : "";
        ResetFilterText = canExecute ? "Clear" : "";

        return canExecute;
    }

    [RelayCommand(CanExecute = nameof(CanAddOneToDeck))]
    private void AddOneToDeck(Card card)
    {
        try
        {
            if (CurrentDeck.CountCard(card) == Deck.CARD_DUPLICATE_LIMIT)
            {
                Message = "Deck already contains " + Deck.CARD_DUPLICATE_LIMIT + " copies.";
            }
            else if (CurrentDeck.Champion != null && CurrentDeck.Champion.Equals(card))
            {
                Message = $"{card.Name} is already deck champion.";
                return;

            }
            else if (!CanAddOneToDeck(card))
            {
                Message = "Can't add to this deck.";
                return;
            }
            else
            {
                CurrentDeck?.Add(card);

                Message = card.Name + " Added";
                AddOneToDeckCommand.NotifyCanExecuteChanged();
                RemoveCardCommand.NotifyCanExecuteChanged();

                Analytics.TrackEvent("AddCard", new Dictionary<string, string> {
                    { "Card", card.ToString()},
                    { "Guid", card.Guid?.ToString() }});
            }
            _lastActionedCard = card;
            UpdateStatus();
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
    }

    private bool CanAddOneToDeck(Card card)
        => CurrentDeck != null && card != null &&
           (CurrentDeck.Champion == null || !CurrentDeck.Champion.Equals(card)) &&
           (CurrentDeck.CountCard(card) < Deck.CARD_DUPLICATE_LIMIT) &&
           (CurrentDeck.CanOverload || (card.Type == CardType.CHAMPION || CurrentDeck.Count < Deck.MAX_CARD_COUNT));

    [RelayCommand(CanExecute = nameof(CanRemoveCard))]
    private void RemoveCard(Card p)
    {
        try
        {
            if (!CanRemoveCard(p))
            {
                Message = "No cards to remove";
                return;
            }

            if (p.Type == CardType.CHAMPION && CurrentDeck.Champion.Equals(p)) CurrentDeck.Champion = null;
            else CurrentDeck.Remove(p);

            Message = p.Name + " Removed";
            AddOneToDeckCommand.NotifyCanExecuteChanged();
            RemoveCardCommand.NotifyCanExecuteChanged();
            _lastActionedCard = p;
            UpdateStatus();

            Analytics.TrackEvent("RemoveCard", new Dictionary<string, string> {
                    { "Card", p.ToString()},
                    { "Guid", p.Guid?.ToString() }});
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
    }

    private bool CanRemoveCard(Card p)
        => (CurrentDeck != null && CurrentDeck.Contains(p)) ||
           (p?.Type == CardType.CHAMPION && CurrentDeck?.Champion != null && CurrentDeck.Champion.Equals(p));

    [RelayCommand]
    private async Task DeckFilter()
    {
        if (!IsChooser) return;
        FilterByDeck = !FilterByDeck;
        ChooserFilterText = FilterByDeck ? "" : "[Deck]";

        if (FilterByDeck) Message = "Showing current deck cards";
        else Message = "Showing all valid cards";

        await ApplyFilters();

        Analytics.TrackEvent("FilterByDeck");
    }

    [RelayCommand]
    private async Task FilterFaction(object p)
    {
        FilteredByFaction = true;
        FactionFilter = (Faction)p;
        await ApplyFilters();

        Analytics.TrackEvent("FilterByFaction", new Dictionary<string, string> {
                    { "Faction", FactionFilter.ToString()}});
    }

    [RelayCommand]
    private async Task FilterCardType(CardType p)
    {
        FilteredByType = true;
        TypeFilter = p;
        await ApplyFilters();

        Analytics.TrackEvent("FilterByType", new Dictionary<string, string> {
                    { "Faction", TypeFilter.ToString()}});
    }

    [RelayCommand]
    private void FilterByRarity(Rarity p)
    {
        RarityFilter = (int)p;

        Analytics.TrackEvent("FilterByRarity", new Dictionary<string, string> {
                    { "Faction", RarityFilter.ToString()}});
    }
    
    [RelayCommand]
    private async Task ResetFactionFilters()
    {
        FilteredByFaction = false;
        FactionFilter = Faction.INVALID;
        await ApplyFilters();
    }
    
    [RelayCommand]
    private async Task ResetTypeFilter()
    {
        FilteredByType = false;
        TypeFilter = CardType.INVALID;
        await ApplyFilters();
    }
    
    [RelayCommand]
    private async Task FindSimilar(Card p)
    {
        if (p.Traits.Any())
        {
            //var flags = (Traits)p.Traits.Aggregate((i, t) => i | t);
            TraitFilter = p.Traits;
            await ApplyFilters();

            Analytics.TrackEvent("FindSimilar", new Dictionary<string, string> {
                    { "Faction", RarityFilter.ToString()}});
        }
    }
    
    [RelayCommand]
    private async Task SaveDeck()
    {
        if (CurrentDeck.Type == DeckType.NORMAL_DECK || CurrentDeck.Type == DeckType.GENERATED_DECK)
        {
            await _deckService.SaveDeckAsync(this.CurrentDeck);
        }
        else if (CurrentDeck.Type == DeckType.EXTERNAL_DECK)
        {
            //ask user if they want to save locally
            var answer = await _dialogService.ShowMessage("Save this deck locally?", "Save " + CurrentDeck.Name, "Save", "Discard");
            if (answer)
            {
                var savedDeck = await _deckService.SaveDeckAsync(this.CurrentDeck);

                Analytics.TrackEvent("SaveExternalDeck", new Dictionary<string, string> {
                    { "Type", savedDeck.DeckFaction.ToString()},
                    { "Deck", savedDeck.ToString() }});
            }
        }
    }
    
    [RelayCommand()]
    private void CloseCard()
    {
        SelectedCard = null;
    }

    [RelayCommand(CanExecute = nameof(CanNextCard))]
    private void NextCard()
    {  
        if (FilteredCards == null || SelectedCard == null) return;
        var selectedIndex = FilteredCards.IndexOf(SelectedCard);
        if (FilteredCards.Count > selectedIndex + 1) SelectedCard = FilteredCards[selectedIndex + 1];
    }

    private bool CanNextCard()
    {
        if (FilteredCards == null || SelectedCard == null) return false;
        var selectedIndex = FilteredCards.IndexOf(SelectedCard);
        return FilteredCards.Count > selectedIndex + 1;
    }
    
    [RelayCommand(CanExecute = nameof(CanPreviousCard))]
    private void PreviousCard()
    {
        if (FilteredCards == null || SelectedCard == null) return;
        var selectedIndex = FilteredCards.IndexOf(SelectedCard);
        if (selectedIndex != 0) SelectedCard = FilteredCards[selectedIndex - 1];
    }
    
    private bool CanPreviousCard()
    {
        if (FilteredCards == null || SelectedCard == null) return false;
        var selectedIndex = FilteredCards.IndexOf(SelectedCard);
        return selectedIndex != 0;
    }

    #endregion
        
    #region Properties
    private bool _suspendFilters = false;
    private Card _lastActionedCard = null;
    private ReadOnlyCollection<Card> _unfilteredCards = null;

    public bool IsInitialized { get; set; }

    [ObservableProperty] private bool _isFiltersVisible = false;
    [ObservableProperty] private IEnumerable<Card> _allCards = null;
    [ObservableProperty] private string _traitFilterDisplay = "";
    [ObservableProperty] private bool _canFilterByDelirium = true;
    [ObservableProperty] private bool _canFilterByEssence = true;
    [ObservableProperty] private bool _canFilterBySilence = true;
    [ObservableProperty] private bool _canFilterByThorns = true;
    [ObservableProperty] private bool _canFilterByStrife = true;
    [ObservableProperty] private bool _canFilterByScales = true;
    [ObservableProperty] private bool _canFilterByEclipse = true;
    [ObservableProperty] private bool _canFilterByAegis = true;
    [ObservableProperty] private string _factionFilterText = "Faction";
    [ObservableProperty] private bool _filteredByFaction = false;
    [ObservableProperty] private string _typeFilterText = "Type";
    [ObservableProperty] private bool _filteredByType = false;
    [ObservableProperty] private bool _filterByDeck = false;
    [ObservableProperty] private string _RarityFilterText = "All";
    [ObservableProperty] private string _costFilterText = "*";
    [ObservableProperty] private string _resetFilterText = "";
    [ObservableProperty] private string _resetFilterIcon = "";
    [ObservableProperty] private string _chooserFilterText = "";
    [ObservableProperty] private bool _isChooser = false;
    [ObservableProperty] private string _deckStatus = "";

    private Deck _currentDeck = null;
    public Deck CurrentDeck
    {
        get => _currentDeck;
        set 
        {
            SetProperty(ref _currentDeck, value);
            AddOneToDeckCommand.NotifyCanExecuteChanged();
            RemoveCardCommand.NotifyCanExecuteChanged();
        }
    }

    public bool AegisFactionEnabled => _unfilteredCards?.Any(c =>c.Faction == Faction.AEGIS) ?? false;
    public bool NinthFactionEnabled => _unfilteredCards?.Any(c =>c.Faction == Faction.NINTH) ?? false;
    public string NinthFactionText => Enum.TryParse("10", out Faction faction) ? faction.ToString() : "";

    private Card _selectedCard = null;
    public Card SelectedCard
    {
        get => _selectedCard;
        set
        {
            // if (App.RuntimePlatform == App.Device.UWP
            //     && _lastActionedCard != null && value == _lastActionedCard)
            // {
            //     _lastActionedCard = null;
            //     return;
            // }

            SetProperty(ref _selectedCard, value);
            PreviousCardCommand.NotifyCanExecuteChanged();
            NextCardCommand.NotifyCanExecuteChanged();
            if (value != null) _lastActionedCard = null;
        }
    }
    
    private List<Card> _filteredCards = null;
    public List<Card> FilteredCards
    {
        get => _filteredCards;
        set
        {
            SelectedCard = null;
            SetProperty(ref _filteredCards, value);
        }
    }
    
    private string _msg = "";
    public string Message
    {
        get => _msg;
        set
        {
            SetProperty(ref _msg, value);
            if (!String.IsNullOrWhiteSpace(_msg)) 
                Toast.Make(value).Show();
        }
    }
    
    private int _costFilter = MAX_COSTS_FILTER;
    public int CostFilter
    {
        get => _costFilter;
        set
        {
            if (_costFilter == value) return;

            SetProperty(ref _costFilter, value);
            if (value == MAX_COSTS_FILTER) CostFilterText = "*";
            else if (value == MAX_COSTS_FILTER - 1) CostFilterText = (MAX_COSTS_FILTER - 1) + "+";
            else CostFilterText = value.ToString();
            ApplyFilters();
        }
    }
    
    private int _RarityFilter = 0;
    public int RarityFilter
    {
        get => _RarityFilter;
        set
        {
            if (_RarityFilter == value) return;

            SetProperty(ref _RarityFilter, value);
            if (value == 0) RarityFilterText = "All";
            else RarityFilterText = ((Rarity)value).ToString();
            ApplyFilters();
        }
    }
    
    private CardType _typeFilter = CardType.INVALID;
    public CardType TypeFilter
    {
        get => _typeFilter;
        private set
        {
            SetProperty(ref _typeFilter, value);
            TypeFilterText = _typeFilter == CardType.INVALID ? "ALL" : _typeFilter.ToString();
        }
    }
    
    private Faction _factionFilter = Faction.INVALID;
    public Faction FactionFilter
    {
        get => _factionFilter;
        set
        {
            SetProperty(ref _factionFilter, value);
            FactionFilterText = _factionFilter == Faction.INVALID ? "ALL" : _factionFilter.ToString();
        }
    }
    
    private List<string> _traitFilter;
    public List<string> TraitFilter
    {
        get => _traitFilter;
        private set
        {
            SetProperty(ref _traitFilter, value);
            if (value != null && value.Count() > 0)
            {
                var display = new StringBuilder("Traits: ");
                foreach (var trait in value)
                {
                    display.Append(trait.ToString().Trim('_').Replace('_', ' '));
                    display.Append(", ");
                }
                TraitFilterDisplay = display.ToString().Trim().Trim(',');
                Toast.Make(TraitFilterDisplay).Show();
            }
            else TraitFilterDisplay = "";
        }
    }
    
    private List<string> _cardSets;
    public List<string> CardSets
    {
        get
        {
            if (_unfilteredCards is null) return null;
            if (_cardSets != null) return _cardSets;
            else
            {
                _cardSets = Enum.GetValues(typeof(CardSet)).Cast<CardSet>().Skip(2).AsQueryable()
                   .Where(cs => _unfilteredCards.Any(c => c.CardSet == cs))
                   .Select(cs => cs.ToString()).ToList();
                _cardSets.Insert(0, _CARD_SET_FILTER_DEFAULT);
                CardSetFilter = _CARD_SET_FILTER_DEFAULT;
                return _cardSets;
            }
        }
    }
    
    private const string _CARD_SET_FILTER_DEFAULT = "ALL";
    private string _cardSetFilter;
    public string CardSetFilter
    {
        get => _cardSetFilter;
        set
        {
            SetProperty(ref _cardSetFilter, value);
            ApplyFilters();
        }
    }
    
    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    var startingText = _searchText;
                    await Task.Delay(750);//wait for more input
                    if (startingText == _searchText)
                        await ApplyFilters();
                });
            }
        }
    }
    #endregion

}