using Newtonsoft.Json;
using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace XamlStudio.Toolkit.Services
{
    /// <summary>
    /// Class to assist in parsing a Xaml string and returning an UIElement.
    /// 
    /// Wrapper around XamlReader.Load* with extra pre/post processing to support more features like loading images from an external source.
    /// 
    /// References:
    ///     https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.markup.xamlreader
    ///     https://docs.microsoft.com/en-us/windows/uwp/xaml-platform/xaml-namespaces-and-namespace-mapping
    ///     https://docs.microsoft.com/en-us/windows/uwp/data-binding/data-binding-in-depth
    ///     https://blogs.msdn.microsoft.com/mcsuksoldev/2010/08/27/designdata-mvvm-support-in-blend-vs2010-and-wpfsilverlight/
    /// </summary>
    public partial class XamlRenderService
    {
        /// <summary>
        /// Given a StorageFolder and path loads a Data file to load as an object.
        /// </summary>
        /// <param name="source">Starting StorageFolder.</param>
        /// <param name="path">string path.</param>
        /// <returns></returns>
        private static async Task<object> LoadDataSource(StorageFolder source, string path)
        {
            var file = await GetFileFromPath(source, path);

            dynamic data = null;

            if (file != null)
            {
                // TODO: Separate the inner part of this logic from the file reading part so it can be used by json text.
                var content = await FileIO.ReadTextAsync(file);

                // TODO: Do I just add errors in deserializing to the Errors bucket as well?
                // Do I have a separate Error Bucket?
                switch (file.FileType.ToLower())
                {
                    case ".json":
                        try
                        {
                            data = JsonConvert.DeserializeObject<ExpandoObject>(content);
                        }
                        catch (Exception)
                        {
                            // TODO: Would need XamlRenderResultContext here to add errors...
                        }
                        break;
                    case ".xml":
                        // TODO
                        break;
                }
            }

            return data;
        }

        //// TODO: Move to general helper location somewhere?
        /// <summary>
        /// Given a path, e.g. "Images\Owl.jpg", navigates the structure from StorageFolder/Files.
        /// </summary>
        /// <param name="root">Starting StorageFolder.</param>
        /// <param name="path">string path.</param>
        /// <returns></returns>
        public static async Task<StorageFile> GetFileFromPath(StorageFolder root, string path)
        {
            // Flip path around and try again.
            if (path.Contains("\\"))
            {
                return await GetFileFromPath(root, path.Replace("\\", "/"));
            }

            // Trim start if we have an absolute marker, as we'll still be going from the same place.
            // TODO: Do we need to do this outside here?
            if (path.StartsWith("/"))
            {
                path = path.Substring(1);
            }

            // If we have a subfolder path, we need to navigate into folder first.
            if (path.Contains("/"))
            {
                var folder = path.Substring(0, path.IndexOf("/"));
                if (await root.TryGetItemAsync(folder) is StorageFolder newroot)
                {
                    // Call again with subfolder and truncated path.
                    return await GetFileFromPath(newroot, path.Substring(folder.Length + 1));
                }

                // Couldn't find sub-folder.
                return null;
            }

            // Return file if it exists.
            return await root.TryGetItemAsync(path) as StorageFile;
        }

        /// <summary>
        /// Visits each element and child of the given element and passes them to the given action.
        /// </summary>
        /// <param name="element">Root element to start from.</param>
        /// <param name="func">Function to execute.</param>
        private static void VisitUIElements(UIElement element, Action<UIElement> func)
        {
            // Visit element
            func(element);

            if (element is Panel)
            {
                foreach (var child in (element as Panel).Children)
                {
                    // Visit each child in the panel
                    VisitUIElements(child, func);
                }
            }
            else if (element != null)
            {
                // Use ContentProperty Attribute to figure out which property we should look for as the 'Content' for this control
                var contentpropname = ContentPropertySearch(element.GetType());
                //var attr = element.GetType().GetTypeInfo().GetCustomAttribute(typeof(ContentPropertyAttribute), true) as ContentPropertyAttribute;
                if (contentpropname != null)
                {
                    if (element.GetType().GetProperty(contentpropname).GetValue(element) is UIElement child)
                    {
                        VisitUIElements(child, func);
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves the Content Property's Name for the given type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string ContentPropertySearch(Type type)
        {
            if (type == null)
            {
                return null;
            }

            // Using GetCustomAttribute directly isn't working for some reason, so we'll dig in ourselves
            //var attr = type.GetTypeInfo().GetCustomAttribute(typeof(ContentPropertyAttribute), true);
            var attr = type.GetTypeInfo().CustomAttributes.FirstOrDefault((element) => element.AttributeType == typeof(ContentPropertyAttribute));
            if (attr != null)
            {
                //return attr as ContentPropertyAttribute;
                return attr.NamedArguments.First().TypedValue.Value as string;
            }

            return ContentPropertySearch(type.GetTypeInfo().BaseType);
        }
    }
}
