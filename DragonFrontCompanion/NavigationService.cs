using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Alerts;
using DragonFrontCompanion.Helpers;
using DragonFrontCompanion.Ioc;
using Microsoft.AppCenter.Analytics;

namespace DragonFrontCompanion;

public interface IViewModel
{
    bool IsModal { get; set; }
    bool IsTopOfNavStack { get; set; }
    bool IsCleanedUp { get;}
    string Title { get; set; }

    void Cleanup();
}
public interface ITabletViewModelFor { }
public interface ITabletViewModelFor<T> : ITabletViewModelFor where T : IViewModel { }
public interface IViewFor { }
public interface IViewFor<T> : IViewFor where T : class, IViewModel
{
    T ViewModel { get; set; }
}

public interface IRequiresInitialize
{
    bool IsInitialized { get; }
}

public interface INavigationService
{
    Task Pop();

    Task<T> Push<T>(bool AsRoot = false, bool uniqueVm = true) where T : class, IViewModel;

    Task<T> Push<T>(Action<T> initializeVm, bool AsRoot = false, bool uniqueVm = true) where T : class, IViewModel;

    Task<T> Push<T>(Func<T, Task> initializeVm, bool AsRoot = false, bool uniqueVm = true) where T : class, IViewModel;

    Task<T> PushAsNewStack<T>(Action<T> initializeVm, bool uniqueVm = true) where T : class, IViewModel;

    Task<T> PushAsNewStack<T>(Func<T, Task> initializeVm, bool uniqueVm = true) where T : class, IViewModel;

    Task Push<T>(T viewModel, bool AsRoot = false) where T : class, IViewModel;

    Task PushAsNewStack<T>(T viewModel) where T : class, IViewModel;

    Task RestoreMainStack();

    T CreateViewModelInstance<T>(bool unique = true) where T : class;
}

public class NavigationService : INavigationService
{

    [PreferredConstructor]
    public NavigationService()
    {
    }

    public NavigationService(INavigation injectedNavigation) //Used for unit testing
        => _injectedNavigation = injectedNavigation;

    //map of alternate implementations for tablet layout
    static readonly Dictionary<Type, Type> _viewModelTabletDictionary = new Dictionary<Type, Type>();

    int _modalCounter = 0;
    Page? _mainPage = null;

    #region Public Interface

    public async Task Pop()
    {
        try
        {
            await PopModal();
        }
        catch (ArgumentOutOfRangeException)
        {
            //wasn't modal, pop normally
            await PopNonModal();
        }
    }

    public Task<T> Push<T>(bool AsRoot = false, bool uniqueVm = true) where T : class, IViewModel
        => Push<T>(null, AsRoot, uniqueVm);

    public async Task<T> Push<T>(Action<T> initializeVm, bool AsRoot = false, bool uniqueVm = true) where T : class, IViewModel
    {
        var vm = CreateViewModelInstance<T>(uniqueVm);
        initializeVm?.Invoke(vm);

        await Push(vm, AsRoot);

        return vm;
    }

    public async Task<T> Push<T>(Func<T, Task>? initializeVm, bool AsRoot = false, bool uniqueVm = true) where T : class, IViewModel
    {
        var vm = CreateViewModelInstance<T>(uniqueVm);
        if (initializeVm != null) await initializeVm(vm);

        await Push(vm, AsRoot);

        return vm;
    }

    public async Task<T> PushAsNewStack<T>(Action<T> initializeVm, bool uniqueVm = true) where T : class, IViewModel
    {
        var vm = CreateViewModelInstance<T>(uniqueVm);
        initializeVm?.Invoke(vm);

        await PushAsNewStack(vm);

        return vm;
    }

    public async Task<T> PushAsNewStack<T>(Func<T, Task> initializeVm, bool uniqueVm = true) where T : class, IViewModel
    {
        var vm = CreateViewModelInstance<T>(uniqueVm);
        if (initializeVm != null) await initializeVm(vm);

        await PushAsNewStack(vm);

        return vm;
    }

