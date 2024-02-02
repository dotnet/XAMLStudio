using Microsoft.Language.Xml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

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
    private static readonly Dictionary<string, DependencyProperty> _matchableProperties = new()
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
        { nameof(UIElement.Transitions), UIElement.TransitionsProperty },
        { nameof(UIElement.UseLayoutRounding), UIElement.UseLayoutRoundingProperty },
        { nameof(UIElement.Visibility), UIElement.VisibilityProperty },
        { nameof(UIElement.XYFocusDownNavigationStrategy), UIElement.XYFocusDownNavigationStrategyProperty },
        { nameof(UIElement.XYFocusKeyboardNavigation), UIElement.XYFocusKeyboardNavigationProperty },
        { nameof(UIElement.XYFocusLeftNavigationStrategy), UIElement.XYFocusLeftNavigationStrategyProperty },
        { nameof(UIElement.XYFocusRightNavigationStrategy), UIElement.XYFocusRightNavigationStrategyProperty },
        { nameof(UIElement.XYFocusUpNavigationStrategy), UIElement.XYFocusUpNavigationStrategyProperty },
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
        // TODO: Should do 'Background' but that can be Control.BackgroundProperty or Panel.BackgroundProperty, this may need to be a list of DependencyProperty(s) to try and fetch...
        // Should handle 'Content' of ContentControl
    };

    /// <summary>
    /// These are special helpers which can map short-hand values in XML text like Margin="4" to the actual value of the struct or value in the Framework, for instance in the Margin case Thickness. String input should not be empty.
    /// They should return <c>true</c> if the text matches the value.
    /// </summary>
    private static Dictionary<DependencyProperty, Func<string, object, bool>> _propertyConverters = new()
    {
        // TODO: Names colors will probably come up when we get Foreground/Background above.
        // Could we use this approach for names resources and values as well to parse the resource text, we'd need access to the resources...
        [FrameworkElement.MarginProperty] = (text, value) =>
        {
            // Get the individual values, see https://learn.microsoft.com/uwp/api/windows.ui.xaml.thickness
            if (value is Thickness thickness)
            {
                var parts = text.Split(",");
                switch (parts.Length)
                {
                    case 1: // Uniform
                        if (double.TryParse(parts[0], out var uniform))
                        {
                            return thickness.Left == uniform
                                && thickness.Right == uniform
                                && thickness.Top == uniform
                                && thickness.Bottom == uniform;
                        }
                        break;
                    case 2: // Left/Right, Top/Bottom
                        if (double.TryParse(parts[0], out var leftright)
                            && double.TryParse(parts[1], out var topbottom))
                        {
                            return thickness.Left == leftright
                                && thickness.Right == leftright
                                && thickness.Top == topbottom
                                && thickness.Bottom == topbottom;
                        }
                        break;
                    case 4: // Left, Top, Right, Bottom
                        if (double.TryParse(parts[0], out var left)
                            && double.TryParse(parts[1], out var top)
                            && double.TryParse(parts[2], out var right)
                            && double.TryParse(parts[3], out var bottom))
                        {
                            return thickness.Left == left
                                && thickness.Right == right
                                && thickness.Top == top
                                && thickness.Bottom == bottom;
                        }
                        break;
                    default:
                        // We can't compare, we don't understand this format.
                        return false; 
                }
            }

            // Value isn't a thickness we can't compare.
            return false; 
        },
    };

    private BidirectionalDictionary<IXmlElementSyntax, DependencyObject> _treeMapper = new();

    public XamlXmlTreeCoordinator()
    {
    }

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
            if (DoElementsMatch(node, findElement))
            {
                return node;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++)
            {
                var child = VisualTreeHelper.GetChild(node, i);
                if (!explored.Contains(child) && !_treeMapper.ContainsValue(child))
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
            if (_matchableProperties.TryGetValue(attr.Name, out var depProp) 
                && !string.IsNullOrEmpty(attr.Value))
            {
                // TODO: Check if xml value is binding, if so check if the visual element has a BindingExpression
                //// if (element is FrameworkElement fwe && fwe.GetBindingExpression(depProp))

                var vvalue = element.GetValue(depProp);

                // See if we have a converter for this type of property (like Margin) to check against,
                // otherwise, we just do straight string comparison.
                if (_propertyConverters.TryGetValue(depProp, out var converter))                    
                {
                    if (!converter(attr.Value, vvalue)) return false;
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
}
