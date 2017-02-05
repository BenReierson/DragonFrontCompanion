using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DragonFrontCompanion.Contols
{
    public class BindableToolbarItem : ToolbarItem
    {

        public BindableToolbarItem()
        {
            InitVisibility();
        }

        private async void InitVisibility()
        {
            OnIsVisibleChanged(this, false, IsVisible);
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();
            InitVisibility();
        }

        public bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        public static BindableProperty IsVisibleProperty =  BindableProperty.Create(nameof(IsVisible), typeof(bool), typeof(BindableToolbarItem), false, propertyChanged: OnIsVisibleChanged);

        private static void OnIsVisibleChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            var item = bindable as BindableToolbarItem;

            if (item.Parent == null)
                return;

            var items = ((ContentPage)item.Parent).ToolbarItems;

            if ((bool)newvalue && !items.Contains(item))
            {
                items.Insert(item.InsertIndex, item);
            }
            else if (!(bool)newvalue && items.Contains(item))
            {
                items.Remove(item);
            }
        }

        public static BindableProperty InsertIndexProperty =  BindableProperty.Create(nameof(InsertIndex), typeof(int), typeof(BindableToolbarItem), 0);

        public int InsertIndex
        {
            get { return (int)GetValue(InsertIndexProperty); }
            set { SetValue(InsertIndexProperty, value); }
        }
    }
}