    public async Task Push<T>(T viewModel, bool AsRoot) where T : class, IViewModel
    {
        VerifyInitialized(viewModel);

        // if (!String.IsNullOrWhiteSpace(viewModel.Title))
        //     _=Toast.Make($"Opening {viewModel.Title}").Show();
        //
        if (AsRoot)
        {
            try
            {//Pop any modal windows first, the service doesn't do this for us
                while (_modalCounter > 0) await PopModal();
            }
            catch (ArgumentOutOfRangeException)
            { //Modal counter is out of sync, so reset it. 
                //This can happen on android if the back button is used to bypass the navigation service.
                _modalCounter = 0;
            }
            await FormsNavigation.PopToRootAsync(false);
        }

        if (viewModel.IsModal)
        {//always push vms marked as modal modally
            Debug.WriteLine("Pushing as modal");
            await PushModalAsync(viewModel);
        }
        else if (_modalCounter > 0 && Application.Current.MainPage.Navigation.ModalStack.Count > 0)
        {//assume we need to push onto the modal stack 
            Debug.WriteLine("Pushing as modal");
            await PushModalAsync(viewModel);
        }
        else
        {
            var view = InstantiateView(viewModel);
            await FormsNavigation.PushAsync((Page)view);

            TrackNav(viewModel);
        }
    }

