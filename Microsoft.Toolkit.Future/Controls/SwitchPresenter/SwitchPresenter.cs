using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace Microsoft.Toolkit.Uwp.UI.Controls.Future
{
    [TemplatePart(Name = ContentPresenterPartName, Type = typeof(ContentPresenter))]
    [ContentProperty(Name = nameof(SwitchCases))]
    public sealed class SwitchPresenter : Control
    {
        public Case CurrentCase
        {
            get { return (Case)GetValue(CurrentCaseProperty); }
            set { SetValue(CurrentCaseProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentCase.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentCaseProperty =
            DependencyProperty.Register(nameof(CurrentCase), typeof(Case), typeof(SwitchPresenter), new PropertyMetadata(null));

        public CaseCollection SwitchCases
        {
            get { return (CaseCollection)GetValue(SwitchCasesProperty); }
            set { SetValue(SwitchCasesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SwitchCases.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SwitchCasesProperty =
            DependencyProperty.Register(nameof(SwitchCases), typeof(CaseCollection), typeof(SwitchPresenter), new PropertyMetadata(null, new PropertyChangedCallback(OnSwitchCasesPropertyChanged)));

        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(object), typeof(SwitchPresenter), new PropertyMetadata(null, new PropertyChangedCallback(OnValuePropertyChanged)));

        private static void OnSwitchCasesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                ((SwitchPresenter)e.OldValue).SwitchCases.CaseCollectionChanged -= OnCaseValuePropertyChanged;
            }

            var xswitch = (SwitchPresenter)d;

            foreach (var xcase in xswitch.SwitchCases)
            {
                // Set our parent
                xcase.Parent = xswitch;
            }

            // Will trigger on collection change and case value changed
            xswitch.SwitchCases.Parent = xswitch;
            xswitch.SwitchCases.CaseCollectionChanged += OnCaseValuePropertyChanged;
        }

        private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // When our Switch's expression changes, re-evaluate.
            var xswitch = (SwitchPresenter)d;

            xswitch.EvaluateCases();
        }

        private static void OnCaseValuePropertyChanged(object sender, EventArgs e)
        {
            // When something about our collection of cases changes, re-evaluate.
            var collection = (CaseCollection)sender;

            collection.Parent.EvaluateCases();
        }

        private const string ContentPresenterPartName = "PART_SwitchContentPresenter";

        private ContentPresenter _contentPresenter;

        public SwitchPresenter()
        {
            this.DefaultStyleKey = typeof(SwitchPresenter);

            this.SwitchCases = new CaseCollection();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _contentPresenter = this.GetTemplateChild(ContentPresenterPartName) as ContentPresenter;

            EvaluateCases();
        }

        private void EvaluateCases()
        {
            // Abort if we have no target.
            if (_contentPresenter == null)
            {
                return;
            }

            if (CurrentCase != null && 
                CurrentCase.Value != null && 
                CurrentCase.Value.Equals(Value))
            {
                // If the current case we're on already matches our current value, 
                // then we don't have any work to do.
                return;
            }

            Case xdefault = null;
            Case newcase = null;

            foreach (var xcase in SwitchCases)
            {
                if (xcase.IsDefault)
                {
                    if (xdefault != null)
                    {
                        throw new ArgumentException("Too many default cases provided for SwitchPresenter.");
                    }

                    xdefault = xcase;
                    continue;
                }

                if (xcase.Value != null && xcase.Value.Equals(Value))
                {
                    newcase = xcase;
                    break;
                }
            }

            if (newcase == null && xdefault != null)
            {
                // Inject default if we found one.
                newcase = xdefault;
            }

            // Only bother changing things around if we have a new case.
            if (newcase != CurrentCase)
            {
                // Disconnect old content from visual tree.
                if (CurrentCase != null && CurrentCase.Content != null)
                {
                    // TODO: If we disconnect here, we need to recreate later?
                    // Provide Option?
                    //VisualTreeHelper.DisconnectChildrenRecursive(CurrentCase.Content);
                }

                // Hookup new content.
                _contentPresenter.Content = newcase.Content;

                CurrentCase = newcase;
            }
        }
    }
}
