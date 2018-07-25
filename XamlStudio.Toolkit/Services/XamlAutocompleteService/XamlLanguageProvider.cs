using Monaco;
using Monaco.Editor;
using Monaco.Languages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using XamlStudio.Toolkit.Models;

namespace XamlStudio.Toolkit.Services
{
    /// <summary>
    /// Monaco CompletionItemProvider for XAML.
    /// </summary>
    public class XamlLanguageProvider : CompletionItemProvider
    {
        private const string ElementTrigger = "<";
        private const string NamespaceTrigger = ":";
        private const string AttributeValueTrigger = "\"";

        public List<XmlnsNamespace> KnownNamespaces { get; set; }

        public string[] TriggerCharacters => new string[] { ElementTrigger, NamespaceTrigger };

        public IAsyncOperation<CompletionList> ProvideCompletionItemsAsync(IModel model, IPosition position, CompletionContext context)
        {
            return AsyncInfo.Run(async delegate (CancellationToken cancelationToken)
            {
                var items = new List<CompletionItem>();

                // get editor content before the pointer
                var textUntilPosition = await model.GetValueInRangeAsync(new Range(1, 1, position.LineNumber, position.Column));

                // get list of assemblies
                var namespaces = KnownNamespaces.Concat(XamlAutocompleteService.Instance.GetNamespaces(await model.GetValueAsync()));

                if (context.TriggerCharacter == NamespaceTrigger)
                {
                    // TODO: Don't show these if in first tag... show suggestions from known namespaces list... (not accessible here, in App :()
                    var lastOpenedTag = XamlLanguageHelpers.GetLastOpenedTag(textUntilPosition);//areaUntilPositionInfo.ClearedText);

                    if (lastOpenedTag.HasValue)
                    {
                        var prefix = lastOpenedTag.Value.TagName;

                        if (namespaces.FirstOrDefault(n => n.Name == prefix) is XmlnsNamespace ns && ns.Path != null && ns.Path.StartsWith("using:"))
                        {
                            var @namespace = ns.Path.Split(':')[1];

                            if (AppAssemblyInfo.Instance.TypesByNamespace.TryGetValue(@namespace, out var types))
                            {
                                foreach (var t in types) //.Where(t => t.IsAssignableFrom(typeof(DependencyObject))))
                                {
                                    items.Add(new CompletionItem(t.Name, CompletionItemKind.Class));
                                }
                            }
                        }
                    }
                }

                return new CompletionList()
                {
                    Items = items
                };
            });
        }

        public IAsyncOperation<CompletionItem> ResolveCompletionItemAsync(CompletionItem item)
        {
            throw new NotImplementedException();
        }
    }
}
