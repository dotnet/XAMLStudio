// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Language.Xml;
using Monaco;
using Monaco.Editor;
using Monaco.Languages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using XamlStudio.Toolkit.Extensions;
using XamlStudio.Toolkit.Models;

namespace XamlStudio.Toolkit.Services;

/// <summary>
/// Monaco CompletionItemProvider for XAML.
/// </summary>
public class XamlLanguageProvider : CompletionItemProvider
{
    private const string ElementTrigger = "<";
    private const string NamespaceTrigger = ":";
    private const string AttributeValueTrigger = "\"";
    private const string CloseElementTrigger = "/";

    public List<XmlnsNamespace> KnownNamespaces { get; set; }

    public string[] TriggerCharacters => new string[] { ElementTrigger, CloseElementTrigger, NamespaceTrigger, AttributeValueTrigger };

    public IAsyncOperation<CompletionList> ProvideCompletionItemsAsync(IModel model, IPosition position, CompletionContext context)
    {
        return AsyncInfo.Run(async delegate (CancellationToken cancelationToken)
        {
            var items = new List<CompletionItem>();

            // get editor content before the pointer
            var text = await model.GetValueAsync();

            // TODO: Should try and coordinate where we need this model between render, document, and here.
            // LINK: Document.xaml.cs:UpdateBreadcrumbs
            var _xmlRoot = Parser.ParseText(text);

            var index = text.GetCharacterIndex((int)position.LineNumber, (int)position.Column);

            if (index == -1)
            {
                return null;
            }

            var raw_node = _xmlRoot.FindNode(index + 1);

            var parentTagName = raw_node?.ParentElement?.Name;

            // get list of assemblies
            var namespaces = KnownNamespaces.Concat(XamlAutocompleteService.Instance.GetNamespaces(await model.GetValueAsync()));

            if (context.TriggerCharacter == CloseElementTrigger)
            {
                if (index >= 1 && text[index - 1] == '<')
                {
#if UNO
                    items.Add(new CompletionItem(parentTagName, "", CompletionItemKind.Class));
#else
                    // Only show suggestion if we're starting a close tag.
                    items.Add(new CompletionItem(parentTagName, CompletionItemKind.Class));
#endif
                }
            }
            else if (context.TriggerCharacter == ElementTrigger)
            {
                // TODO: Add hint of containing element (ResourceDictionary, 
                XamlAutocompleteService.Instance.AddDefaultSuggestions(items, KnownNamespaces);
            }
            else if (context.TriggerCharacter == NamespaceTrigger)
            {
                // TODO: Don't show these if in first tag... show suggestions from known namespaces list... (not accessible here, in App :()
                if (!string.IsNullOrWhiteSpace(parentTagName))
                {
                    XamlAutocompleteService.Instance.AddNamespaceSuggestions(items, parentTagName.Trim(':'), KnownNamespaces);
                }
            }
            else if (context.TriggerCharacter == AttributeValueTrigger)
            {
                if (!string.IsNullOrWhiteSpace(parentTagName))
                {
                    var attribute = raw_node.FirstAncestorOrSelf<XmlAttributeSyntax>();

                    XamlAutocompleteService.Instance.AddValueSuggestions(items, parentTagName, attribute?.Name);
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(parentTagName))
                {
                    var attribute = raw_node.FirstAncestorOrSelf<XmlAttributeSyntax>();

                    // TODO: Filter out already used properties...
                    XamlAutocompleteService.Instance.AddPropertySuggestions(items, parentTagName);
                }
            }

            return new CompletionList()
            {
#if UNO
                Suggestions = items.ToArray()
#else
                Items = items
#endif
            };
        });
    }

    public IAsyncOperation<CompletionItem> ResolveCompletionItemAsync(CompletionItem item)
    {
        throw new NotImplementedException();
    }

#if UNO
    public async Task<CompletionList> ProvideCompletionItemsAsync(IModel model, Position position, CompletionContext context)
    {
        // Forward to the WinRT-style implementation and unwrap the result
        var op = ProvideCompletionItemsAsync((IModel)model, (IPosition)position, context);
        return await op.AsTask().ConfigureAwait(false);
    }

    public async Task<CompletionItem> ResolveCompletionItemAsync(IModel model, CompletionItem item)
    {
        throw new NotImplementedException();
    }
#endif
}
