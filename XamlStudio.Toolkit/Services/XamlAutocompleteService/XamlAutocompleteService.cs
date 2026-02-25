// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Monaco.Languages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using XamlStudio.Toolkit.Helpers;
using XamlStudio.Toolkit.Models;

namespace XamlStudio.Toolkit.Services;

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
#if UNO
                    items.Add(new CompletionItem(t.Name, "", CompletionItemKind.Class)); // TODO:
#else
                    items.Add(new CompletionItem(t.Name, CompletionItemKind.Class));
#endif
                }
            }
        }

        // Add namespace suggestions
        foreach (var ns in namespaces)
        {
#if UNO
            items.Add(new CompletionItem(ns.Name, "", CompletionItemKind.Module)); // TODO:
#else
            items.Add(new CompletionItem(ns.Name, CompletionItemKind.Module));
#endif
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
#if UNO
                    items.Add(new CompletionItem(t.Name, "", CompletionItemKind.Class));
#else
                    items.Add(new CompletionItem(t.Name, CompletionItemKind.Class));
#endif
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
#if UNO
                items.Add(new CompletionItem(property.Name.Substring(0, property.Name.Length - 8), "", CompletionItemKind.Property));
#else
                // Trim 'Property' off DependencyProperty name.
                items.Add(new CompletionItem(property.Name.Substring(0, property.Name.Length - 8), CompletionItemKind.Property));
#endif
            }
        }
    }

    public void AddValueSuggestions(List<CompletionItem> items, string tagName, string attribute)
    {
        if (tagName.Contains(":"))
        {
            tagName = tagName.Split(':')[1];
        }

        if (XamlRenderService.GetTypeFromName(tagName) is Type type && type != null)
        {
            var prop = type.GetProperties().FirstOrDefault(p => p.Name == attribute);

            if (prop != null && prop.PropertyType.IsSubclassOf(typeof(Enum)))
            {
                foreach (var value in Enum.GetNames(prop.PropertyType))
                {
#if UNO
                    items.Add(new CompletionItem(value, "", CompletionItemKind.Value));
#else
                    items.Add(new CompletionItem(value, CompletionItemKind.Value));
#endif
                }
            }
            // FontWeights...grrr
            /*else if (prop.PropertyType.IsSubclassOf(typeof(ValueType)))
            {
                // For now...
                var e = XamlRenderService.GetTypeFromName(prop.Name + "s");
                if (e != null && e.IsSubclassOf(typeof(Enum)))
                {
                    foreach (var value in Enum.GetNames(e))
                    {
                        items.Add(new CompletionItem(value, CompletionItemKind.Value));
                    }
                }
            }*/
        }
    }

    public static IEnumerable<PropertyInfo> GetDependencyProperties(Type type)
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
