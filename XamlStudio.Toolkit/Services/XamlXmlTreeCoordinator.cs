// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Language.Xml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace XamlStudio.Toolkit.Services;

/// <summary>
/// Class to help coordinate Xml Parse Tree and XAML Visual Tree elements.
/// </summary>
public class XamlXmlTreeCoordinator
{
    /// <summary>
    /// Dictionary that maps a string name that will be seen in XML for an Attribute of a property to a DependencyProperty.
    /// These are used to help identify and align Visual tree elements to their XML counterparts.
    /// </summary>
    public static ReadOnlyDictionary<Type, ReadOnlyDictionary<string, DependencyProperty>> AttributeNameToDependencyProperty { get; } = new(new Dictionary<Type, ReadOnlyDictionary<string, DependencyProperty>>()
    {
        { typeof(UIElement), new(new Dictionary<string, DependencyProperty>()
        {
            { nameof(UIElement.AccessKey), UIElement.AccessKeyProperty },
            { nameof(UIElement.AccessKeyScopeOwner), UIElement.AccessKeyScopeOwnerProperty },
            { nameof(UIElement.AllowDrop), UIElement.AllowDropProperty },
            { nameof(UIElement.CacheMode), UIElement.CacheModeProperty },
            { nameof(UIElement.CanBeScrollAnchor), UIElement.CanBeScrollAnchorProperty },
            { nameof(UIElement.CanDrag), UIElement.CanDragProperty },
            { nameof(UIElement.CompositeMode), UIElement.CanBeScrollAnchorProperty },
            { nameof(UIElement.ContextFlyout), UIElement.ContextFlyoutProperty },
            { nameof(UIElement.ExitDisplayModeOnAccessKeyInvoked), UIElement.ExitDisplayModeOnAccessKeyInvokedProperty },
            { nameof(UIElement.HighContrastAdjustment), UIElement.HighContrastAdjustmentProperty },
            { nameof(UIElement.IsAccessKeyScope), UIElement.IsAccessKeyScopeProperty },
            { nameof(UIElement.IsDoubleTapEnabled), UIElement.IsDoubleTapEnabledProperty },
            { nameof(UIElement.IsHitTestVisible), UIElement.IsHitTestVisibleProperty },
            { nameof(UIElement.IsHoldingEnabled), UIElement.IsHoldingEnabledProperty },
            { nameof(UIElement.IsRightTapEnabled), UIElement.IsRightTapEnabledProperty },
            { nameof(UIElement.IsTapEnabled), UIElement.IsTapEnabledProperty },
            { nameof(UIElement.KeyboardAcceleratorPlacementMode), UIElement.KeyboardAcceleratorPlacementModeProperty },
            { nameof(UIElement.KeyboardAcceleratorPlacementTarget), UIElement.KeyboardAcceleratorPlacementTargetProperty },
            { nameof(UIElement.KeyTipHorizontalOffset), UIElement.KeyTipHorizontalOffsetProperty },
            { nameof(UIElement.KeyTipPlacementMode), UIElement.KeyTipPlacementModeProperty },
            { nameof(UIElement.KeyTipTarget), UIElement.KeyTipTargetProperty },
            { nameof(UIElement.KeyTipVerticalOffset), UIElement.KeyTipVerticalOffsetProperty },
            { nameof(UIElement.Lights), UIElement.LightsProperty },
            { nameof(UIElement.ManipulationMode), UIElement.ManipulationModeProperty },
            { nameof(UIElement.Opacity), UIElement.OpacityProperty },
            { nameof(UIElement.PointerCaptures), UIElement.PointerCapturesProperty },
            { nameof(UIElement.Projection), UIElement.ProjectionProperty },
            { nameof(UIElement.RenderTransform), UIElement.RenderTransformProperty },
            { nameof(UIElement.RenderTransformOrigin), UIElement.RenderTransformOriginProperty },
            { nameof(UIElement.Shadow), UIElement.ShadowProperty },
            { nameof(UIElement.Transform3D), UIElement.Transform3DProperty },
#if !UNO
            { nameof(UIElement.Transitions), UIElement.TransitionsProperty }, // TODO: https://github.com/unoplatform/uno/issues/22288
#endif
            { nameof(UIElement.UseLayoutRounding), UIElement.UseLayoutRoundingProperty },
            { nameof(UIElement.Visibility), UIElement.VisibilityProperty },
            { nameof(UIElement.XYFocusDownNavigationStrategy), UIElement.XYFocusDownNavigationStrategyProperty },
            { nameof(UIElement.XYFocusKeyboardNavigation), UIElement.XYFocusKeyboardNavigationProperty },
            { nameof(UIElement.XYFocusLeftNavigationStrategy), UIElement.XYFocusLeftNavigationStrategyProperty },
            { nameof(UIElement.XYFocusRightNavigationStrategy), UIElement.XYFocusRightNavigationStrategyProperty },
            { nameof(UIElement.XYFocusUpNavigationStrategy), UIElement.XYFocusUpNavigationStrategyProperty },
        })},
        { typeof(Control), new(new Dictionary<string, DependencyProperty>()
        {
            { nameof(Control.Background), Control.BackgroundProperty },
            { nameof(Control.BackgroundSizing), Control.BackgroundSizingProperty },
            { nameof(Control.BorderBrush), Control.BorderBrushProperty },
            { nameof(Control.BorderThickness), Control.BorderThicknessProperty },
            { nameof(Control.CharacterSpacing), Control.CharacterSpacingProperty },
            { nameof(Control.CornerRadius), Control.CornerRadiusProperty },
            { nameof(Control.ElementSoundMode), Control.ElementSoundModeProperty },
            { nameof(Control.FocusState), Control.FocusStateProperty },
            { nameof(Control.FontFamily), Control.FontFamilyProperty },
            { nameof(Control.FontSize), Control.FontSizeProperty },
            { nameof(Control.FontStretch), Control.FontStretchProperty },
            { nameof(Control.FontStyle), Control.FontStyleProperty },
            { nameof(Control.FontWeight), Control.FontWeightProperty },
            { nameof(Control.Foreground), Control.ForegroundProperty },
            { nameof(Control.HorizontalContentAlignment), Control.HorizontalContentAlignmentProperty },
            { nameof(Control.IsEnabled), Control.IsEnabledProperty },
            { nameof(Control.IsFocusEngaged), Control.IsFocusEngagedProperty },
            { nameof(Control.IsFocusEngagementEnabled), Control.IsFocusEngagementEnabledProperty },
            { nameof(Control.IsTabStop), Control.IsTabStopProperty },
            // TODO: IsTemplateFocus/IsTextScaleFactor
            { nameof(Control.Padding), Control.PaddingProperty },
            // TODO: RequiresPointer
            { nameof(Control.TabIndex), Control.TabIndexProperty },
            // TODO: TabNavigation/UseSystemFocusVisuals
            { nameof(Control.VerticalContentAlignment), Control.VerticalContentAlignmentProperty },
            // TODO: XYFocus
        })},
        { typeof(FrameworkElement), new(new Dictionary<string, DependencyProperty>()
        {
            // Actual Height/Width (not set)
            { nameof(FrameworkElement.AllowFocusOnInteraction), FrameworkElement.AllowFocusOnInteractionProperty },
            { nameof(FrameworkElement.AllowFocusWhenDisabled), FrameworkElement.AllowFocusWhenDisabledProperty },
            // DataContext... too complex?
            { nameof(FrameworkElement.FlowDirection), FrameworkElement.FlowDirectionProperty },
            { nameof(FrameworkElement.FocusVisualMargin), FrameworkElement.FocusVisualMarginProperty },
            // TODO: FocusVisual... Brushes
            { nameof(FrameworkElement.Height), FrameworkElement.HeightProperty },
            { nameof(FrameworkElement.HorizontalAlignment), FrameworkElement.HorizontalAlignmentProperty },
            { nameof(FrameworkElement.Language), FrameworkElement.LanguageProperty },
            { nameof(FrameworkElement.Margin), FrameworkElement.MarginProperty },
            { nameof(FrameworkElement.MaxHeight), FrameworkElement.MaxHeightProperty },
            { nameof(FrameworkElement.MaxWidth), FrameworkElement.MaxWidthProperty },
            { nameof(FrameworkElement.MinHeight), FrameworkElement.MinHeightProperty },
            { nameof(FrameworkElement.MinWidth), FrameworkElement.MinWidthProperty },
            // Note: Name is special cased below due to XML x:Name usage...
            { nameof(FrameworkElement.RequestedTheme), FrameworkElement.RequestedThemeProperty },
            // Style... probably need special case???
            { nameof(FrameworkElement.Tag), FrameworkElement.TagProperty },
            { nameof(FrameworkElement.VerticalAlignment), FrameworkElement.VerticalAlignmentProperty },
            { nameof(FrameworkElement.Width), FrameworkElement.WidthProperty },
        })},
        { typeof(Image), new (new Dictionary<string, DependencyProperty>()
        {
            { nameof(Image.NineGrid), Image.NineGridProperty },
            // PlayToSource obsolete?
            { nameof(Image.Source), Image.SourceProperty },
            { nameof(Image.Stretch), Image.StretchProperty },
        })},
        { typeof(Border), new (new Dictionary<string, DependencyProperty>()
        {
            { nameof(Border.Background), Border.BackgroundProperty },
            { nameof(Border.BackgroundSizing), Border.BackgroundSizingProperty },
            { nameof(Border.BorderBrush), Border.BorderBrushProperty },
            { nameof(Border.BorderThickness), Border.BorderThicknessProperty },
            { nameof(Border.CornerRadius), Border.CornerRadiusProperty },
            { nameof(Border.Padding), Border.PaddingProperty },
        })},
        { typeof(Grid), new (new Dictionary<string, DependencyProperty>()
        {
            { nameof(Grid.BackgroundSizing), Grid.BackgroundSizingProperty },
            { nameof(Grid.BorderBrush), Grid.BorderBrushProperty },
            { nameof(Grid.BorderThickness), Grid.BorderThicknessProperty },
            { nameof(Grid.ColumnSpacing), Grid.ColumnSpacingProperty },
            { nameof(Grid.CornerRadius), Grid.CornerRadiusProperty },
            { nameof(Grid.Padding), Grid.PaddingProperty },
            { nameof(Grid.RowSpacing), Grid.RowSpacingProperty },
        })},
        { typeof(Panel), new (new Dictionary<string, DependencyProperty>()
        {
            { nameof(Panel.Background), Panel.BackgroundProperty },
            // ChildTransitions
            { nameof(Panel.IsItemsHost), Panel.IsItemsHostProperty },
        })},
        { typeof(RadioButton), new (new Dictionary<string, DependencyProperty>()
        {
            { nameof(RadioButton.GroupName), RadioButton.GroupNameProperty },
        })},
        { typeof(StackPanel), new (new Dictionary<string, DependencyProperty>()
        {
            { nameof(StackPanel.AreScrollSnapPointsRegular), StackPanel.AreScrollSnapPointsRegularProperty },
            { nameof(StackPanel.BackgroundSizing), StackPanel.BackgroundSizingProperty },
            { nameof(StackPanel.BorderBrush), StackPanel.BorderBrushProperty },
            { nameof(StackPanel.CornerRadius), StackPanel.CornerRadiusProperty },
            { nameof(StackPanel.Orientation), StackPanel.OrientationProperty },
            { nameof(StackPanel.Padding), StackPanel.PaddingProperty },
            { nameof(StackPanel.Spacing), StackPanel.SpacingProperty },
        })},
        { typeof(TextBlock), new(new Dictionary<string, DependencyProperty>()
        {
            { nameof(TextBlock.CharacterSpacing), TextBlock.CharacterSpacingProperty },
            { nameof(TextBlock.FontFamily), TextBlock.FontFamilyProperty },
            { nameof(TextBlock.FontSize), TextBlock.FontSizeProperty },
            { nameof(TextBlock.FontStretch), TextBlock.FontStretchProperty },
            { nameof(TextBlock.FontStyle), TextBlock.FontStyleProperty },
            { nameof(TextBlock.FontWeight), TextBlock.FontWeightProperty },
            { nameof(TextBlock.Foreground), TextBlock.ForegroundProperty },
            { nameof(TextBlock.HorizontalTextAlignment), TextBlock.HorizontalTextAlignmentProperty },
            // Is properties...
            { nameof(TextBlock.Padding), TextBlock.PaddingProperty },
            { nameof(TextBlock.Text), TextBlock.TextProperty },
            // Other Text properties
        })},
        { typeof(ToggleButton), new(new Dictionary<string, DependencyProperty>()
        {
            { nameof(ToggleButton.IsChecked), ToggleButton.IsCheckedProperty },
            { nameof(ToggleButton.IsThreeState), ToggleButton.IsThreeStateProperty },
        })},
        { typeof(ItemsControl), new(new Dictionary<string, DependencyProperty>()
        {
            { nameof(ItemsControl.DisplayMemberPath), ItemsControl.DisplayMemberPathProperty },
            { nameof(ItemsControl.GroupStyleSelector), ItemsControl.GroupStyleSelectorProperty },
            { nameof(ItemsControl.IsGrouping), ItemsControl.IsGroupingProperty },
            { nameof(ItemsControl.ItemContainerStyle), ItemsControl.ItemContainerStyleProperty },
            { nameof(ItemsControl.ItemContainerStyleSelector), ItemsControl.ItemContainerStyleSelectorProperty },
            { nameof(ItemsControl.ItemsPanel), ItemsControl.ItemsPanelProperty },
            { nameof(ItemsControl.ItemsSource), ItemsControl.ItemsSourceProperty },
            { nameof(ItemsControl.ItemTemplate), ItemsControl.ItemTemplateProperty },
            { nameof(ItemsControl.ItemTemplateSelector), ItemsControl.ItemTemplateSelectorProperty },
        })},
        { typeof(Selector), new(new Dictionary<string, DependencyProperty>()
        {
            { nameof(Selector.IsSynchronizedWithCurrentItem), Selector.IsSynchronizedWithCurrentItemProperty },
            { nameof(Selector.SelectedIndex), Selector.SelectedIndexProperty },
            { nameof(Selector.SelectedItem), Selector.SelectedItemProperty },
            { nameof(Selector.SelectedValue), Selector.SelectedValueProperty },
            { nameof(Selector.SelectedValuePath), Selector.SelectedValuePathProperty },
        })},
        { typeof(ListViewBase), new(new Dictionary<string, DependencyProperty>()
        {
            { nameof(ListViewBase.CanDragItems), ListViewBase.CanDragItemsProperty },
            { nameof(ListViewBase.CanReorderItems), ListViewBase.CanReorderItemsProperty },
            { nameof(ListViewBase.DataFetchSize), ListViewBase.DataFetchSizeProperty },
            { nameof(ListViewBase.Footer), ListViewBase.FooterProperty },
            { nameof(ListViewBase.FooterTemplate), ListViewBase.FooterTemplateProperty },
            { nameof(ListViewBase.FooterTransitions), ListViewBase.FooterTransitionsProperty },
            { nameof(ListViewBase.Header), ListViewBase.HeaderProperty },
            { nameof(ListViewBase.HeaderTemplate), ListViewBase.HeaderTemplateProperty },
            { nameof(ListViewBase.HeaderTransitions), ListViewBase.HeaderTransitionsProperty },
            { nameof(ListViewBase.IsActiveView), ListViewBase.IsActiveViewProperty },
            { nameof(ListViewBase.IsItemClickEnabled), ListViewBase.IsItemClickEnabledProperty },
            { nameof(ListViewBase.IsMultiSelectCheckBoxEnabled), ListViewBase.IsMultiSelectCheckBoxEnabledProperty },
            { nameof(ListViewBase.IsSwipeEnabled), ListViewBase.IsSwipeEnabledProperty },
            { nameof(ListViewBase.IsZoomedInView), ListViewBase.IsZoomedInViewProperty },
            { nameof(ListViewBase.ReorderMode), ListViewBase.ReorderModeProperty },
            { nameof(ListViewBase.SelectionMode), ListViewBase.SelectionModeProperty },
            { nameof(ListViewBase.SemanticZoomOwner), ListViewBase.SemanticZoomOwnerProperty },
            { nameof(ListViewBase.ShowsScrollingPlaceholders), ListViewBase.ShowsScrollingPlaceholdersProperty },
            { nameof(ListViewBase.SingleSelectionFollowsFocus), ListViewBase.SingleSelectionFollowsFocusProperty },
        })},
    });

