using System.Windows.Input;
using DragonFrontCompanion.Data.Services;
using DragonFrontCompanion.Ioc;
using DragonFrontDb;
using FFImageLoading.Maui;

namespace DragonFrontCompanion.Controls;

public partial class CardDetail
{
    public static readonly BindableProperty CloseCommandProperty =
        BindableProperty.Create(nameof(CloseCommand), typeof(ICommand), typeof(CardDetail));

    public ICommand CloseCommand
    {
        get { return (ICommand)GetValue(CloseCommandProperty); }
        set { SetValue(CloseCommandProperty, value); }
    }

    public static readonly BindableProperty NextCommandProperty =
        BindableProperty.Create(nameof(NextCommand), typeof(ICommand), typeof(CardDetail), propertyChanged: OnNextCommandChanged);

    private static void OnNextCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var instance = bindable as CardDetail;
        if (instance == null || newValue == null) return;

        instance.NextCardButton.Command = (ICommand)newValue;
    }

    public ICommand NextCommand
    {
        get { return (ICommand)GetValue(NextCommandProperty); }
        set { SetValue(NextCommandProperty, value); }
    }

    public static readonly BindableProperty PreviousCommandProperty =
        BindableProperty.Create(nameof(PreviousCommand), typeof(ICommand), typeof(CardDetail), propertyChanged: OnPreviousCommandChanged);

    private static void OnPreviousCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var instance = bindable as CardDetail;
        if (instance == null || newValue == null) return;

        instance.PreviousCardButton.Command = (ICommand)newValue;
    }

    public ICommand PreviousCommand
    {
        get { return (ICommand)GetValue(PreviousCommandProperty); }
        set { SetValue(PreviousCommandProperty, value); }
    }

    public static readonly BindableProperty CardProperty = BindableProperty.Create(nameof(Card), typeof(Card), typeof(CardDetail));

    public Card Card
    {
        get { return (Card)GetValue(CardProperty); }
        set { SetValue(CardProperty, value); }
    }
    
    private object _Traits;
    public object Traits
    {
        get => _Traits;
        set
        {
            _Traits = value;
            OnPropertyChanged();
        }
    }

    async void OnShowTraits(object sender, EventArgs e)
    {
        var cardsTraits = await SimpleIoc.Default.GetInstance<ICardsService>().GetCardTraitsAsync();
        Traits = (from t in Card.Traits
                  select new { Trait = t.ToString().Trim('_').Replace('_',' '), Description = cardsTraits[t] })
                 .ToList();
    }

    void OnCloseTraits(object sender, EventArgs e)
        => Traits = null;

    public CardDetail()
    {
        InitializeComponent();
    }
}