using DragonFrontDb;

namespace DragonFrontCompanion.Views;

public partial class CardsPage
{
    public CardsPage()
    {
        InitializeComponent();;
    }

    protected override bool OnBackButtonPressed()
    {
        if (ViewModel.IsFiltersVisible)
        {
            ViewModel.ToggleFiltersCommand.Execute(null);
            return true;
        }

        return base.OnBackButtonPressed();

    }

    void CardsList_SelectionChanged(System.Object sender, Microsoft.Maui.Controls.SelectionChangedEventArgs e)
    {
        if (ViewModel?.SelectedCard is not null)
            CardsList.ScrollTo(ViewModel.SelectedCard);
    }
}