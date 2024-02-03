using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace XamlStudio.Controls
{
    public class AppFontIcon : FontIcon
    {

        /// <summary>
        /// Invoked whenever application code or internal processes (such as a rebuilding layout pass)
        /// call ApplyTemplate. In simplest terms, this means the method is called just before a UI element 
        /// displays in your app. Override this method to influence the default post-template logic of a class.
        /// </summary>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        #region IconName (Dependency Property)

        /// <summary>
        /// Dependency Property associated to <see cref="IconName"/> property.
        /// </summary>
        public static readonly DependencyProperty IconNameProperty =
            DependencyProperty.Register(
                nameof(IconName),
                typeof(IconGlyphNames),
                typeof(AppFontIcon),
                new PropertyMetadata(string.Empty, OnIconNameChanged)
                );

        /// <summary>
        /// Assigns a <see cref="IconGlyphNames"/> value for the the icon 
        /// to be rendered on this instance of <see cref="AppFontIcon"/>.
        /// </summary>
        public IconGlyphNames IconName
        {
            get { return (IconGlyphNames)GetValue(IconNameProperty); }
            set { SetValue(IconNameProperty, value); }
        }


        /// <summary>
        /// Raised when the value of <see cref="IconName"/> changes
        /// to capture the unicode value that maps the icon name and assigned it
        /// to <see cref="FontIcon.Glyph"/>.
        /// </summary>
        public static void OnIconNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AppFontIcon sender)
            {
                sender.Glyph = IconGlyphValues.GetIcon(sender.IconName);
            }
        }

        #endregion

        #region IconSize (Dependency Property)

        /// <summary>
        /// Dependency Property associated to <see cref="IconSize"/> property.
        /// </summary>
        public static readonly DependencyProperty IconSizeProperty =
            DependencyProperty.Register(
                nameof(IconSize),
                typeof(IconContexts),
                typeof(AppFontIcon),
                new PropertyMetadata(IconContexts.Default, OnIconSizeChanged)
                );

        /// <summary>
        /// Assignes a <see cref="IconContexts"/> value for the icon pre-defined
        /// sizes used to render the icon on this instance of <see cref="AppFontIcon"/>.
        /// </summary>
        public IconContexts IconSize
        {
            get { return (IconContexts)GetValue(IconSizeProperty); }
            set { SetValue(IconSizeProperty, value); }
        }

        /// <summary>
        /// Raised when the value of <see cref="IconSize"/> changes
        /// to capture the corresponding double value to assignd to <see cref="FontIcon.FontSize"/>.
        /// </summary>
        public static void OnIconSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AppFontIcon sender && sender.IconSize != IconContexts.Custom)
            {
                sender.FontSize = IconSizeValues.GetIconSize(sender.IconSize);
            }
        }

        #endregion

        #region CustomIconSize (Dependency Property)

        /// <summary>
        /// Dependency Property associated to <see cref="CustomIconSize"/> property.
        /// </summary>
        public static readonly DependencyProperty CustomIconSizeProperty =
            DependencyProperty.Register(
                nameof(CustomIconSize),
                typeof(double),
                typeof(AppFontIcon),
                new PropertyMetadata(16.0, OnCustomIconSizeChanged)
                );

        /// <summary>
        /// Assignes a double value for the icon size
        /// used to render the icon on this instance of <see cref="AppFontIcon"/>.
        /// </summary>
        public double CustomIconSize
        {
            get { return (double)GetValue(CustomIconSizeProperty); }
            set { SetValue(CustomIconSizeProperty, value); }
        }

        /// <summary>
        /// Raised when the value of <see cref="IconSize"/> changes
        /// to capture the corresponding double value to assignd to <see cref="FontIcon.FontSize"/>.
        /// </summary>
        public static void OnCustomIconSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AppFontIcon sender && sender.CustomIconSize != double.NaN)
            {
                sender.FontSize = sender.CustomIconSize;
                sender.IconSize = IconContexts.Custom;
            }
        }

        #endregion

    }
    #region Icon Glyphs

    /// <summary>
    /// IconGlyphValues provides a mapping lookup for
    /// icon names into their corresponding unicode value
    /// to be rendered in the FontIcon-derived class.
    /// </summary>
    public static class IconGlyphValues
    {
        /// <summary>
        /// Tries to get a unicode value out of the given icon name.
        /// </summary>
        public static string GetIcon(IconGlyphNames icon)
        {
            return IconMapping.TryGetValue(icon, out string value) ? value : string.Empty;
        }

        /// <summary>
        /// Maps the icon names into unicode values.
        /// </summary>
        private static readonly Dictionary<IconGlyphNames, string> IconMapping = new Dictionary<IconGlyphNames, string>()
        {
            {IconGlyphNames.Search,     "\uE721"},
            {IconGlyphNames.Work,       "\uE821"},
            {IconGlyphNames.Copy,       "\uE8C8"},
            {IconGlyphNames.Photo2,     "\uEB9F"},
            {IconGlyphNames.Bug,        "\uEBE8"},
            {IconGlyphNames.Wheel,      "\uEE94"},
            {IconGlyphNames.Settings,   "\uE713"},
            {IconGlyphNames.Properties, "\uE90F"},
        };
    }

    /// <summary>
    /// Defines the list of possible values for IconSize
    /// </summary>
    public enum IconGlyphNames
    {
        Search,
        Work,
        Copy,
        Photo2,
        Bug,
        Wheel,
        Settings,
        Properties,
    }

    #endregion

    #region IconSize Lookup

    /// <summary>
    /// This class helps translate a FontIcon size name into its 
    /// corresponding numerical value.
    /// </summary>
    public static class IconSizeValues
    {
        /// <summary>
        /// Tries to get a numercal value out of the indicated name.
        /// </summary>
        public static double GetIconSize(IconContexts sizeName)
        {
            // Default Value is 16 based on ContentControlFontSize from ThemeResources
            return SizeMapping.TryGetValue(sizeName, out double value) ? value : 16.0;
        }

        /// <summary>
        /// Maps the Size names to numerical values.
        /// </summary>
        private static readonly Dictionary<IconContexts, double> SizeMapping = new Dictionary<IconContexts, double>()
        {
            {IconContexts.Smaller,       10.0 },
            {IconContexts.Small,         12.0 },
            {IconContexts.Default,       16.0 },
            {IconContexts.Medium,        18.0 },
            {IconContexts.MediumLarge,   20.0 },
            {IconContexts.Large,         24.0 },
            {IconContexts.Larger,        32.0 },
            {IconContexts.ExtraLarge,    40.0 },
            {IconContexts.SuperLarge,    48.0 },
            {IconContexts.Largest,       60.0 },
        };
    }

    /// <summary>
    /// Defines the list of possible values for IconSize
    /// </summary>
    public enum IconContexts
    {
        /// <summary>
        /// Use CustomIconSize property to change the Icon Size to Custom.
        /// </summary>
        Custom,
        /// <summary>
        /// CHanges Fotn size to 10 points.
        /// </summary>
        Smaller,
        /// <summary>
        /// Changes Font size to 12 points.
        /// </summary>
        Small,
        /// <summary>
        /// Changes Font size to 16 points.
        /// </summary>
        Default,
        /// <summary>
        /// Changes Font size to 17 points.
        /// </summary>
        Medium,
        /// <summary>
        /// Changes Font size to 20 points.
        /// </summary>
        MediumLarge,
        /// <summary>
        /// Changes Font size to 24 points.
        /// </summary>
        Large,
        /// <summary>
        /// Changes Font size to 32 points.
        /// </summary>
        Larger,
        /// <summary>
        /// Changes Font size to 40 points.
        /// </summary>
        ExtraLarge,
        /// <summary>
        /// Changes Font size to 48 points.
        /// </summary>
        SuperLarge,
        /// <summary>
        /// Changes Font size to 60 points.
        /// </summary>
        Largest,
    }

    #endregion
}
