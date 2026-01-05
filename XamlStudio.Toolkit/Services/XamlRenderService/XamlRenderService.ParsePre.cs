// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using XamlStudio.Toolkit.Models;

namespace XamlStudio.Toolkit.Services;

//// Pre-processing of content before trying to load element with XamlReader.

public partial class XamlRenderService
{
    private const string RegexPattern_FirstTagAfterComment = @"(?!(<\?|<!))*<[^<!\?]*?>";
    private const string RegexPattern_ElementName = "<((?<Prefix>\\w+):)?(?<Type>\\w+)";

    private static Regex _initialTagSearcher = new Regex(RegexPattern_FirstTagAfterComment, RegexOptions.Compiled);
    private static Regex _elementNameSearcher = new Regex(RegexPattern_ElementName, RegexOptions.Compiled);

    /// <summary>
    /// Returns the first initial element tag after a comment.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static Match GetInitialElement(string content)
    {
        var match = _initialTagSearcher.Match(content);
        if (match.Success)
        {
            return match;
        }

        return null;
    }

    /// <summary>
    /// Pre-processes raw string to inject any missing xmlns namespaces.  Returns the starting XamlRenderResultContext;
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    internal static void PreProcessXmlns(ref XamlRenderResultContext context, ref XamlRenderSettings settings)
    {
        // Copy our initial content as we can't change it.
        var content = string.Copy(context.Content);

        // Look for the first non-meta, non-comment tag.
        var match = GetInitialElement(content);
        if (match != null && match.Success)
        {
            // Keep track of which namespaces we have to inject.
            var namespaces = new List<XmlnsNamespace>();

            var value = match.Value;

            // See what the type of this tag is and mark it, 
            // as a lot of things care about FrameworkElement level items.
            var type = _elementNameSearcher.Match(value);
            if (type.Success)
            {
                if (type.Groups["Type"].Value == "Application")
                {
                    // Replace content with ResourceDictionary posing as 'Application'
                    content = UnwrapApplicationResourceDictionary(content);
                }

                ////var prefix = type.Groups["Prefix"]?.Value;
                var typename = type.Groups["Type"]?.Value;

                context.ElementType = GetTypeFromName(typename);
                context.IsFrameworkElement = IsFrameworkElement(context.ElementType);
            }

            // TODO: Get existing list of namespaces.

            // Injection site.
            var endOfTag = match.Index + match.Length - 1;

            // Pre-parse and check for xmlns namespaces.           
            if (!content.Contains("xmlns"))
            {
                // Support injecting into a single tag only.
                if (match.Value.EndsWith("/>"))
                {
                    endOfTag--;
                }

                // Add to our list of namespaces we need to add.
                namespaces.Add(new XmlnsNamespace(XmlnsPrefix, XmlnsRequiredPath));
            }

            // Look to see if we're trying to use any namespaces we know about
            foreach (var ns in settings.KnownNamespaces)
            {
                var usage = "<" + ns.Name + ":";
                var included = XmlnsPrefix + ':' + ns.Name;

                if (content.Contains(usage) && !value.Contains(included))
                {
                    // If our first tag doesn't have the namespace, but we see it, then it's missing...
                    namespaces.Add(ns);
                }
            }

            if (namespaces.Count > 0)
            {
                context.HasSuggestion = true;

                var sb = new StringBuilder();

                var i = 0;
                foreach (var ns in namespaces)
                {
                    i++;

                    sb.Append(" ");
                    if (ns.Name != XmlnsPrefix)
                    {
                        sb.Append(XmlnsPrefix);
                        sb.Append(":");
                    }
                    sb.Append(ns.Name);
                    sb.Append("=\"");
                    sb.Append(ns.Path);
                    sb.Append("\"");
                    if (!settings.KeepSuggestedContentSameLength && i < namespaces.Count)
                    {
                        sb.Append(Environment.NewLine);
                        sb.Append("\t"); // TODO: Grab length to end of first space, to make pretty...
                    }
                }

                // Inject xmlns base-path into tag on same line.
                content = content.Substring(0, endOfTag) + sb.ToString() + content.Substring(endOfTag);
            }

            context.DetectedNamespaces = namespaces.ToArray();
        }

        // Update our content holders.
        context.SuggestedContent = content;
        context.RenderedContent = content;
    }

    public static Type GetTypeFromName(string typename)
    {
        // TODO: Do we need to worry about conflicting names across assemblies and pass in the namespace to this function?
        return AppAssemblyInfo.Instance.KnownTypes.FirstOrDefault(t => t.Name == typename);
    }

    internal static bool IsFrameworkElement(Type type)
    {
        if (type == null)
        {
            return false;
        }

        return typeof(FrameworkElement).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
    }

    private static string UnwrapApplicationResourceDictionary(string xaml)
    {
        /// Naive implementation to test concept
        /// TODO: Investigate doing with proper XML Tree
        /// Need to preserve spacing/indentation as much as possible for line number matching
        var lines = xaml.Split(["\r\n", "\n"], StringSplitOptions.None);
        StringBuilder result = new();

        foreach (var line in lines)
        {
            var trimmedLine = line.TrimStart();

            // Skip x:Class line
            // Skip Application.Resources opening tag
            if (trimmedLine.Contains("x:Class=")
                || trimmedLine.StartsWith("<Application.Resources>")
                || trimmedLine.StartsWith("</Application.Resources>"))
            {
                // Add blank line as replacement to preserve line numbers
                result.AppendLine();
                continue;
            }
            // Replace Application opening tag with ResourceDictionary
            else if (trimmedLine.StartsWith("<Application"))
            {
                result.AppendLine(line.Replace("<Application", "<ResourceDictionary"));
                continue;
            }
            // Replace Application closing tag with ResourceDictionary
            else if (trimmedLine.StartsWith("</Application>"))
            {
                result.AppendLine(line.Replace("</Application>", "</ResourceDictionary>"));
                continue;
            }

            result.AppendLine(line);
        }

        return result.ToString().TrimEnd();
    }
}
