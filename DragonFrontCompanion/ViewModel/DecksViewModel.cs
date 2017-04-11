using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using System.Collections.ObjectModel;
using DragonFrontCompanion.Data;
using GalaSoft.MvvmLight.Command;
using DragonFrontDb.Enums;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms;

namespace DragonFrontCompanion.ViewModel
{
    public class DecksViewModel : ViewModelBase
    {
        private IDeckService _deckService;
        private INavigationService _navigationService;
        private IDialogService _dialogService;

        public DecksViewModel(IDeckService deckService, INavigationService navigationService, IDialogService dialogService) 
        {
            _deckService = deckService;
            _navigationService = navigationService;
            _dialogService = dialogService;

            _decks.CollectionChanged += DecksChanged;

            MessagingCenter.Subscribe<Deck>(this, App.MESSAGES.NEW_DECK_SAVED, (d) => RefreshDecksCommand.Execute(null));
        }

        #region Properties
        private ObservableCollection<Deck> _decks = new ObservableCollection<Deck>();
        public ObservableCollection<Deck> Decks
        {
            get { return _decks; }
            set
            {
                bool changed = _decks.Count != value.Count;
                if (!changed)
                {
                    foreach (var deck in value)
                    {
                        changed = !_decks.Contains(deck);
                        if (changed) break;
                    }
                }
                if (changed)
                {
                    _decks.Clear();
                    foreach (var deck in value)
                    {
                        _decks.Add(deck);
                    }
                    RaisePropertyChanged();
                }
            }
        }

        private void DecksChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(Title));
        }

        private bool _busy = false;
        public bool IsBusy
        {
            get { return _busy; }
            set { Set(ref _busy, value); }
        }


        private bool _showBusy = false;
        public bool ShowBusy
        {
            get { return _showBusy; }
            set { Set(ref _showBusy, value); }
        }


        private bool _canUndo = false;
        public bool CanUndo
        {
            get { return _canUndo; }
            set { Set(ref _canUndo, value); }
        }

        private bool _hasNavigated = false;
        public bool HasNavigated
        {
            get { return _hasNavigated; }
            set { Set(ref _hasNavigated, value); }
        }

        public string Title => Decks != null ? "Decks (" + Decks.Count + ")" : "Decks";
        #endregion

        #region Commands
        private RelayCommand<Deck> _openDeck;

        /// <summary>
        /// Gets the ToggleEditDeckDetailsCommand.
        /// </summary>
        public RelayCommand<Deck> OpenDeckCommand
        {
            get
            {
                return _openDeck
                    ?? (_openDeck = new RelayCommand<Deck>(
                    p =>
                    {
                        if (HasNavigated) return;
                        HasNavigated = true;
                        _navigationService.NavigateTo(ViewModelLocator.DeckPageKey, p);
                    }));
            }
        }

        private RelayCommand<Deck> _deleteDeck;

        /// <summary>
        /// Gets the DeleteDeckCommand.
        /// </summary>
        public RelayCommand<Deck> DeleteDeckCommand
        {
            get
            {
                return _deleteDeck
                    ?? (_deleteDeck = new RelayCommand<Deck>(
                    async p =>
                    {
                        ShowBusy = true; IsBusy = true;
                        Decks.Remove(p);
                        await _deckService.DeleteDeckAsync(p);
                        CanUndo = _deckService.DeckRestoreAvailable;
                        ShowBusy = false; IsBusy = false;
                    }));
            }
        }

        private RelayCommand _undo;

        /// <summary>
        /// Gets the UndoCommand.
        /// </summary>
        public RelayCommand UndoCommand
        {
            get
            {
                return _undo
                    ?? (_undo = new RelayCommand(
                    async () =>
                    {
                        if (!_deckService.DeckRestoreAvailable) { CanUndo = false; return; }

                        ShowBusy = true; IsBusy = true;
                        var restoredDeck = await _deckService.RestoreLastDeletedDeck(true);
                        if (restoredDeck != null)
                        {
                            Decks.Insert(0, restoredDeck);
                            MessagingCenter.Send<string>("Deck Restored", App.MESSAGES.SHOW_TOAST);
                        }
                        else
                        {
                            MessagingCenter.Send<string>("Deck Restore Failed", App.MESSAGES.SHOW_TOAST);
                        }
                        CanUndo = _deckService.DeckRestoreAvailable;
                        ShowBusy = false; IsBusy = false;
                    }));
            }
        }

        private RelayCommand<Faction> _chooseFaction;

        /// <summary>
        /// Gets the ChooseFactionCommand.
        /// </summary>
        public RelayCommand<Faction> ChooseFactionCommand
        {
            get
            {
                return _chooseFaction
                    ?? (_chooseFaction = new RelayCommand<Faction>(
                    p =>
                    {
                        if (HasNavigated) return;
                        HasNavigated = true;

                        //create a new deck with chosen faction
                        var deck = new Deck(p, App.VersionName) { Name = "New " + p.ToString() + " Deck" };
                        Decks.Insert(0, deck);
                        
                        //open deck
                        _navigationService.NavigateTo(ViewModelLocator.DeckPageKey, deck);
                    }));
            }
        }

        private RelayCommand<bool> _refresh;

        /// <summary>
        /// Gets the RefreshDecksCommand.
        /// </summary>
        public RelayCommand<bool> RefreshDecksCommand
        {
            get
            {
                return _refresh
                    ?? (_refresh = new RelayCommand<bool>(
                    async (p) =>
                    {
                        if (p) { ShowBusy = true; IsBusy = true; }
                        Decks = new ObservableCollection<Deck>(await _deckService.GetSavedDecksAsync());
                        CanUndo = _deckService.DeckRestoreAvailable;
                        IsBusy = false;
                        ShowBusy = false;
                    }));
            }
        }


        private RelayCommand<Deck> _shareDeck;

        /// <summary>
        /// Gets the ShareDeckCommand.
        /// </summary>
        public RelayCommand<Deck> ShareDeckCommand
        {
            get
            {
                return _shareDeck
                    ?? (_shareDeck = new RelayCommand<Deck>(
                    async p =>
                    {
                        var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
                        if (status != PermissionStatus.Granted)
                        {
                            var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Storage });
                            status = results[Permission.Storage];
                        }
                        if (status == PermissionStatus.Granted)
                        {
                            if (App.RuntimePlatform == App.Device.Windows)
                            {
                                var share = await _dialogService.ShowMessage("Share deck, or export deck file?", "Share/Export", "Share", "Export", null);
                                if (share) MessagingCenter.Send<Deck>(p, App.MESSAGES.SHARE_DECK);
                                else MessagingCenter.Send<Deck>(p, App.MESSAGES.EXPORT_DECK);
                            }
                            else MessagingCenter.Send<Deck>(p, App.MESSAGES.SHARE_DECK);
                        }
                    }));
            }
        }

        private RelayCommand<Deck> _dupeDeck;

        /// <summary>
        /// Gets the DuplicateDeckCommand.
        /// </summary>
        public RelayCommand<Deck> DuplicateDeckCommand
        {
            get
            {
                return _dupeDeck
                    ?? (_dupeDeck = new RelayCommand<Deck>(
                    p =>
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
                        _deckService.SaveDeckAsync(dupeDeck);
                    }));
            }
        }
#endregion
    }
}
