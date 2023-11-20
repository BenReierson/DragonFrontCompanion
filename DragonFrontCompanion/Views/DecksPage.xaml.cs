
namespace DragonFrontCompanion.Views;

public partial class DecksPage
{
    public DecksPage()
    {
        InitializeComponent();
    }

    private void DeleteDeck_Clicked(object sender, EventArgs e)
    {
        var mi = ((MenuItem)sender);
        ViewModel.DeleteDeckCommand.Execute(mi.CommandParameter);
    }

    private void Champion_Tapped(object sender, ItemTappedEventArgs e)
    {
        ViewModel.OpenDeckCommand.Execute(e.Group);
    }

    private void Deck_Edit(object sender, EventArgs e)
    {
        ViewModel.OpenDeckCommand.Execute(((View)sender).BindingContext);
    }

    private async void Deck_ContextMenu(object sender, ItemTappedEventArgs e)
    {

        var result = await DisplayActionSheet(((Deck)e.Group).Name, "Cancel", null, new string[] { "Delete", "Duplicate", "Share" });

        if (result == "Delete")
        {
            ViewModel.DeleteDeckCommand.Execute(e.Group);
        }
        else if (result == "Duplicate")
        {
            ViewModel.DuplicateDeckCommand.Execute(e.Group);
        }
        else if (result == "Share")
        {
            ViewModel.ShareDeckCommand.Execute(e.Group);
        }
    }

    // protected override bool OnBackButtonPressed()
    // {
    //     if (SlideMenu.IsShown)
    //     {
    //         try { this.HideMenu(); } catch (Exception) { }
    //         return true;
    //     }
    //     else return base.OnBackButtonPressed();
    //
    // }
}