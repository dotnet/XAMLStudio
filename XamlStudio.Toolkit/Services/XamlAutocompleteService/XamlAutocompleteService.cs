using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Monaco.Languages;
using Windows.UI.Xaml;
using XamlStudio.Toolkit.Helpers;
using XamlStudio.Toolkit.Models;

namespace XamlStudio.Toolkit.Services
{
    public class XamlAutocompleteService
    {
        public static XamlAutocompleteService Instance => Singleton<XamlAutocompleteService>.Instance;

        private const string RegexPattern_XmlnsNamespace = @"xmlns(|:(?<Prefix>[\w-]*))=""(?<Namespace>.*)""";

        private static Regex _namespaceSearcher = new Regex(RegexPattern_XmlnsNamespace, RegexOptions.Compiled);

        public IEnumerable<XmlnsNamespace> GetNamespaces(string content)
        {
            var match = XamlRenderService.GetInitialElement(content);
            if (match != null && match.Success)
            {
                var namespaces = new List<XmlnsNamespace>();

                foreach (Match m in _namespaceSearcher.Matches(match.Value))
                {
                    if (m.Success)
                    {
                        if (m.Groups["Prefix"].Length > 0)
                        {
                            namespaces.Add(new XmlnsNamespace(m.Groups["Prefix"].Value, m.Groups["Namespace"].Value));
                        }
                    }
                }

                return namespaces;
            }

            return Array.Empty<XmlnsNamespace>();
        }

        public void AddDefaultSuggestions(List<CompletionItem> items, List<XmlnsNamespace> namespaces)
        {
            // Add control suggestions from windows namespace
            var defaultns = new string[] {
                        "Windows.UI.Xaml.Controls",
                        "Windows.UI.Xaml.Media",
                        "Windows.UI.Xaml.Shapes",
                    };

            foreach (var ns in defaultns)
            {
                // TODO: Cache in XamlAutocompleteService
                if (AppAssemblyInfo.Instance.TypesByNamespace.TryGetValue(ns, out var types))
                {
                    foreach (var t in types.Where(t => t.IsSubclassOf(typeof(DependencyObject))))
                    {
                        items.Add(new CompletionItem(t.Name, CompletionItemKind.Class));
                    }
                }
            }

            // Add namespace suggestions
            foreach (var ns in namespaces)
            {
                items.Add(new CompletionItem(ns.Name, CompletionItemKind.Module));
            }
        }

        public void AddNamespaceSuggestions(List<CompletionItem> items, string prefix, List<XmlnsNamespace> namespaces)
        {
            if (namespaces.FirstOrDefault(n => n.Name == prefix) is XmlnsNamespace ns && ns.Path != null && ns.Path.StartsWith("using:"))
            {
                var @namespace = ns.Path.Split(':')[1];

                if (AppAssemblyInfo.Instance.TypesByNamespace.TryGetValue(@namespace, out var types))
                {
                    foreach (var t in types.Where(t => t.IsSubclassOf(typeof(DependencyObject))))
                    {
                        items.Add(new CompletionItem(t.Name, CompletionItemKind.Class));
                    }
                }
            }
        }

        public void AddPropertySuggestions(List<CompletionItem> items, string tagName)
        {
            if (tagName.Contains(":"))
            {
                tagName = tagName.Split(':')[1];
            }

            if (XamlRenderService.GetTypeFromName(tagName) is Type type && type != null)
            {
                foreach (var property in GetDependencyProperties(type))
                {
                    // Trim 'Property' off DependencyProperty name.
                    items.Add(new CompletionItem(property.Name.Substring(0, property.Name.Length - 8), CompletionItemKind.Property));
                }
            }
        }

        private static IEnumerable<PropertyInfo> GetDependencyProperties(Type type)
        {
            var properties = type.GetProperties(BindingFlags.Static | BindingFlags.Public)
                                 .Where(p => p.PropertyType == typeof(DependencyProperty));

            if (type.BaseType != null)
            {
                properties = properties.Union(GetDependencyProperties(type.BaseType));
            }

            return properties;
        }
    }
}
