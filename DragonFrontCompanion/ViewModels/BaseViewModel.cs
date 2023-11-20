
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DragonFrontCompanion.Helpers;
namespace DragonFrontCompanion.ViewModels;

public abstract partial class BaseViewModel : ObservableObject, IViewModel
{
    protected readonly INavigationService _navigationService;
    protected readonly IDialogService _dialogService;
    protected BaseViewModel(INavigationService navigationService, IDialogService dialogService)
    {
        _navigationService = navigationService;
        _dialogService = dialogService;
    }
    
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isModal;
    [ObservableProperty] private bool _isTopOfNavStack;
    [ObservableProperty] private string _title;
    
    public bool IsCleanedUp { get; protected set; }

    [RelayCommand]
    private Task NavigateBack() => _navigationService.Pop();

#pragma warning disable CS1998 //Virtual methods with no await
    public virtual async Task OnAppearing() { }
    public virtual async Task OnDisappearing() { }
#pragma warning restore CS1998

    public virtual void Cleanup() 
    {
        IsCleanedUp = true;
    }

}