    /// <summary>
    /// There are some types which get converter to more complex types, but we want to compare the intent of the values instead...
    /// </summary>
    private static Dictionary<Type, Func<object, object, bool>> _comparators = new()
    {
        // For Image.Source for instance
        [typeof(BitmapImage)] = (v1, v2) =>
        {
            if (v1 is BitmapImage b1
                && v2 is BitmapImage b2)
            {
                // TODO: Added null check, but feel like stale object is getting passed in here sometimes on 2nd render?
                if (b1.UriSource?.AbsoluteUri == b2.UriSource?.AbsoluteUri)
                {
                    return true;
                }
                // With the XamlBindingHelper.ConvertValue we see it convert differently than the XamlReader.Load, so use this in case the resource scheme was picked differently.
                else if ((b1.UriSource?.Scheme == "ms-appx" && b2.UriSource?.Scheme == "ms-resource") ||
                         (b1.UriSource?.Scheme == "ms-resource" && b2.UriSource?.Scheme == "ms-appx"))
                {
                    var path1 = b1.UriSource?.AbsoluteUri.Replace("ms-appx:///", "").Replace("ms-resource:///Files/", "");
                    var path2 = b2.UriSource?.AbsoluteUri.Replace("ms-appx:///", "").Replace("ms-resource:///Files/", "");
                    return path1 == path2;
                }
            }

            return false;
        },
        [typeof(SolidColorBrush)] = (v1, v2) =>
        {
            if (v1 is SolidColorBrush s1
                && v2 is SolidColorBrush s2
                && s1.Color == s2.Color
                && s1.Opacity == s2.Opacity)
            {
                return true;
            }

            return false;
        }
    };

