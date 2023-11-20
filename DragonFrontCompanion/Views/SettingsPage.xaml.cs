using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonFrontCompanion.Views;

public partial class SettingsPage 
{
    public SettingsPage()
    {
        InitializeComponent();
    }
    
    private void DataSource_Unfocused(object sender, FocusEventArgs e)
    {
        _=ViewModel.CheckForUpdate();
    }
    private void DataSource_Focused(object sender, FocusEventArgs e)
    {
        DataSourceEntry.SelectionLength = DataSourceEntry.Text?.Length ?? 0;
    }
}