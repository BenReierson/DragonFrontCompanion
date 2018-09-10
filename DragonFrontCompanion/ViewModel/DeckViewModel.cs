
using DragonFrontDb;
using DragonFrontDb.Enums;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DragonFrontCompanion.Data;
using Xamarin.Forms;

namespace DragonFrontCompanion.ViewModel
{
    public class DeckViewModel :ViewModelBase
    {
        private IDeckService _deckService;
        private INavigationService _navigationService;
        private IDialogService _dialog;

        public DeckViewModel(IDeckService deckService, INavigationService navigationService, IDialogService dialog)
        {
            _deckService = deckService;
            _navigationService = navigationService;
            _dialog = dialog;
        }


        #region Properties

        private Deck _deck = null;
        public Deck CurrentDeck
        {
            get { return _deck; }
            set
            {
                SelectedCard = null;
                Set(ref _deck, value);
            }
        }


        private string _editButtonText = "";
        public string EditText
        {
            get { return _editButtonText; }
            set { Set(ref _editButtonText, value); }
        }


        private bool _editMode = false;
        public bool EditMode
        {
            get { return _editMode; }
            set { Set(ref _editMode, value); }
        }

        private Card _selectedCard = null;
        public Card SelectedCard
        {
            get { return _selectedCard; }
            set
            {
                Set(ref _selectedCard, value);
                PreviousCardCommand.RaiseCanExecuteChanged();
                NextCardCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _hasNavigated = false;
        public bool HasNavigated
        {
            get { return _hasNavigated; }
            set { Set(ref _hasNavigated, value); }
        }

        #endregion

        #region Commands
        private RelayCommand _editCards;

        /// <summary>
        /// Gets the EditDeckCardsCommand.
        /// </summary>
        public RelayCommand EditDeckCardsCommand
        {
            get
            {
                return _editCards
                    ?? (_editCards = new RelayCommand(
                    () =>
                    {
                        if (HasNavigated) return;
                        HasNavigated = true;
                        _navigationService.NavigateTo(ViewModelLocator.CardsPageKey, CurrentDeck);
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
                        if (CurrentDeck.Type == DeckType.NORMAL_DECK)
                        {
                            await _deckService.SaveDeckAsync(this.CurrentDeck);
                        }
                        else if (CurrentDeck.Type == DeckType.EXTERNAL_DECK)
                        {
                            //ask user if they want to save locally
                            await _dialog.ShowMessage("Save this deck?", "Save " + CurrentDeck.Name, "Save", "Discard", async (answer) =>
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

        private RelayCommand _editDeck;

        /// <summary>
        /// Gets the ToggleEditDeckDetailsCommand.
        /// </summary>
        public RelayCommand ToggleEditDeckDetailsCommand
        {
            get
            {
                return _editDeck
                    ?? (_editDeck = new RelayCommand(
                    async () =>
                    {
                        EditMode = !EditMode;
                        EditText = EditMode ? "Done" : "";
                        if (!EditMode) await _deckService.SaveDeckAsync(this.CurrentDeck);
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
                        if (this.CurrentDeck.CanUndo)
                        {
                            await _dialog.ShowMessage("Undo last change to this deck?", "Undo", "Undo", "Cancel", async (answer) =>
                            {
                                if (answer)
                                {
                                    var restoredDeck = await _deckService.UndoLastSave(this.CurrentDeck);
                                    if (restoredDeck != null)
                                    {
                                        this.CurrentDeck = restoredDeck;
                                        MessagingCenter.Send<string>("Undo Successful", App.MESSAGES.SHOW_TOAST);
                                    }
                                    else
                                    {
                                        MessagingCenter.Send<string>("Undo Failed", App.MESSAGES.SHOW_TOAST);
                                    }
                                }
                            });
                        }
                    }));
            }
        }

        private RelayCommand _shareDeck;

        /// <summary>
        /// Gets the ShareDeckCommand.
        /// </summary>
        public RelayCommand ShareDeckCommand
        {
            get
            {
                return _shareDeck
                    ?? (_shareDeck = new RelayCommand(
                    async () =>
                    {
                        var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
                        if (status != PermissionStatus.Granted)
                        {
                            var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Storage });
                            status = results[Permission.Storage];
                        }
                        if (status == PermissionStatus.Granted)
                        {
                            if (App.RuntimePlatform == App.Device.UWP)
                            {
                                var share = await _dialog.ShowMessage("Share deck, or export deck file?", "Share/Export", "Share", "Export", null);
                                if (share) MessagingCenter.Send<Deck>(CurrentDeck, App.MESSAGES.SHARE_DECK);
                                else MessagingCenter.Send<Deck>(CurrentDeck, App.MESSAGES.EXPORT_DECK);
                            }
                            else MessagingCenter.Send<Deck>(CurrentDeck, App.MESSAGES.SHARE_DECK);
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
                        if (SelectedCard == null || CurrentDeck == null ) return;
                        if (SelectedCard == CurrentDeck.Champion && CurrentDeck.DistinctFaction?.Count > 1)
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
                    },
                    () =>
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
                        if (SelectedCard == null || CurrentDeck == null || !CurrentDeck.Contains(SelectedCard)) return;
                        var cardList = SelectedCard.Faction == Faction.UNALIGNED ? CurrentDeck.DistinctUnaligned : CurrentDeck.DistinctFaction;
                        var selectedIndex = cardList.IndexOf(cardList.First(g => g.Card == SelectedCard));
                        if (selectedIndex != 0) SelectedCard = cardList[selectedIndex - 1].Card;
                        else if (SelectedCard.Faction == Faction.UNALIGNED && selectedIndex == 0 && CurrentDeck.DistinctFaction?.Count > 0)
                        {//move to faction list
                            SelectedCard = CurrentDeck.DistinctFaction.Last().Card;
                        }
                    },
                    () =>
                    {
                        if (SelectedCard == null || CurrentDeck == null || !CurrentDeck.Contains(SelectedCard)) return false;
                        var cardList = SelectedCard.Faction == Faction.UNALIGNED ? CurrentDeck.DistinctUnaligned : CurrentDeck.DistinctFaction;
                        var selectedIndex = cardList.IndexOf(cardList.First(g => g.Card == SelectedCard));
                        return selectedIndex != 0 ||
                               (SelectedCard.Faction == Faction.UNALIGNED && selectedIndex == 0 && CurrentDeck.DistinctFaction?.Count > 0);
                    }));
            }
        }
#endregion


    }
}
