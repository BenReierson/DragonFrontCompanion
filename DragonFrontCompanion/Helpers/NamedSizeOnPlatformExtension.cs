using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DragonFrontCompanion.Helpers
{
    [ContentProperty("FontSize")]
    public class NamedSizeOnPlatformExtension : IMarkupExtension
    {
        public NamedSizeOnPlatformExtension()
        {
        }

        public string iOS { get; set; }
        public string Android { get; set; }
        public string WinPhone { get; set; }

        public Type ViewType { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            object val = NamedSize.Default;

            string fontSize = this.getValueByPlatform();

            if (NamedSize.IsDefined(typeof(NamedSize), fontSize))
            {
                val = Device.GetNamedSize((NamedSize)NamedSize.Parse(typeof(NamedSize), fontSize), ((ViewType.Equals(null)) ? typeof(Label) : ViewType));
            }

            return val;
        }

        string getValueByPlatform()
        {
            string val = string.Empty;

            switch (App.RuntimePlatform)
            {
                case App.Device.iOS:
                    val = iOS;
                    break;
                case App.Device.Android:
                    val = Android;
                    break;
                case App.Device.UWP:
                    val = WinPhone;
                    break;
                default:
                    val = "Default";
                    break;
            }

            return val;
        }
    }
}
