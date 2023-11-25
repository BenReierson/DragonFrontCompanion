using System.Globalization;
using Microsoft.AppCenter.Crashes;
using QRCoder;

namespace DragonFrontCompanion.Helpers
{
    public class StringToQrCodeImageConverter : IValueConverter
    {
        public Color DarkColor { get; set; } = Color.FromHex("#E1CA35");
        public Color LightColor { get; set; } = Color.FromHex("#123338");
        public bool DrawQuietZones { get; set; } = false;

        byte[] darkColorBytes;
        byte[] lightColorBytes;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (darkColorBytes is null || lightColorBytes is null)
            {
                DarkColor.ToRgba(out var dr, out var dg, out var db, out var da);
                darkColorBytes = new[] { dr, dg, db, da };

                LightColor.ToRgba(out var lr, out var lg, out var lb, out var la);
                lightColorBytes = new[] { lr, lg, lb, la };
            }

            ImageSource codeImageSource = null;

            if (value is string code && !string.IsNullOrEmpty(code))
            {
                try
                {
                    var qrGenerator = new QRCodeGenerator();
                    var qrData = qrGenerator.CreateQrCode(code, QRCodeGenerator.ECCLevel.L);
                    var qRCode = new PngByteQRCode(qrData);
                    var qrCodeBytes = qRCode.GetGraphic(20, darkColorBytes, lightColorBytes, DrawQuietZones);
                    codeImageSource = ImageSource.FromStream(() => new MemoryStream(qrCodeBytes));
                }
                catch (Exception ex)
                {
                    Crashes.TrackError(ex, new Dictionary<string, string>
                    {
                        {"Error", "Failed to convert deck code to qr code" },
                        {"DeckString", code }
                    });
                }
            }

            return codeImageSource;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

