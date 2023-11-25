using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Newtonsoft.Json;

namespace DragonFrontCompanion.Helpers;

public interface IDialogService
{
    Task ShowError(string message, string title, string buttonText);

    Task ShowError(Exception error, string title, string buttonText);

    Task ShowMessage(string message, string title);

    Task ShowMessage(string message, string title, string buttonText);

    Task<bool> ShowMessage(string message, string title, string buttonConfirmText, string buttonCancelText);

    Task ShowMessageBox(string message, string title);

    Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons);

}

public class DialogService : IDialogService
{
    private Page _dialogPage;

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
        string buttonText)
    {
        if (string.IsNullOrEmpty(message))
        {
            _ = LogError(message, title, supressed: true);
            return;
        }

        await MainThread.InvokeOnMainThreadAsync(async ()=>
            await _dialogPage.DisplayAlert(
                title,
                message,
                buttonText));
        
        _ = LogError(message, title);

    }

    public async Task ShowError(
        Exception error,
        string title,
        string buttonText)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
            await _dialogPage.DisplayAlert(
                title,
                error.Message,
                buttonText));
        
        _ = LogError(error.Message, title, error);
    }

    public async Task ShowMessage(
        string message,
        string title)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        await _dialogPage.DisplayAlert(
            title,
            message,
            "OK"));
    }

    public async Task ShowMessage(
        string message,
        string title,
        string buttonText)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
            await _dialogPage.DisplayAlert(
                title,
                message,
                buttonText));
    }

    public async Task<bool> ShowMessage(
        string message,
        string title,
        string buttonConfirmText,
        string buttonCancelText)
    {
        return await MainThread.InvokeOnMainThreadAsync(async () =>
            await _dialogPage.DisplayAlert(title, message, buttonConfirmText, buttonCancelText));
    }

    public async Task ShowMessageBox(
        string message,
        string title)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
            await _dialogPage.DisplayAlert(
                title,
                message,
                "OK"));
    }
    public Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons)
        => _dialogPage.DisplayActionSheet(title, cancel, destruction, buttons);
}