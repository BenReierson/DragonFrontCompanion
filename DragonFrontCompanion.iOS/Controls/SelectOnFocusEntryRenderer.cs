using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;
using DragonFrontCompanion.Contols;
using Xamarin.Forms.Platform.iOS;
using UIKit;
using DragonFrontCompanion.iOS.Controls;

[assembly: ExportRenderer(typeof(SelectOnFocusEntry), typeof(SelectOnFocusEntryRenderer))]
namespace DragonFrontCompanion.iOS.Controls
{
    public class SelectOnFocusEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Entry> e)
        {
            base.OnElementChanged(e);
            var nativeTextField = (UITextField)Control;
            nativeTextField.EditingDidBegin += (object sender, EventArgs eIos) => {
                nativeTextField.PerformSelector(new ObjCRuntime.Selector("selectAll"), null, 0.0f);
            };
        }
    }
}