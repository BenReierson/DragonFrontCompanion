using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace DragonFrontCompanion.Views
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();

            var feedbackButton = ((App)App.Current).FeedbackButton;
            if (feedbackButton != null) MainStack.Children.Add(feedbackButton);
        }
    }
}
