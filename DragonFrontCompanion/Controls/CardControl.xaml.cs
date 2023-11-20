using DragonFrontCompanion.Helpers;
using DragonFrontDb;

namespace DragonFrontCompanion.Controls;

public partial class CardControl 
{
    CardImageConverter cardImages = new CardImageConverter();
    ImageSourceConverter imageSourceConverter = new ImageSourceConverter();

    public CardControl()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (BindingContext is Card card)
            CardCachedImage.Source = imageSourceConverter.ConvertFromString(cardImages.Convert(card, null, null, default) as string) as ImageSource;
        else
            CardCachedImage.Source = CardCachedImage.ErrorPlaceholder;
    }

    void CachedImage_Error(System.Object sender, FFImageLoading.Maui.CachedImageEvents.ErrorEventArgs e)
    {
        CardCachedImage.Source = CardCachedImage.ErrorPlaceholder;
    }
}