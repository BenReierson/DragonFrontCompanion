using System.Reflection;
using DragonFrontCompanion.ViewModels;
namespace DragonFrontCompanion.Views;

public class BaseContentPage<T> : ContentPage, IViewFor<T> where T : BaseViewModel
{
    protected BaseContentPage()
    {
        if (ShouldAutoCreateVm())
        {
            BindingContext = NavigationService.CreateViewModelInstanceStatic<T>(unique: false);
        }
    }

    public T ViewModel
    {
        get => BindingContext as T;
        set { if (BindingContext != value) { BindingContext = value; OnPropertyChanged(); } }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ViewModel?.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        ViewModel?.OnDisappearing();
    }
    
    bool ShouldAutoCreateVm()
    {
        if (DesignMode.IsDesignModeEnabled) return true;

        var interfaces = typeof(T).GetTypeInfo().ImplementedInterfaces.ToList();
        return !interfaces.Contains(typeof(IRequiresInitialize));
    }
}