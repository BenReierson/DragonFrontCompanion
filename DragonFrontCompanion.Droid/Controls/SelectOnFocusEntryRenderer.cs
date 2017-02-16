using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using DragonFrontCompanion.Controls;
using DragonFrontCompanion.Droid.Controls;

[assembly: ExportRenderer(typeof(SelectOnFocusEntry), typeof(SelectOnFocusEntryRenderer))]
namespace DragonFrontCompanion.Droid.Controls
{
    public class SelectOnFocusEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement == null)
            {
                var nativeEditText = (global::Android.Widget.EditText)Control;
                nativeEditText.SetSelectAllOnFocus(true);
            }
        }
    }
}