    public async Task PushAsNewStack<T>(T viewModel) where T : class, IViewModel
    {
        if ((_modalCounter > 0 && Application.Current.MainPage.Navigation.ModalStack.Count > 0)
            || viewModel.IsModal)
        {
            await PushModalAsync(viewModel);
        }
        else
        {
            await Task.Delay(10);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _mainPage = DetailPage;

                viewModel.IsModal = false;
                viewModel.IsTopOfNavStack = true;

                SwitchDetailPage(viewModel);
                TrackNav();
            });
            await Task.Delay(250);
        }
    }

    public async Task RestoreMainStack()
    {
        await Task.Delay(10);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (Application.Current.MainPage is FlyoutPage md && _mainPage != null)
                md.Detail = _mainPage;
            else if (_mainPage != null)
                Application.Current.MainPage = _mainPage;

            _mainPage = null;
        });
        await Task.Delay(250);
    }

    public T CreateViewModelInstance<T>(bool unique = true) where T : class
    {
        return CreateViewModelInstanceStatic<T>(unique);
    }

    public static T CreateViewModelInstanceStatic<T>(bool unique = true) where T : class
    {
        VM? GetVM<VM>() where VM : class
        {
            if (unique) return SimpleIoc.Default.GetInstanceWithoutCaching<VM>(Guid.NewGuid().ToString());
            return SimpleIoc.Default.GetInstance<VM>();
        }

        object? GetVMByType(Type vmType)
        {
            if (unique) return SimpleIoc.Default.GetInstanceWithoutCaching(vmType, Guid.NewGuid().ToString()) as T;
            return SimpleIoc.Default.GetInstance(vmType) as T;
        }

        try
        {
            // if (_viewModelTabletDictionary.ContainsKey(typeof(T))
            //     && App.UseTabletLayout)
            // {//override in tablet mode
            //     var tabletVmType = _viewModelTabletDictionary[typeof(T)];
            //     return GetVMByType(tabletVmType) as T ?? throw new ArgumentException();
            // }

            return GetVM<T>() ?? throw new ArgumentException();
        }
        catch (Exception)
        {
            return GetVM<T>() ?? throw new ArgumentException();
        }
    }
    #endregion

    #region Private Methods

    void TrackNav(IViewModel? viewModel = null, [CallerMemberName]string navType = "")
    {
        var properties = new Dictionary<string, string> { { "Type", navType } };
        if (viewModel != null) properties.Add("ViewModel", viewModel.GetType().Name);

        Analytics.TrackEvent("NAVIGATION", properties);
    }

    async Task PushModalAsync<T>(T viewModel) where T : class, IViewModel
    {
        VerifyInitialized(viewModel);
        viewModel.IsModal = true;

        TrackNav(viewModel);
        _modalCounter++;

        Page view;
        try
        {
            view = (Page)InstantiateView(viewModel);
        }
        catch (InvalidCastException)
        {
            view = (Page)InstantiateViewForSubclass(viewModel);
        }

        // Most likely we're going to want to put this into a navigation page so we can have a title bar on it
        var nv = new NavigationPage(view);

        await FormsNavigation.PushModalAsync(nv);
    }

    async Task PopNonModal()
    {
        try
        {
            await FormsNavigation.PopAsync(true);
            TrackNav();
        }
        catch (Exception)
        {
            if (_mainPage != null &&
                FormsNavigation?.NavigationStack?.Count == 1)
            {//were at the top of the secondary stack, restore main stack
                await RestoreMainStack();
            }
            else throw;
        }
    }

    async Task PopModal()
    {
        try
        {
            await FormsNavigation.PopModalAsync(true);
            if (_modalCounter > 0) _modalCounter--;
            TrackNav();
        }
        catch (Exception)
        {
            if (_mainPage != null &&
                FormsNavigation?.NavigationStack?.Count == 1)
            {//were at the top of the secondary stack, restore main stack
                await RestoreMainStack();
            }
            else throw;
        }
    }

    //Help to make sure we're initializing viewmodels that require it
    void VerifyInitialized<T>(T viewModel) where T : class, IViewModel
    {
#if DEBUG
        try
        {
            if (viewModel is IRequiresInitialize)
            {
                if (!((IRequiresInitialize)viewModel).IsInitialized)
                    throw new ArgumentException($"{viewModel.GetType().FullName} navigated without proper initialization.");
            }
            else 
            {
                var methods = viewModel.GetType().GetRuntimeMethods();
                foreach (var method in methods)
                {
                    if (method.Name == "Initialize")
                        throw new ArgumentException($"{viewModel.GetType().FullName} has an Initialize() method but doesn't implement {nameof(IRequiresInitialize)}");
                }
            }
        }
        catch (ArgumentException ex)
        {
            SimpleIoc.Default.GetInstance<IDialogService>()?.ShowError(ex, "Debug Error", "OK");
        }
#endif
    }
    #endregion

    #region Originally Sourced from https://github.com/codemillmatt/codemill.vmfirstnav
    INavigation? _injectedNavigation;
    INavigation FormsNavigation
    {
        get
        {
            if (_injectedNavigation != null) return _injectedNavigation;

            var tabController = Application.Current.MainPage as TabbedPage;
            var masterController = Application.Current.MainPage as FlyoutPage;

            // First check to see if we're on a tabbed page, then master detail, finally go to overall fallback
            return tabController?.CurrentPage?.Navigation ??
                   (masterController?.Detail as TabbedPage)?.CurrentPage?.Navigation ?? // special consideration for a tabbed page inside master/detail
                   masterController?.Detail?.Navigation ??
                   Application.Current.MainPage.Navigation;
        }
    }

    public static void RegisterViewModels()
    {
        var asm = typeof(App).Assembly;

        // Loop through everything in the assembley that implements IViewFor<T>
        foreach (var type in asm.DefinedTypes.Where(dt => !dt.IsAbstract &&
                                                          dt.ImplementedInterfaces.Any(ii => ii == typeof(IViewFor))))
        {
            // Get the IViewFor<T> portion of the type that implements it
            var viewForType = type.ImplementedInterfaces.FirstOrDefault(
                ii => ii.IsConstructedGenericType &&
                      ii.GetGenericTypeDefinition() == typeof(IViewFor<>));

            // Register it, using the T as the key and the view as the value
            Register(viewForType.GenericTypeArguments[0], type.AsType());
        }

        // Loop through everything in the assembley that implements IViewModel
        // ensure they are registered in case they don't have a specific IViewFor match
        foreach (var type in asm.DefinedTypes.Where(dt => !dt.IsAbstract &&
                                                          dt.ImplementedInterfaces.Any(ii => ii == typeof(IViewModel))))
        {
            Register(type);
        }

        //find any tablet view models
        foreach (var type in asm.DefinedTypes.Where(dt => !dt.IsAbstract &&
                                                          dt.ImplementedInterfaces.Any(ii => ii == typeof(ITabletViewModelFor))))
        {
            // Get the ITabletViewModelFor<T> portion of the type that implements it
            var vmForType = type.ImplementedInterfaces.FirstOrDefault(
                ii => ii.IsConstructedGenericType &&
                      ii.GetGenericTypeDefinition() == typeof(ITabletViewModelFor<>));

            if (!_viewModelTabletDictionary.ContainsKey(vmForType.GenericTypeArguments[0]))
                _viewModelTabletDictionary.Add(vmForType.GenericTypeArguments[0], type.AsType());
        }
    }

    /// <summary>
    /// Cleanup all registered viewmodels and re-register them with the IOC
    /// This ensures no existing instances will be re-used
    /// </summary>
    public static void Cleanup()
    {
        foreach (var vmType in _viewModelViewDictionary.Keys)
        {
            foreach (var instance in SimpleIoc.Default.GetAllCreatedInstances(vmType))
            {
                if (instance is IViewModel vm) vm.Cleanup();
                SimpleIoc.Default.Unregister(instance);
            }
            UnregisterFromIoc(vmType);
            RegisterInIoc(vmType);
        }
    }

    public static void Register(Type viewModelType, Type? viewType = null)
    {
        RegisterInIoc(viewModelType);

        // ensure Register can be called again without register the same viewmodel again.
        if (_viewModelViewDictionary.ContainsKey(viewModelType))
            return;

        if (viewType != null) _viewModelViewDictionary.Add(viewModelType, viewType);
    }

    static void RegisterInIoc(Type typeToRegister)
    {
        try
        {
            // Get the Register<T1,T2>() method
            MethodInfo methodInfo =
                SimpleIoc.Default.GetType().GetMethods()
                   .Where(m => m.Name == "Register")
                   .Where(m => m.IsGenericMethod)
                   .Where(m => m.GetGenericArguments().Length == 1)
                   .Where(m => m.GetParameters().Length == 0)
                   .Single();

            // Create a version of the method that takes your types
            methodInfo = methodInfo.MakeGenericMethod(typeToRegister);

            // Invoke the method on the default container (no parameters)
            methodInfo.Invoke(SimpleIoc.Default, null);
        }
        catch (Exception) { }
    }

    static void UnregisterFromIoc(Type typeToUnregister)
    {
        try
        {
            MethodInfo methodInfo =
                SimpleIoc.Default.GetType().GetMethods()
                   .Where(m => m.Name == "Unregister")
                   .Where(m => m.IsGenericMethod)
                   .Where(m => m.GetGenericArguments().Length == 1)
                   .Where(m => m.GetParameters().Length == 0)
                   .Single();

            // Create a version of the method that takes your types
            methodInfo = methodInfo.MakeGenericMethod(typeToUnregister);

            // Invoke the method on the default container (no parameters)
            methodInfo.Invoke(SimpleIoc.Default, null);
        }
        catch (Exception) { }
    }


    // View model to view lookup - making the assumption that view model to view will always be 1:1
    static readonly Dictionary<Type, Type> _viewModelViewDictionary = new Dictionary<Type, Type>();

    // Because we're going to do a hard switch of the page, either return
    // the detail page, or if that's null, then the current main page       
    Page DetailPage
    {
        get
        {
            var masterController = Application.Current.MainPage as FlyoutPage;

            return masterController?.Detail ?? Application.Current.MainPage;
        }
        set
        {
            var masterController = Application.Current.MainPage as FlyoutPage;

            if (masterController != null)
            {
                masterController.Detail = value;
                masterController.IsPresented = false;
            }
            else
            {
                Application.Current.MainPage = value;
            }
        }
    }

    void SwitchDetailPage<T>(T viewModel) where T : class, IViewModel
    {

        var view = InstantiateViewForSubclass(viewModel);

        Page newDetailPage;

        // Tab pages shouldn't go into navigation pages
        if (view is TabbedPage)
            newDetailPage = (Page)view;
        else
            newDetailPage = new NavigationPage((Page)view);

        DetailPage = newDetailPage;
    }

    void PopTo<T>() where T : class, IViewModel
    {
        var pagesToRemove = new List<Page>();
        var upper = FormsNavigation.NavigationStack.Count;

        // Loop through the nav stack backwards
        for (int i = upper - 1; i >= 0; i--)
        {
            var currentView = FormsNavigation.NavigationStack[i] as IViewFor;

            // Stop the whole show if one of the pages isn't an IViewFor
            if (currentView == null) return;

            var strongTypedPaged = currentView as IViewFor<T>;

            // If we hit the view model type, break out
            if (strongTypedPaged != null) break;

            // Finally - always add to the list
            if (currentView is Page currentPage) pagesToRemove.Add(currentPage);
        }

        foreach (var item in pagesToRemove)
        {
            FormsNavigation.RemovePage(item);
        }
    }

    static IViewFor<T> InstantiateView<T>(T viewModel) where T : class, IViewModel
    {
        // Figure out what type the view model is
        var viewModelType = viewModel.GetType();

        // look up what type of view it corresponds to
        var viewType = _viewModelViewDictionary[viewModelType];

        // instantiate it
        var view = (IViewFor<T>)Activator.CreateInstance(viewType);
        view.ViewModel = viewModel;
        return view;

    }

    static IViewFor InstantiateViewForSubclass<T>(T viewModel) where T : class, IViewModel
    {
        var viewModelType = viewModel.GetType();

        var viewType = _viewModelViewDictionary[viewModelType];

        var view = (IViewFor)Activator.CreateInstance(viewType);

        var viewModelProperty = view.GetType().GetRuntimeProperty(nameof(IViewFor<T>.ViewModel));

        viewModelProperty?.SetValue(view, viewModel);

        return view;
    }
    #endregion
}