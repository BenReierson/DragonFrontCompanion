
using DragonFrontDb;

namespace DragonFrontCompanion.Views;

public partial class DeckPage 
{
    public DeckPage() 
    { 
        InitializeComponent();
    }

    private void DeckHeader_ChampionTapped(object sender, ItemTappedEventArgs e)
    {
        if (ViewModel.EditMode)
        {//close edit mode
            ViewModel.ToggleEditDeckDetailsCommand.Execute(null);
        }
        else if (e.Item != null)
        {//Show champion popup
            ViewModel.SelectedCard = e.Item as Card;
        }
        else
        {//no champion, go to edit 
            ViewModel.EditDeckCardsCommand.Execute(null);
        }
    }

    private void DeckHeader_EditModeToggleRequest(object sender, EventArgs e)
    {
        ViewModel.ToggleEditDeckDetailsCommand.Execute(null);
    }

}