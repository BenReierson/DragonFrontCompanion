using System;
using System.Linq.Expressions;
using Xamarin.Forms;

namespace DragonFrontCompanion.Controls
{
    //
    // Summary:
    //     Specifies where the image will occur relative to the text on a
    public enum ImageOrientation
    {
        //
        // Summary:
        //     The image to left
        ImageToLeft = 0,
        //
        // Summary:
        //     The image on top
        ImageOnTop = 1,
        //
        // Summary:
        //     The image to right
        ImageToRight = 2,
        //
        // Summary:
        //     The image on bottom
        ImageOnBottom = 3
    }

    /// <summary>
    /// Creates a button with text and an image.
    /// The image can be on the left, above, on the right or below the text.
    /// </summary>
    public class ImageButton : Button
    {
        /// <summary>
        /// Backing field for the Source property.
        /// </summary>
        public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(ImageSource), typeof(ImageButton), null, BindingMode.OneWay, 
            propertyChanged:(bindable, oldvalue, newvalue) => ((VisualElement)bindable).ToString());

        /// <summary>
        /// Backing field for the DisabledSource property.
        /// </summary>
        public static readonly BindableProperty DisabledSourceProperty = BindableProperty.Create(nameof(DisabledSource), typeof(ImageSource), typeof(ImageButton), null, BindingMode.OneWay,
            propertyChanged: (bindable, oldvalue, newvalue) => ((VisualElement)bindable).ToString());

        /// <summary>
        /// Backing field for the image width property.
        /// </summary>
        public static readonly BindableProperty ImageWidthRequestProperty =
            BindableProperty.Create(nameof(ImageWidthRequest), typeof(int), typeof(ImageButton), default(int));

        /// <summary>
        /// Backing field for the image height property.
        /// </summary>
        public static readonly BindableProperty ImageHeightRequestProperty =
            BindableProperty.Create(nameof(ImageHeightRequest), typeof(int), typeof(ImageButton), default(int));

        /// <summary>
        /// Backing field for the orientation property.
        /// </summary>
        public static readonly BindableProperty OrientationProperty =
            BindableProperty.Create(nameof(Orientation),typeof(ImageOrientation), typeof(ImageButton), ImageOrientation.ImageToLeft);

        /// <summary>
        /// Backing field for the tint color property.
        /// </summary>
        public static readonly BindableProperty ImageTintColorProperty = 
            BindableProperty.Create(nameof(ImageTintColor), typeof(Color), typeof(ImageButton), Color.Transparent);

        /// <summary>
        /// Backing field for the disbaled tint color property.
        /// </summary>
        public static readonly BindableProperty DisabledImageTintColorProperty = 
            BindableProperty.Create(nameof(DisabledImageTintColor),typeof(Color), typeof(ImageButton), Color.Transparent);

        /// <summary>
        /// Gets or sets the ImageSource to use with the control.
        /// </summary>
        /// <value>
        /// The Source property gets/sets the value of the backing field, SourceProperty.
        /// </value>
        [TypeConverter(typeof(ImageSourceConverter))]
        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        /// <summary>
        /// Gets or sets the ImageSource to use with the control when the button is disabled.
        /// </summary>
        /// <value>
        /// The Source property gets/sets the value of the backing field, DisabledSourceProperty.
        /// </value>
        [TypeConverter(typeof(ImageSourceConverter))]
        public ImageSource DisabledSource
        {
            get { return (ImageSource)GetValue(DisabledSourceProperty); }
            set { SetValue(DisabledSourceProperty, value); }
        }

        /// <summary>
        /// Gets or sets The orientation of the image relative to the text.
        /// </summary> 
        /// <value>
        /// The Orientation property gets/sets the value of the backing field, OrientationProperty.
        /// </value> 
        public ImageOrientation Orientation
        {
            get { return (ImageOrientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// Gets or sets the requested height of the image. If less than or equal to zero than a 
        /// height of 50 will be used.
        /// </summary>
        /// <value>
        /// The ImageHeightRequest property gets/sets the value of the backing field, ImageHeightRequestProperty.
        /// </value> 
        public int ImageHeightRequest
        {
            get { return (int)GetValue(ImageHeightRequestProperty); }
            set { SetValue(ImageHeightRequestProperty, value); }
        }

        /// <summary>
        /// Gets or sets the requested width of the image. If less than or equal to zero than a 
        /// width of 50 will be used.
        /// </summary>
        /// <value>
        /// The ImageWidthRequest property gets/sets the value of the backing field, ImageWidthRequestProperty.
        /// </value> 
        public int ImageWidthRequest
        {
            get { return (int)GetValue(ImageWidthRequestProperty); }
            set { SetValue(ImageWidthRequestProperty, value); }
        }

        /// <summary>
        /// Gets or sets the tint color of the image 
        /// </summary>
        /// <value>
        /// The ImageTintColor property gets/sets the value of the backing field, ImageTintColorProperty.
        /// </value> 
        public Color ImageTintColor
        {
            get { return (Color)GetValue(ImageTintColorProperty); }
            set { SetValue(ImageTintColorProperty, value); }
        }

        /// <summary>
        /// Gets or sets the tint color of the image when the button is disabled
        /// </summary>
        /// <value>
        /// The DisabledImageTintColor property gets/sets the value of the backing field, DisabledImageTintColorProperty.
        /// </value> 
        public Color DisabledImageTintColor
        {
            get { return (Color)GetValue(DisabledImageTintColorProperty); }
            set { SetValue(DisabledImageTintColorProperty, value); }
        }
    }
}