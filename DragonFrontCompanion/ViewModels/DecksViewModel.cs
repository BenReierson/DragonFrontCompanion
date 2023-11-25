using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DragonFrontCompanion.Data;
using DragonFrontCompanion.Helpers;
using DragonFrontDb.Enums;
using Microsoft.AppCenter.Analytics;

namespace DragonFrontCompanion.ViewModels;

public partial class DecksViewModel : BaseViewModel
{
    private readonly IDeckService _deckService;

    public DecksViewModel(INavigationService navigationService, IDialogService dialogService, IDeckService deckService) : base(navigationService, dialogService)
    {
        Title = "Decks";
        
        _deckService = deckService;
        _deckService.DeckChanged += (sender, deck) => MainThread.BeginInvokeOnMainThread(() => _=RefreshDecks(false));
        Decks.CollectionChanged += (sender, args) => 
            Title = Decks?.Any() ?? false ? "Decks (" + Decks.Count + ")" : "Decks";
    }
    
    public static byte[] ReadAllBytes(Stream instream)
    {
        if (instream is MemoryStream)
            return ((MemoryStream) instream).ToArray();

        using (var memoryStream = new MemoryStream())
        {
            instream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }

    public override async Task OnAppearing()
    {
        await base.OnAppearing();
        _= RefreshDecks(!Decks.Any());
    }

    #region Properties

    [ObservableProperty] private bool _showBusy;
    [ObservableProperty] private bool _canUndo;
    [ObservableProperty] private bool _isFactionPickerVisible;
    [ObservableProperty] private ObservableCollection<Deck> _decks = new ObservableCollection<Deck>();
    [ObservableProperty] private string _qrDeckCode;
    [ObservableProperty] private bool _isScanningForQrCode;

    public bool AegisFactionEnabled => Deck.CardDictionary != null && Deck.CardDictionary.Any(c => c.Value != null && c.Value.Faction == Faction.AEGIS);
    public bool NinthFactionEnabled => Deck.CardDictionary != null && Deck.CardDictionary.Any(c => c.Value != null && (int)c.Value.Faction == 10);
    public string NinthFactionText => Enum.TryParse("10", out Faction faction) ? faction.ToString() : "";
    
    #endregion

    #region  Commands

    [RelayCommand]
    private async Task OpenFile()
    {
        try
        {
            var fromFile = "From (dfd) file";
            var fromClipboard = "From clipboard";
            var fromQrCode = "Scan QR Code";

            var clipboardContents = await Clipboard.GetTextAsync();
            var mightBeDeck = !string.IsNullOrWhiteSpace(clipboardContents)
                && (clipboardContents.StartsWith(App.AppDataScheme) || Convert.TryFromBase64String(clipboardContents, new Span<byte>(new byte[clipboardContents.Length]), out _));

            var options = new List<string> { fromFile, fromQrCode };
            if (mightBeDeck) options.Add(fromClipboard);
            var choice = await _dialogService.DisplayActionSheet("Choose Deck Source", "Cancel", null, options.ToArray());

            if (choice == fromFile)
            {
                var file = await FilePicker.PickAsync();
                if (file != null)
                {
                    var fileStream = await file.OpenReadAsync();
                    var fileBytes = ReadAllBytes(fileStream);
                    var deck = await _deckService.OpenDeckDataAsync(Encoding.UTF8.GetString(fileBytes, 0, fileBytes.Length));

                    Analytics.TrackEvent("OpenDeckFile", new Dictionary<string, string> {
                    { "Faction", deck.DeckFaction.ToString()}});

                    if (DeviceInfo.Platform == DevicePlatform.Android)
                    {//Android seems to need a moment to recover from file picker
                        _=Toast.Make("Opening Deck...").Show();
                        await Task.Delay(1000);
                    }

                    await _navigationService.Push<DeckViewModel>(vm => vm.Initialize(deck));
                }
            }
            else if (choice == fromClipboard)
            {
                await OpenDeckUrl(clipboardContents);
            }
            else if (choice == fromQrCode)
            {
                IsScanningForQrCode = true;
            }
        }
        catch (ArgumentException ex)
        {
            await _dialogService.ShowError(ex.Message, "Error", "OK");
        }
        catch (Exception)
        {
            await _dialogService.ShowMessage("Could not open deck. Data may be invalid or corrupt.", "Error");
        }
    }

    public async Task OpenDeckUrl(string url)
    {
        try
        {
            if (url.StartsWith(App.AppDataScheme) && url.Contains(App.AppDeckCodeHost))
                url = new Uri(url).Segments?.LastOrDefault();

            var deck = _deckService.DeserializeDeckString(url);

            Analytics.TrackEvent("OpenDeckString", new Dictionary<string, string> {
                    { "Faction", deck.DeckFaction.ToString()},
                    { "DeckString", url }});

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                _ = Toast.Make("Opening Deck...").Show();
                await _navigationService.Push<DeckViewModel>(vm => vm.Initialize(deck));

            });
        }
        catch (Exception)
        {
            await _dialogService.ShowMessage("Could not open deck. Data may be invalid or corrupt.", "Error");
        }
    }

    [RelayCommand]
    private async Task OpenDeck(Deck p)
    {
        await _navigationService.Push<DeckViewModel>(vm => vm.Initialize(p));
    }

    [RelayCommand]
    private async Task DeleteDeck(Deck p)
    {
        try
        {
            ShowBusy = true; IsBusy = true;
            Decks.Remove(p);
            await _deckService.DeleteDeckAsync(p);
            CanUndo = _deckService.DeckRestoreAvailable;
            ShowBusy = false; IsBusy = false;
            Analytics.TrackEvent("DeleteDeck");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowError(ex, "Error", "OK");
        }
    }

    [RelayCommand]
    private async Task Undo()
    {
        try
        {
            if (!_deckService.DeckRestoreAvailable) { CanUndo = false; return; }

            ShowBusy = true; IsBusy = true;
            var restoredDeck = await _deckService.RestoreLastDeletedDeck(true);
            if (restoredDeck != null)
            {
                Decks.Insert(0, restoredDeck);
                _=Toast.Make("Deck Restored").Show();
            }
            else
            {
                _=Toast.Make("Deck Restore Failed").Show();
            }
            CanUndo = _deckService.DeckRestoreAvailable;

            Analytics.TrackEvent("Undo");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowError(ex, "Error", "OK");
        }
        finally
        {
            ShowBusy = false; IsBusy = false;
        }
    }

    [RelayCommand]
    private void NewDeck() => IsFactionPickerVisible = true;
    
    [RelayCommand]
    private void CancelNewDeck() => IsFactionPickerVisible = false;

    [RelayCommand]
    private async Task ChooseFaction(Faction p)
    {
        try
        {
            IsFactionPickerVisible = false;
        
            //create a new deck with chosen faction
            var deck = new Deck(p, App.VersionName) { Name = "New " + p.ToString() + " Deck",
            Description = "(description)"};
            Decks.Insert(0, deck);

            Analytics.TrackEvent("NewDeck", new Dictionary<string, string> {
                    { "Faction", deck.DeckFaction.ToString()}});

            //open deck
            await _navigationService.Push<DeckViewModel>(vm => vm.Initialize(deck));
        }
        catch (Exception ex)
        {
            await _dialogService.ShowError(ex, "Error", "OK");
        }
    }

    [RelayCommand]
    private async Task RefreshDecks(bool p)
    {
        try
        {
            if (p)
            {
                ShowBusy = true;
                IsBusy = true;
            }

            var freshDecks = await _deckService.GetSavedDecksAsync();
            if (!Decks.SequenceEqual(freshDecks))
            {
                Decks.Clear();
                foreach (var d in freshDecks)
                    Decks.Add(d);
            }

            CanUndo = _deckService.DeckRestoreAvailable;

            if (!Decks.Any())
            {//trigger new deck dialog if no decks exist
                _=Toast.Make("Pick a faction for your first deck, or select 'Open' to import an existing deck.").Show();
                NewDeck();
            }
        }
        catch (Exception)
        {
            await _dialogService.ShowMessage("Could not load decks. Data may be invalid or corrupt. You may need to delete the app and re-install.", "Error");
        }
        IsBusy = false;
        ShowBusy = false;
    }

    [RelayCommand]
    private async Task ShareDeck(Deck deck)
    {
        try
        {
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
            {
                var deckCode = _deckService.SerializeToDeckString(deck);
                QrDeckCode = App.GetApplinkFromDeckCode(deckCode);

                Analytics.TrackEvent("ShareDeckQrCode", new Dictionary<string, string> {
                    { "Faction", deck.DeckFaction.ToString()},
                    { "DeckString", deckCode }});
            }

            if (choice == file)
            {
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
    private void CloseQrDeckCode()
        => QrDeckCode = null;

    [RelayCommand]
    private async Task DuplicateDeck(Deck p)
    {

        try
        {
            var dupeDeck = new Deck(p.DeckFaction, App.VersionName)
            {
                Name = "COPY - " + p.Name,
                Description = p.Description,
                Champion = p.Champion,
                CanOverload = p.Count > Deck.MAX_CARD_COUNT
            };
            foreach (var c in p)
            {
                dupeDeck.Add(c);
            }
            Decks.Insert(0, dupeDeck);
            await _deckService.SaveDeckAsync(dupeDeck);
        }
        catch (Exception ex)
        {
            await _dialogService.ShowError(ex, "Error", "OK");
        }
    }

    [RelayCommand]
    private async Task ImportDeckFromClipboard()
    {
        try
        {
            var deckString = await Clipboard.GetTextAsync();
            if (string.IsNullOrWhiteSpace(deckString))
                throw new ArgumentException("Nothing found in clipboard. Copy a deckstring and try again.");

            var deck = _deckService.DeserializeDeckString(deckString);

            Analytics.TrackEvent("ImportDeckString", new Dictionary<string, string> {
                    { "Faction", deck.DeckFaction.ToString()},
                    { "DeckString", deckString }});

            await _navigationService.Push<DeckViewModel>(vm => vm.Initialize(deck));
        }
        catch (Exception ex)
        {
            await _dialogService.ShowError(ex, "Error", "OK");
        }
    }

    #endregion
}