    private BidirectionalDictionary<IXmlElementSyntax, DependencyObject> _treeMapper = new();

    public int Count => _treeMapper.Count;

    public XamlXmlTreeCoordinator()
    {
    }

    public IReadOnlyCollection<DependencyObject> GetVisualElements() => _treeMapper.Values;

    public IReadOnlyCollection<IXmlElementSyntax> GetXmlElements() => _treeMapper.Keys;

    public bool TryGetVisualElement(IXmlElementSyntax node, out DependencyObject element)
    {
        return _treeMapper.TryGetValue(node, out element);
    }

    public bool TryGetXmlElement(DependencyObject element, out IXmlElementSyntax node)
    {
        return _treeMapper.Inverse.TryGetValue(element, out node);
    }

    //// TODO: Not sure how long this takes, so maybe wrap in a Task? Though does require UIThread...hmmm
    public void Initialize(XmlDocumentSyntax docRoot, DependencyObject visualRoot)
    {
        _treeMapper.Clear();
        ////_treeMapper.Add(docRoot, visualRoot);

        /* --- NOTES ---
         * 
         * We do make some assumptions that assume the order of children within the tree will be consistent.
         * i.e. the first and second child listed in XML will be the first and second child in the Visual Tree child collection.
         * 
         */

        // Map entire Xml Tree
        foreach (var child in docRoot.DescendantsAndSelf())
        {
            // When we find a match, try and use that parent visual element as the new root for further sub-searching
            var searchRoot = visualRoot;
            if (child.Parent != null && _treeMapper.TryGetValue(child.Parent, out var element))
            {
                searchRoot = element;
            }

            // TODO: How do we want to handle Resources/Styles of things as they don't have a direct visual?

            var uie = FindMatchingUIElement(searchRoot, child);

            if (uie != null)
            {
                _treeMapper.Add(child, uie);
            }
            else
            {
                // TODO: There are a lot of cases which fall into here, resources, styles
                // but also complex attribute setting (e.g. RowDefinitions of Grid)
                // TODO: Ignorable list to put above?
#if DEBUG
                // Something went wrong... not quite yet
                //// Debugger.Break();
#endif
            }
        }
    }

