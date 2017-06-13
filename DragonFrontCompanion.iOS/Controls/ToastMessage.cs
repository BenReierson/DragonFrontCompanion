using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Foundation;
using UIKit;

namespace DragonFrontCompanion.iOS.Controls
{
    public static class ToastMessage
    {
        const double LONG_DELAY = 3.5;
        const double SHORT_DELAY = 2.0;

        static NSTimer alertDelay;
        static UIAlertController alert;

        public static void LongAlert(string message)
        {
            ShowAlert(message, LONG_DELAY);
        }
        public static void ShortAlert(string message)
        {
            ShowAlert(message, SHORT_DELAY);
        }

        static void ShowAlert(string message, double seconds)
        {
            if (alert != null) dismissMessage();

            alertDelay = NSTimer.CreateScheduledTimer(seconds, (obj) =>
            {
                dismissMessage();
            });
            alert = UIAlertController.Create(null, message, UIAlertControllerStyle.Alert);
            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alert, true, null);
        }
        static void dismissMessage()
        {
            if (alert != null)
            {
                alert.DismissViewController(true, null);
                alert = null;
            }
            if (alertDelay != null)
            {
                alertDelay.Dispose();
            }
        }
    }
}