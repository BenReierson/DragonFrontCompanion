
namespace DragonFrontCompanion.Controls;

public partial class DeckControl : ContentView
{
    public static readonly BindableProperty ContextMenuEnabledProperty = BindableProperty.Create(nameof(ContextMenuEnabled), typeof(bool), typeof(DeckControl), true, propertyChanged: OnContextMenuEnabledChanged);

    private static void OnContextMenuEnabledChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var instance = bindable as DeckControl;
        if (instance == null || newValue == null) return;


        instance.ContextMenu.IsVisible = (bool)newValue;// && App.RuntimePlatform != App.Device.Windows; //tap on this menu isn't working in uwp
        instance.ModifiedLabel.IsVisible = !(bool)newValue;
        instance.PriceLabel.IsVisible = !(bool)newValue;
    }

    public bool ContextMenuEnabled
    {
        get { return (bool)GetValue(ContextMenuEnabledProperty); }
        set {SetValue(ContextMenuEnabledProperty, value); }
    }

    public static readonly BindableProperty EditModeProperty = BindableProperty.Create(nameof(EditMode), typeof(bool), typeof(DeckControl), false, propertyChanged: OnEditModeChanged);

    private static void OnEditModeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var instance = bindable as DeckControl;

        if (instance == null) return;

        instance.NameLabel.IsVisible = !(bool)newValue;
        instance.DescriptionLabel.IsVisible = !(bool)newValue;
        instance.NameEntry.IsVisible = (bool)newValue;
        instance.DescriptionEntry.IsVisible = (bool)newValue;

        instance.StatsGrid.IsVisible = !(bool)newValue;

        if ((bool)newValue)
        {
            instance.NameEntry.Focus();
        }
    }

    public bool EditMode
    {
        get { return (bool)GetValue(EditModeProperty); }
        set { SetValue(EditModeProperty, value);}
    }

    public DeckControl()
    {
        InitializeComponent();
        // ContextImage.IsVisible = App.RuntimePlatform != App.Device.UWP;
    }

    public event EventHandler<ItemTappedEventArgs> ChampionTapped;

    private void Champion_Tapped(object sender, EventArgs e)
    {
        var deck = ((BindableObject)sender).BindingContext as Deck;
        ChampionTapped?.Invoke(this, new ItemTappedEventArgs(deck, deck?.Champion, 0));
    }

    public event EventHandler EditModeToggleRequest;

    private void EditModeToggle(object sender, EventArgs e)
    {
        EditModeToggleRequest?.Invoke(this, e);
    }

    public event EventHandler<ItemTappedEventArgs> ContextMenuTapped;

    private void OnContextMenuTapped(object sender, EventArgs e)
    {
        ContextMenuTapped?.Invoke(this, new ItemTappedEventArgs(BindingContext, e, 0));
    }
}