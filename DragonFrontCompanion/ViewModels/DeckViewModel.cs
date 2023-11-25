using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DragonFrontCompanion.Data;
using DragonFrontCompanion.Helpers;
using DragonFrontDb;
using DragonFrontDb.Enums;
using Microsoft.AppCenter.Analytics;
using Newtonsoft.Json;
using System.Text;

namespace DragonFrontCompanion.ViewModels;

public partial class DeckViewModel : BaseViewModel, IRequiresInitialize
{
    private readonly IDeckService _deckService;

    Deck _initDeck;

    public DeckViewModel(INavigationService navigationService, IDialogService dialogService, IDeckService deckService) : base(navigationService, dialogService)
    {
        _deckService = deckService;
    }

    public void Initialize(Deck deck)
    {
        Title = deck.Name;
        _initDeck = deck;

        IsInitialized = true;
    }

    public override async Task OnAppearing()
    {
        await base.OnAppearing();

        _deckService.DeckChanged += DeckService_DeckChanged;

        if (_initDeck != null && CurrentDeck is null)
        {
            IsBusy = true;
            await Task.Delay(300);
            CurrentDeck = _initDeck;
            _initDeck = null;
            IsBusy = false;
        }
        
        if (CurrentDeck.Count == 0 && 
            string.IsNullOrEmpty(CurrentDeck.FilePath) &&
            CurrentDeck.LastModified > DateTime.Now - TimeSpan.FromSeconds(5))
        {//this deck was just created, automatically start in edit mode
            await ToggleEditDeckDetails();
        }

        SetDeckCode();
    }

    public override async Task OnDisappearing()
    {
        _deckService.DeckChanged -= DeckService_DeckChanged;

        if (EditMode) await ToggleEditDeckDetails();
        else await SaveDeck();
        
        await base.OnDisappearing();
    }

    void SetDeckCode()
        => MainThread.BeginInvokeOnMainThread(()=>
            QrDeckCode = CurrentDeck?.IsValid ?? false ? App.GetApplinkFromDeckCode(_deckService.SerializeToDeckString(CurrentDeck)) : null);

    void DeckService_DeckChanged(object sender, Deck changedDeck)
    {
        if (changedDeck == CurrentDeck)
            SetDeckCode();
    }

    #region Properties
    public bool IsInitialized { get; private set; }

    [ObservableProperty] private string _editText;
    [ObservableProperty] private bool _editMode;
    [ObservableProperty] private string _qrDeckCode;
    [ObservableProperty] private string _largeQrDeckCode;

    private Deck _currentDeck;
    public Deck CurrentDeck
    {
        get => _currentDeck;
        set
        {
            SetProperty(ref _currentDeck, value);
            SetDeckCode();
        }
    }

