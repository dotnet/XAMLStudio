
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private const string RegexPattern_FirstTagAfterComment = @"^.*?(?!(<\?|<!))<[^<]*?>";
        private const string RegexPattern_ElementName = "<((?<Prefix>\\w+):)?(?<Type>\\w+)";

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
                var namespaces = new List<XmlnsNamespace>();

                var value = match.Value;

                // See what the type of this tag is and mark it, 
                // as a lot of things care about FrameworkElement level items.
                var type = _elementNameSearcher.Match(value);
                if (type.Success)
                {
                    ////var prefix = type.Groups["Prefix"]?.Value;
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
                    namespaces.Add(new XmlnsNamespace(XmlnsPrefix, XmlnsRequiredPath));
                }
                
                // Look to see if we're trying to use any namespaces we know about
                foreach (var ns in settings.KnownNamespaces)
                {
                    var usage = ns.Name + ":";
                    var included = XmlnsPrefix + ':' + ns.Name;

                    if (content.Contains(usage) && !value.Contains(included))
                    {
                        // If our first tag doesn't have the namespace, but we see it, then it's missing...
                        namespaces.Add(ns);
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
            }

            // Update our content holders.
            context.SuggestedContent = content;
            context.RenderedContent = content;
        }

        public static Type GetTypeFromName(string typename)
        {
            foreach (var assem in LoadedAssemblies)
            {
                var type = assem.GetExportedTypes().FirstOrDefault(t => t.Name == typename);
                if (type != null)
                {
                    // TODO: Do we need to worry about conflicting names across assemblies and pass in the namespace to this function?
                    return type;
                }
            }

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

        private static List<Assembly> LoadedAssemblies { get; set; }

        internal static async Task LoadAssembliesAsync()
        {
            LoadedAssemblies = new List<Assembly>();

            var files = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFilesAsync();
            if (files == null)
                return;

            foreach (var file in files.Where(file => file.FileType == ".dll" || file.FileType == ".exe"))
            {
                try
                {
                    LoadedAssemblies.Add(Assembly.Load(new AssemblyName(file.DisplayName)));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }
    }
}
