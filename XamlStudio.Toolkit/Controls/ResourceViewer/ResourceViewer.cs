using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using XamlStudio.Toolkit.Extensions;
using XamlStudio.Toolkit.Services;

namespace XamlStudio.Toolkit.Controls
{
    public sealed class ResourceViewer : Control
    {
        public ResourceDictionary ResourceDictionary
        {
            get { return (ResourceDictionary)GetValue(ResourceDictionaryProperty); }
            set { SetValue(ResourceDictionaryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ResourceDictionary.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ResourceDictionaryProperty =
            DependencyProperty.Register(nameof(ResourceDictionary), typeof(ResourceDictionary), typeof(ResourceViewer), new PropertyMetadata(null));

        public ObservableCollection<ResourceKeyInfo> ResourceKeys
        {
            get { return (ObservableCollection<ResourceKeyInfo>)GetValue(ResourceKeysProperty); }
            set { SetValue(ResourceKeysProperty, value); }
        }

        public XDocument XmlDocument { get; set; }

        // Using a DependencyProperty as the backing store for ResourceKeys.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ResourceKeysProperty =
            DependencyProperty.Register(nameof(ResourceKeys), typeof(ObservableCollection<ResourceKeyInfo>), typeof(ResourceViewer), new PropertyMetadata(new ObservableCollection<ResourceKeyInfo>()));

        // TODO: Can I expose this here and use in template somehow?
        /*public DataTemplateSelector ResourceTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(ResourceTemplateSelectorProperty); }
            set { SetValue(ResourceTemplateSelectorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ResourceTemplateSelector.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ResourceTemplateSelectorProperty =
            DependencyProperty.Register(nameof(ResourceTemplateSelector), typeof(DataTemplateSelector), typeof(ResourceViewer), new PropertyMetadata(null));*/

        public ResourceViewer()
        {
            DefaultStyleKey = typeof(ResourceViewer);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            LoadResources(ResourceDictionary);
        }

        public void LoadResources(ResourceDictionary resources)
        {
            ResourceKeys.Clear();

            foreach (var kvp in resources)
            {
                var keyinfo = new ResourceKeyInfo(kvp.Key.ToString(), kvp.Value);

                if (keyinfo.Value is Style style && style.TargetType != null)
                {
                    if (Activator.CreateInstance(style.TargetType) is FrameworkElement obj)
                    {
                        // TODO: Add lorem ipsum to TextBlock? (If Text Property not set in style???)

                        obj.Style = style;
                        keyinfo.DisplayedValueControl = obj;

                        if (style.TargetType.GetProperty("Text") is PropertyInfo text)
                        {
                            text.SetValue(obj, "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Praesent vulputate magna.");
                        }
                        else if (style.TargetType.GetProperty("Content") is PropertyInfo content)
                        {
                            content.SetValue(obj, "Test Content");
                        }
                    }
                }
                // TODO: Look at xml parsing to grab key/datatype for DataTemplate?

                ResourceKeys.Add(keyinfo);
            }

            if (XmlDocument != null)
            {
                // Lookup all x:Key attributes (or Type names if no key)
                var keys = XmlDocument.Root.Elements().Select(el => GetResourceKey(el)).ToList();

                // Sort ResourceKeys by the XML ordering
                var rsrc = keys.Join(
                                    ResourceKeys,
                                    i => i,
                                    d => d.Key,
                                    (i, d) => d).ToArray();

                ResourceKeys.Clear();

                foreach (var res in rsrc)
                {
                    ResourceKeys.Add(res);
                }
            }
        }

        private string GetResourceKey(XElement el)
        {
            // If it has a key, use that!
            var key = el.Attributes().GetNamedItem("{http://schemas.microsoft.com/winfx/2006/xaml}Key")?.Value;

            if (key == null)
            {
                // Otherwise, most likely we have a target type
                key = el.Attributes().GetNamedItem("TargetType")?.Value;

                if (key != null)
                {
                    // Get the Namespace or use default
                    var ns = string.Empty;
                    if (key.IndexOf(':') != -1)
                    {
                        var parts = key.Split(':');
                        ns = parts[0];
                        key = parts[1];
                    }
                    else
                    {
                        ns = "Windows.UI.Xaml.Controls";
                    }

                    // Look up type to get fully-qualified type name
                    // TODO: We need to use short friendly namespace here...
                    // Ugh.
                    if (AppAssemblyInfo.Instance.TypesByNamespace.TryGetValue(ns, out var types))
                    {
                        var type = types.FirstOrDefault(t => t.Name == key);
                        if (type != null)
                        {
                            key = type.FullName;
                        }
                    }

                }
            }

            return key;
        }
    }
}
