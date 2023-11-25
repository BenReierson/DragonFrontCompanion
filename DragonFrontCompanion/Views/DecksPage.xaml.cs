
using ZXing.Net.Maui.Controls;

namespace DragonFrontCompanion.Views;

public partial class DecksPage
{
    Grid _barcodeScannerLayout = null;
    bool _barcodeScanned = false;

    public DecksPage()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (ViewModel is not null)
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
       if (e.PropertyName == nameof(ViewModel.IsScanningForQrCode))
        {
            if (ViewModel.IsScanningForQrCode)
            {
                _barcodeScannerLayout = new Grid();
                _barcodeScanned = false;

                var barcodeReader = new CameraBarcodeReaderView
                {
                    HeightRequest = this.Height,
                    WidthRequest = this.Width
                };
                barcodeReader.BarcodesDetected += BarcodesDetected;

                var cancelButton = new Button
                {
                    BackgroundColor = Colors.Black.MultiplyAlpha(.5f),
                    Text = "Cancel",
                    FontSize = 30,
                    TextColor = Colors.White,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.End,
                    Margin = new Thickness(0, 0, 0, 50),
                };
                cancelButton.Clicked += ScanCancelButton_Clicked;

                _barcodeScannerLayout.Children.Add(barcodeReader);
                _barcodeScannerLayout.Children.Add(cancelButton);

                MainLayout.Children.Add(_barcodeScannerLayout);
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() => MainLayout.Children.Remove(_barcodeScannerLayout));
            }
        }
    }

    private void ScanCancelButton_Clicked(object sender, EventArgs e)
    {
        ViewModel.IsScanningForQrCode = false;
    }

    private void DeleteDeck_Clicked(object sender, EventArgs e)
    {
        var mi = ((MenuItem)sender);
        ViewModel.DeleteDeckCommand.Execute(mi.CommandParameter);
    }

    private void Champion_Tapped(object sender, ItemTappedEventArgs e)
    {
        if (DeviceInfo.Platform != DevicePlatform.WinUI)
            ViewModel.OpenDeckCommand.Execute(e.Group);
    }

    private void Deck_Edit(object sender, EventArgs e)
    {
        if (DeviceInfo.Platform != DevicePlatform.WinUI)
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

    protected override bool OnBackButtonPressed()
    {
        if (ViewModel.IsFactionPickerVisible)
        {
            ViewModel.IsFactionPickerVisible = false;
            return true;
        }
        else return base.OnBackButtonPressed();

    }

    void BarcodesDetected(System.Object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
    {
        if (_barcodeScanned) return;

        _barcodeScanned = true;
        ViewModel.IsScanningForQrCode = false;

        if (e.Results.Any())
            _=ViewModel.OpenDeckUrl(e.Results.FirstOrDefault()?.Value);
    }
}