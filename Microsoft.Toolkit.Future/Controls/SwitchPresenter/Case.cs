using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Future
{
    /// <summary>
    /// <see cref="Case"/> is the value container for the <see cref="SwitchPresenter"/>.
    /// </summary>
    [ContentProperty(Name = nameof(Content))]
    public class Case : DependencyObject
    {
        internal SwitchPresenter Parent { get; set; }

        public event EventHandler ValueChanged;

        public UIElement Content
        {
            get { return (UIElement)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Content.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(nameof(Content), typeof(UIElement), typeof(Case), new PropertyMetadata(null));

        public bool IsDefault
        {
            get { return (bool)GetValue(IsDefaultProperty); }
            set { SetValue(IsDefaultProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDefault.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDefaultProperty =
            DependencyProperty.Register(nameof(IsDefault), typeof(bool), typeof(Case), new PropertyMetadata(false));

        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(object), typeof(Case), new PropertyMetadata(null, OnValuePropertyChanged));

        public static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var xcase = (Case)d;

            xcase.ValueChanged?.Invoke(xcase, EventArgs.Empty);
        }

        public Case()
        {
        }
    }
}