    private Card _selectedCard = null;
    public Card SelectedCard
    {
        get => _selectedCard;
        set
        {
            SetProperty(ref _selectedCard, value);
            PreviousCardCommand.NotifyCanExecuteChanged();
            NextCardCommand.NotifyCanExecuteChanged();
        }
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task EditDeckCards()
    {
        await _navigationService.Push<CardsViewModel>(vm=>vm.Initialize(CurrentDeck));
    }

    [RelayCommand]
    private async Task EditDeckUnalignedCards()
    {
        await _navigationService.Push<CardsViewModel>(vm => vm.Initialize(CurrentDeck, factionFilter: Faction.UNALIGNED));
    }

    [RelayCommand]
    private async Task EditDeckFactionCards()
    {
        await _navigationService.Push<CardsViewModel>(vm => vm.Initialize(CurrentDeck, factionFilter: CurrentDeck.DeckFaction));
    }

    [RelayCommand]
    private async Task SaveDeck()
    {
        try
        {
            if (CurrentDeck is null) return;

            if (CurrentDeck.Type == DeckType.NORMAL_DECK)
            {
                await _deckService.SaveDeckAsync(CurrentDeck);
            }
            else if (CurrentDeck.Type == DeckType.EXTERNAL_DECK)
            {
                //ask user if they want to save locally
                var answer = await _dialogService.ShowMessage("Save this deck?", "Save " + CurrentDeck.Name, "Save", "Discard");
                if (answer)
                {
                    var savedDeck = await _deckService.SaveDeckAsync(CurrentDeck);
                }
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowError(ex, "Error", "OK");
        }
    }

    [RelayCommand]
    public async Task ToggleEditDeckDetails()
    {
        EditMode = !EditMode;
        EditText = EditMode ? "Save" : "";
        if (!EditMode) await _deckService.SaveDeckAsync(CurrentDeck);
    }

    [RelayCommand]
    private async Task Undo()
    {
        try
        {
            if (CurrentDeck is null) return;

            if (CurrentDeck.CanUndo)
            {
                var answer = await _dialogService.ShowMessage("Undo last change to this deck?", "Undo", "Undo", "Cancel");
                if (answer)
                {
                    Analytics.TrackEvent("Undo");

                    var restoredDeck = await _deckService.UndoLastSave(CurrentDeck);
                    if (restoredDeck != null)
                    {
                        CurrentDeck = restoredDeck;
                        Toast.Make("Undo Successful").Show();
                    }
                    else
                    {
                        Toast.Make("Undo Failed").Show();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowError(ex, "Error", "OK");
        }
    }

    [RelayCommand]
    private async Task ShareDeck()
    {
        try
        {
            if (CurrentDeck is null) return;

            var deck = CurrentDeck;

            if (String.IsNullOrWhiteSpace(deck?.FilePath))
            {
                await _dialogService.ShowMessage("Can't share a deck that hasn't been saved.", "Error");
                return;
            }

            var copy = "Share Deck Code";
            var qrCode = "QR Code";
            var file = "Share as file (retains Name/Description)";
            var choice = await _dialogService.DisplayActionSheet("Choose Format", "Cancel", null, copy, qrCode, file);

            if (choice == copy)
            {
                var deckstring = _deckService.SerializeToDeckString(deck);
                await Share.RequestAsync(deckstring);
                Analytics.TrackEvent("ShareDeckString", new Dictionary<string, string> {
                    { "Faction", deck.DeckFaction.ToString()},
                    { "DeckString", deckstring }});

                return;
            }

            if (choice == qrCode)
                ShowLargeQrDeckCode();

            if (choice == file)
            {
                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    var deckJson = JsonConvert.SerializeObject(deck, Formatting.Indented);
                    await FileSaver.SaveAsync(deck.Name + ".dfd", new MemoryStream(Encoding.UTF8.GetBytes(deckJson)));
                }
                else
                    await Share.RequestAsync(new ShareFileRequest
                    {
                        Title = "Share " + deck.Name,
                        File = new ShareFile(deck.FilePath, "application/dfd")
                    });

                Analytics.TrackEvent("ShareDeckFile", new Dictionary<string, string> {
                    { "Faction", deck.DeckFaction.ToString()}});
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowError(ex, "Error", "OK");
        }
    }

    [RelayCommand]
    private void ShowLargeQrDeckCode()
    {
        var deckCode = _deckService.SerializeToDeckString(CurrentDeck);
        LargeQrDeckCode = App.GetApplinkFromDeckCode(deckCode);

        Analytics.TrackEvent("ShareDeckQrCode", new Dictionary<string, string> {
                    { "Faction", CurrentDeck.DeckFaction.ToString()},
                    { "DeckString", deckCode }});
    }

    [RelayCommand]
    private void CloseQrDeckCode()
        => LargeQrDeckCode = null;

    [RelayCommand]
    private void SelectCard(Card card)
        => SelectedCard = card;

    [RelayCommand()]
    private void CloseCard()
        => SelectedCard = null;

    [RelayCommand(CanExecute = nameof(CanNextCard))]
    private async Task NextCard()
    {
        if (SelectedCard is null || CurrentDeck is null ) return;
        if (Equals(SelectedCard, CurrentDeck.Champion) && CurrentDeck.DistinctFaction?.Count > 1)
        {
            SelectedCard = CurrentDeck.DistinctFaction[1].Card;
        }
        else
        {
            var cardList = SelectedCard.Faction == Faction.UNALIGNED ? CurrentDeck.DistinctUnaligned : CurrentDeck.DistinctFaction;
            var group = cardList.FirstOrDefault(g => g.Card == SelectedCard);
            if (group != null)
            {
                var selectedIndex = cardList.IndexOf(group);
                if (cardList.Count > selectedIndex + 1) SelectedCard = cardList[selectedIndex + 1].Card;
                else if (SelectedCard.Faction != Faction.UNALIGNED && selectedIndex == cardList.Count - 1 && CurrentDeck.DistinctUnaligned?.Count > 0)
                {//move to unaligned list
                    SelectedCard = CurrentDeck.DistinctUnaligned[0].Card;
                }
            }
        }
    }

    private bool CanNextCard()
    {
        if (SelectedCard == null || CurrentDeck == null ) return false;
        if (SelectedCard == CurrentDeck.Champion && CurrentDeck.DistinctFaction?.Count > 1) return true;
        var cardList = SelectedCard.Faction == Faction.UNALIGNED ? CurrentDeck.DistinctUnaligned : CurrentDeck.DistinctFaction;
        var group = cardList.FirstOrDefault(g => g.Card == SelectedCard);
        if (group != null)
        {
            var selectedIndex = cardList.IndexOf(group);

            return cardList.Count > selectedIndex + 1 || 
                   (SelectedCard.Faction != Faction.UNALIGNED && selectedIndex == cardList.Count - 1 && CurrentDeck.DistinctUnaligned?.Count > 0);
        }
        return false;
    }

    [RelayCommand(CanExecute = nameof(CanPreviousCard))]
    private async Task PreviousCard()
    {
        if (SelectedCard == null || CurrentDeck == null || !CurrentDeck.Contains(SelectedCard)) return;
        var cardList = SelectedCard.Faction == Faction.UNALIGNED ? CurrentDeck.DistinctUnaligned : CurrentDeck.DistinctFaction;
        var selectedIndex = cardList.IndexOf(cardList.First(g => g.Card == SelectedCard));
        if (selectedIndex != 0) SelectedCard = cardList[selectedIndex - 1].Card;
        else if (SelectedCard.Faction == Faction.UNALIGNED && selectedIndex == 0 && CurrentDeck.DistinctFaction?.Count > 0)
        {//move to faction list
            SelectedCard = CurrentDeck.DistinctFaction.Last().Card;
        }
    }

    private bool CanPreviousCard()
    {
        if (SelectedCard == null || CurrentDeck == null || !CurrentDeck.Contains(SelectedCard)) return false;
        var cardList = SelectedCard.Faction == Faction.UNALIGNED ? CurrentDeck.DistinctUnaligned : CurrentDeck.DistinctFaction;
        var selectedIndex = cardList.IndexOf(cardList.First(g => g.Card == SelectedCard));
        return selectedIndex != 0 ||
               (SelectedCard.Faction == Faction.UNALIGNED && selectedIndex == 0 && CurrentDeck.DistinctFaction?.Count > 0);

    }

    #endregion
}