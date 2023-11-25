using FFImageLoading.Maui;
using System.ComponentModel;

namespace DragonFrontCompanion.Controls
{
    /// <summary>
    /// Created to fix CachedImage not rendering on windows as of Maui v7 and FfimageLoadingCompat v0.1.1
    /// </summary>
    public class CustomCachedImage
#if WINDOWS
        :Grid
#else
        :FFImageLoading.Maui.CachedImage
#endif

    {

#if WINDOWS
        public static readonly BindableProperty SourceProperty = BindableProperty.Create("Source", typeof(Microsoft.Maui.Controls.ImageSource), typeof(CachedImage), null, BindingMode.OneWay, null, propertyChanged: OnSourcePropertyChanged);
        private static void OnSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is CustomCachedImage instance)
                instance._sourceImage.Source = newValue as ImageSource;
        }

        [TypeConverter(typeof(Microsoft.Maui.Controls.ImageSourceConverter))]
        public Microsoft.Maui.Controls.ImageSource Source
        {
            get
            {
                return (Microsoft.Maui.Controls.ImageSource)GetValue(SourceProperty);
            }
            set
            {
                SetValue(SourceProperty, value);
            }
        }

        public static readonly BindableProperty ErrorPlaceholderProperty = BindableProperty.Create("ErrorPlaceholder", typeof(Microsoft.Maui.Controls.ImageSource), typeof(CustomCachedImage), null, BindingMode.OneWay, propertyChanged: OnErrorPlaceholderPropertyChanged);
        private static void OnErrorPlaceholderPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is CustomCachedImage instance)
            {
                if (instance._errorImage is null)
                {
                    instance._errorImage = new Image();
                    instance.Children.Insert(0, instance._errorImage);
                }
                instance._errorImage.Source = newValue as ImageSource;
            }
        }

        [TypeConverter(typeof(Microsoft.Maui.Controls.ImageSourceConverter))]
        public Microsoft.Maui.Controls.ImageSource ErrorPlaceholder
        {
            get
            {
                return (Microsoft.Maui.Controls.ImageSource)GetValue(ErrorPlaceholderProperty);
            }
            set
            {
                SetValue(ErrorPlaceholderProperty, value);
            }
        }
        
        public static readonly BindableProperty AspectProperty = BindableProperty.Create("Aspect", typeof(Aspect), typeof(CustomCachedImage), Aspect.AspectFit, propertyChanged: OnAspectPropertyChanged);
        
        private static void OnAspectPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is CustomCachedImage instance)
            {
                instance._sourceImage.Aspect = (Aspect)newValue;
                if (instance._errorImage is not null) instance._errorImage.Aspect = (Aspect)newValue;
            }
        }

        public Aspect Aspect
        {
            get
            {
                return (Aspect)GetValue(AspectProperty);
            }
            set
            {
                SetValue(AspectProperty, value);
            }
        }

        private Image _sourceImage = new Image();
        private Image _errorImage;
        public CustomCachedImage() : base()
        {
            Children.Add(_sourceImage);
        }
#endif
    }
}