    /// <summary>
    /// Uses Breadth-first search to look for the matching UIElement of the given syntax node.
    /// </summary>
    /// <param name="visualNode"></param>
    /// <param name="findElement"></param>
    /// <returns></returns>
    private DependencyObject FindMatchingUIElement(DependencyObject visualNode, IXmlElementSyntax findElement)
    {
        // TODO: Think about or time against a DFS instead to see which is more efficient, not sure if case-by-case.
        // Or maybe if we use DFS with a limited depth, as theoretically we shouldn't have to dig far if we're snapping above
        Queue<DependencyObject> queue = new();
        HashSet<DependencyObject> explored = new()
        {
            visualNode
        };
        queue.Enqueue(visualNode);
        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            if (!_treeMapper.ContainsValue(node)
                && DoElementsMatch(node, findElement))
            {
                return node;
            }

            // Note: ResourceDictionary's don't have a visual tree so the VisualTreeHelper call fails below
            // TODO: Investigate if we can better integrate into ResourceViewer control in the future.
            if (node is ResourceDictionary) continue;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++)
            {
                var child = VisualTreeHelper.GetChild(node, i);

                if (!explored.Contains(child)
                    && !_treeMapper.ContainsValue(child))
                {
                    explored.Add(child);
                    queue.Enqueue(child);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// This method uses a cascading set of negative criteria in order to determine if a Visual element matches an Xml Element.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="xml"></param>
    /// <returns><c>true</c> if we have high-confidence that these elements match based on exclusion criteria</returns>
    public static bool DoElementsMatch(DependencyObject element, IXmlElementSyntax xml)
    {
        // Check if this type matches our XML node name
        if (element.GetType().Name != xml.Name) return false;

        // Check for the x:Name property match (if one)
        var name = element.ReadLocalValue(FrameworkElement.NameProperty);
        if (name != DependencyProperty.UnsetValue && !string.IsNullOrEmpty((string)name))
        {
            var xname = xml.GetAttributeValue("Name", "x");
            if (string.IsNullOrEmpty(xname))
            {
                // Fallback for folks who don't realize they should use x:Name over Name.
                xname = xml.GetAttributeValue("Name");
            }

            if (!name.Equals(xname)) return false;
        }

        // Look through attributes defined in XML to see if they match our visual element
        // TODO: Note these values may get changed at run time later (bindings, property editor), so not sure how we want to handle that...
        foreach (var attr in xml.Attributes)
        {
            // TODO: Should we check if attr.Value is empty that there's no value/default for the element?
            if (TryGetValueForPropertyByTypeAndString(element.GetType(), attr.Name, out var depProp)
                && !string.IsNullOrEmpty(attr.Value)
                && !attr.Value.StartsWith("{Binding") // TODO: Need to handle these scenarios for matching somehow...
                && !attr.Value.StartsWith("{StaticResource")
                && !attr.Value.StartsWith("{ThemeResource"))
            {
                // TODO: Check if xml value is binding, if so check if the visual element has a BindingExpression
                //// if (element is FrameworkElement fwe && fwe.GetBindingExpression(depProp))

                var vvalue = element.ReadLocalValue(depProp);

                // See if we have a converter for this type of property (like Margin) to check against,
                // otherwise, we just do straight string comparison.
                //// TODO: Can we just use XamlBindingHelper here for everything?
                if (vvalue != DependencyProperty.UnsetValue)
                {
                    var nvalue = DependencyProperty.UnsetValue;
                    try
                    {
                        // TODO: Deal with theme resources?
                        nvalue = XamlBindingHelper.ConvertValue(vvalue.GetType(), attr.Value);
                    }
                    catch { }

                    if (_comparators.TryGetValue(vvalue.GetType(), out var comparator))
                    {
                        if (!comparator(vvalue, nvalue)) return false;
                    }
                    else if (!vvalue.Equals(nvalue))
                    {
                        return false;
                    }
                }
                else if (attr.Value != vvalue.ToString())
                {
                    return false;
                }
            }
        }

        // TODO: Should look at Logical Tree helpers parent and child elements too, maybe? Not sure if overkill.
        // Thinking of two plain elements though like Buttons with no other content yet though... index important

        // We've survived the gauntlet of matching different criteria, so we must be a match...
        return true;
    }

    /// <summary>
    /// Walks the list of type inheritance and tries to find the matching property by name to get the DependencyProperty.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <param name="property"></param>
    /// <returns></returns>
    private static bool TryGetValueForPropertyByTypeAndString(Type type, string name, out DependencyProperty? property)
    {
        // Double-check incoming type is a UI type.
        if (!typeof(DependencyObject).IsAssignableFrom(type))
        {
            property = null;
            return false;
        }

        var t = type;
        do
        {
            if (AttributeNameToDependencyProperty.TryGetValue(t, out var properties) &&
                properties.TryGetValue(name, out var depProp))
            {
                property = depProp;
                return true;
            }
            t = t.BaseType;
        } while (t != typeof(DependencyObject)); // All UI Controls should inherit from this, so we can stop there.

        property = null;
        return false;
    }
}
