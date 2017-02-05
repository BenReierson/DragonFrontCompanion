using DragonFrontDb;
using DragonFrontDb.Enums;
using FFImageLoading;
using FFImageLoading.Cache;
using Rg.Plugins.Popup.Animations;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Interfaces.Animations;
using Rg.Plugins.Popup.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace DragonFrontCompanion.Views
{
    public partial class CardPopup : PopupPage
    {

        public static readonly BindableProperty NextCommandProperty = 
            BindableProperty.Create(nameof(NextCommand), typeof(ICommand), typeof(CardPopup), propertyChanged: OnNextCommandChanged);

        private static void OnNextCommandChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var instance = bindable as CardPopup;
            if (instance == null || newValue == null) return;

            instance.NextCardButton.Command = (ICommand)newValue;
        }

        public ICommand NextCommand
        {
            get { return (ICommand)GetValue(NextCommandProperty); }
            set { SetValue(NextCommandProperty, value); }
        }

        public static readonly BindableProperty PreviousCommandProperty =
            BindableProperty.Create(nameof(PreviousCommand), typeof(ICommand), typeof(CardPopup), propertyChanged: OnPreviousCommandChanged);

        private static void OnPreviousCommandChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var instance = bindable as CardPopup;
            if (instance == null || newValue == null) return;

            instance.PreviousCardButton.Command = (ICommand)newValue;
        }

        public ICommand PreviousCommand
        {
            get { return (ICommand)GetValue(PreviousCommandProperty); }
            set { SetValue(PreviousCommandProperty, value); }
        }


        public static readonly BindableProperty CardProperty = BindableProperty.Create(nameof(Card), typeof(Card), typeof(CardPopup), propertyChanged:OnCardChanged);

        public Card Card
        {
            get { return (Card)GetValue(CardProperty); }
            set { SetValue(CardProperty, value); }
        }
        

        private static void OnCardChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var instance = bindable as CardPopup;
            if (instance == null || newValue == null) return;

            if (instance.Card != null)
            {
                instance.ChangeCardAsync(newValue as Card);
            }
        }

        public CardPopup()
        {
            InitializeComponent();

            if (Device.OS == TargetPlatform.Windows || Device.OS == TargetPlatform.WinPhone)
            {
                FrameContainer.Margin = 0;
            }
        }

        protected async Task ChangeCardAsync(Card newCard)
        {
            if (newCard == null) CloseAllPopup();
            else
            {
                if (IsOpen)
                {
                    if (newCard == FrameContainer.BindingContext) return;

                    await Navigation.PopAllPopupAsync();
                    await ImageService.Instance.InvalidateCacheEntryAsync(CardImage.Source.ToString(), CacheType.Memory);
                }

                FrameContainer.BindingContext = newCard;

                await Navigation.PushPopupAsync(this);
                IsOpen = true;
            }
        } 


        private bool _isOpen = false;
        public bool IsOpen
        {
            get { return _isOpen; }
            set { _isOpen = value; OnPropertyChanged(); }
        }

        protected override bool OnBackButtonPressed()
        {
            CloseAllPopup();
            return false;
        }

        protected override bool OnBackgroundClicked()
        {
            CloseAllPopup();
            return false;
        }

        private void CloseButton_Clicked(object sender, EventArgs e)
        {
            CloseAllPopup();
        }

        private async void CloseAllPopup()
        {

            Animation = Resources["OpenCloseAnimation"] as IPopupAnimation;
            await Navigation.PopAllPopupAsync();
            ImageService.Instance.InvalidateCacheEntryAsync(CardImage.Source.ToString(), CacheType.Memory);

            IsOpen = false;
        }

        private void PreviousCardButton_Clicked(object sender, EventArgs e)
        {
            Animation = Resources["PreviousAnimation"] as IPopupAnimation;
        }

        private void NextCardButton_Clicked(object sender, EventArgs e)
        {
            Animation = Resources["NextAnimation"] as IPopupAnimation;
        }

        private void OnTraitsTapped(object sender, EventArgs e)
        {
            var traits = from t in ((Card)FrameContainer.BindingContext).Traits
                         select new { Trait = t.ToString().Trim('_').Replace('_',' '), Description = Cards.TraitsDictionary[t] };

            Navigation.PushPopupAsync(new TraitsPopup() { BindingContext = traits});
        }
    }
}
