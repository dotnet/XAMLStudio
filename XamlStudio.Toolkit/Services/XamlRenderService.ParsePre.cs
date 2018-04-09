
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using XamlStudio.Toolkit.Models;

namespace XamlStudio.Toolkit.Services
{
    //// Pre-processing of content before trying to load element with XamlReader.
    
    public partial class XamlRenderService
    {
        private const string RegexPattern_FirstTagAfterComment = @"^.*?(?!(<\?|<!))<.*?>";
        private const string RegexPattern_ElementName = "<(?<Type>\\w+)";

        private static Regex _initialTagSearcher = new Regex(RegexPattern_FirstTagAfterComment, RegexOptions.Compiled);
        private static Regex _elementNameSearcher = new Regex(RegexPattern_ElementName, RegexOptions.Compiled);

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
            var match = _initialTagSearcher.Match(content);
            if (match.Success)
            {
                // Keep track of which namespaces we have to inject.
                var namespaces = new List<(string name, string path)>();

                var value = match.Value;

                // See what the type of this tag is and mark it, 
                // as a lot of things care about FrameworkElement level items.
                var type = _elementNameSearcher.Match(value);
                if (type.Success)
                {
                    var typename = type.Groups["Type"]?.Value;

                    context.ElementType = GetTypeFromName(typename);
                    context.IsFrameworkElement = IsFrameworkElement(context.ElementType);
                }

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
                    namespaces.Add((XmlnsPrefix, XmlnsRequiredPath));
                }
                
                // Look to see if we're trying to use any namespaces we know about
                foreach (var ns in settings.KnownNamespaces)
                {
                    var lookup = XmlnsPrefix + ':' + ns.Key;
                    if (content.Contains(lookup) && !value.Contains(lookup))
                    {
                        // If our first tag doesn't have the namespace, but we see it, then it's missing...
                        namespaces.Add((ns.Key, ns.Value));
                    }
                }

                if (namespaces.Count > 0)
                {
                    var sb = new StringBuilder();

                    var i = 0;
                    foreach (var ns in namespaces)
                    {
                        i++;

                        sb.Append(" ");
                        sb.Append(ns.name);
                        sb.Append("=\"");
                        sb.Append(ns.path);
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
            }

            // Update our content holders.
            context.SuggestedContent = content;
            context.RenderedContent = content;
        }

        public static Type GetTypeFromName(string typename)
        {
            // TODO: Do we need to look at other assemblies for custom framemwork element types of other libs?

            // Look for other UI controls in Main Assembly.
            return typeof(FrameworkElement).GetTypeInfo().Assembly.GetTypes().FirstOrDefault(type => type.Name == typename);
        }

        internal static bool IsFrameworkElement(Type type)
        {
            if (type == null)
            {
                return false;
            }

            return typeof(FrameworkElement).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
        }
    }
}
