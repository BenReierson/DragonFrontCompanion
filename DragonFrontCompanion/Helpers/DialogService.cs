using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Newtonsoft.Json;
using Xamarin.Forms;
using GalaSoft.MvvmLight.Views;

namespace DragonFrontCompanion.Helpers
{
    public class DialogService : IDialogService
    {
        Page _dialogPage;

        public void Initialize(Page dialogPage)
        {
            _dialogPage = dialogPage;
        }

        async Task LogError(string message, string title, Exception ex = null, bool supressed = false)
        {
            try
            {
                if (!AppCenter.Configured || !(await Analytics.IsEnabledAsync())) return;

                var properties = new Dictionary<string, string> { { "Message", message }, { "Title", title } };
                if (ex != null) properties.Add("Exception", ex.GetType().Name);

                Analytics.TrackEvent($"ERROR_{(supressed ? "SUPPRESSED" : "SHOWN")}", properties);

            }
            catch (Exception) { }
        }

        public async Task ShowError(string message,
            string title,
            string buttonText,
            Action afterHideCallback)
        {
            if (!((App)Application.Current).IsActive) return; //don't show errors when the app isn't in the foreground

            try
            {

                if (string.IsNullOrEmpty(message))
                {
                    _ = LogError(message, title, supressed: true);
                    return;
                }
                else
                {
                    if (App.RuntimePlatform == App.Device.Android)
                    {
                        await UserDialogs.Instance.AlertAsync(new AlertConfig
                        {
                            Message = message,
                            Title = title,
                            OkText = buttonText,
                            OnAction = afterHideCallback
                        });
                    }
                    else
                    {
                        await _dialogPage.DisplayAlert(
                            title,
                            message,
                            buttonText);

                        afterHideCallback?.Invoke();
                    }
                    _ = LogError(message, title);
                }
            }
            catch (Exception ex) { App.LogError(ex); }
        }

        public async Task ShowError(
            Exception error,
            string title,
            string buttonText,
            Action afterHideCallback)
        {
            if (!((App)Application.Current).IsActive) return; //don't show errors when the app isn't in the foreground

            try
            {
                if (App.RuntimePlatform == App.Device.Android)
                {
                    await UserDialogs.Instance.AlertAsync(new AlertConfig
                    {
                        Message = error.Message,
                        Title = title,
                        OkText = buttonText,
                        OnAction = afterHideCallback
                    });
                }
                else
                {
                    await _dialogPage.DisplayAlert(
                        title,
                        error.Message,
                        buttonText);

                    afterHideCallback?.Invoke();
                }

                _ = LogError(error.Message, title, error);

            }
            catch (Exception ex) { App.LogError(ex); }
        }

        public async Task ShowMessage(
            string message,
            string title)
        {
            if (!((App)Application.Current).IsActive) return; //don't show errors when the app isn't in the foreground

            try
            {
                if (App.RuntimePlatform == App.Device.Android)
                {
                    await UserDialogs.Instance.AlertAsync(new AlertConfig
                    {
                        Message = message,
                        Title = title,
                        OkText = "OK",
                    });
                }
                else
                {
                    await _dialogPage.DisplayAlert(
                        title,
                        message,
                        "OK");
                }
            }
            catch (Exception ex) { App.LogError(ex); }
        }

        public async Task ShowMessage(
            string message,
            string title,
            string buttonText,
            Action afterHideCallback)
        {
            if (!((App)Application.Current).IsActive) return; //don't show errors when the app isn't in the foreground

            try
            {
                if (App.RuntimePlatform == App.Device.Android)
                {
                    await UserDialogs.Instance.AlertAsync(new AlertConfig
                    {
                        Message = message,
                        Title = title,
                        OkText = buttonText,
                    });
                }
                else
                {
                    await _dialogPage.DisplayAlert(
                        title,
                        message,
                        buttonText);

                    afterHideCallback?.Invoke();
                }
            }
            catch (Exception ex) { App.LogError(ex); }
        }

        public async Task<bool> ShowMessage(
            string message,
            string title,
            string buttonConfirmText,
            string buttonCancelText,
            Action<bool> afterHideCallback)
        {
            if (!((App)Application.Current).IsActive) return false; //don't show errors when the app isn't in the foreground

            try
            {
                if (App.RuntimePlatform == App.Device.Android)
                {
                    var result = await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig
                    {
                        Message = message,
                        Title = title,
                        OkText = buttonConfirmText,
                        CancelText = buttonCancelText,
                        OnAction = afterHideCallback
                    });
                    return result;
                }
                else
                {
                    var result = await _dialogPage.DisplayAlert(
                        title,
                        message,
                        buttonConfirmText,
                        buttonCancelText);

                    afterHideCallback?.Invoke(result);

                    return result;
                }
            }
            catch (Exception ex)
            {
                App.LogError(ex);
                return false;
            }
        }

        public async Task ShowMessageBox(
            string message,
            string title)
        {
            if (!((App)Application.Current).IsActive) return; //don't show errors when the app isn't in the foreground

            try
            {
                if (App.RuntimePlatform == App.Device.Android)
                {
                    await UserDialogs.Instance.AlertAsync(new AlertConfig
                    {
                        Message = message,
                        Title = title,
                    });
                }
                else
                {
                    await _dialogPage.DisplayAlert(
                        title,
                        message,
                        "OK");
                }
            }
            catch (Exception ex) { App.LogError(ex); }
        }

        public async Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons)
        {
            if (!((App)Application.Current).IsActive) return cancel; //don't show errors when the app isn't in the foreground

            try
            {
                if (App.RuntimePlatform == App.Device.Android)
                {
                    //var options = (from button in buttons
                    //select new ActionSheetOption(button)).ToList();
                    //(new ActionSheetConfig
                    //{
                    //    Title = title,
                    //    Cancel = new ActionSheetOption(cancel),
                    //    Options = options,
                    //    Destructive = options.FirstOrDefault(o => o.Text == destruction),
                    //});

                    return await UserDialogs.Instance.ActionSheetAsync(title, cancel, destruction, null, buttons);
                }
                else
                {
                    return await _dialogPage.DisplayActionSheet(title, cancel, destruction, buttons);
                }
            }
            catch (Exception ex)
            {
                App.LogError(ex);
                return cancel;
            }
        }

       

    }
}
