// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.Language.Xml;
using Windows.UI.Xaml;
using XamlStudio.Toolkit.Models;
using XamlStudio.ViewModels;

namespace XamlStudio.Models;

public record OpenActivityChangedMessage(string NewActivity);

public record NavigateToLineMessage(uint Line); // TODO: Need document?

/// <summary>
/// Inserts the specified text into the current document location (or replaces an active selection).
/// </summary>
/// <param name="Text"></param>
public record InsertTextMessage(string Text);

public record AddToXamlMessage(FrameworkElement Element, string Property, string Value);

/// <summary>
/// Requests the rendering process the current document Xml to via XamlRenderService in
/// LINK:Document.xaml.cs:Receive(RenderXamlMessage
/// </summary>
public record RenderXamlMessage();

/// <summary>
/// Sent after <see cref="RenderXamlMessage"/> has completed and has a context (successful or unsuccessful).
/// </summary>
/// <param name="Context"><see cref="XamlRenderResultContext"/></param>
public record XamlCompiledMessage(XamlRenderResultContext Context);

/// <summary>
/// Sent after a successful render cycle started with <see cref="RenderXamlMessage"/> after <see cref="XamlCompiledMessage"/>.
/// </summary>
/// <param name="Context"><see cref="XamlRenderResultContext"/></param>
public record XamlRenderedMessage(XamlRenderResultContext Context);

public record ActiveDocumentViewModelChangedMessage(DocumentViewModel PreviousDocVM, DocumentViewModel NewDocVM);

public record DataSourceSetInFileMessage(string? FileName);

/// <summary>
/// Sent when the caret is moved within the document and provides the XmlDocument Element which corresponds to that location.
/// </summary>
/// <param name="Element"><see cref="IXmlElementSyntax"/></param>
public record EditorSelectedElementMessage(IXmlElementSyntax Element);

/// <summary>
/// Sent when an editor selects an explicit Visual Element component and needs to update other related tools interacting with a selected visual element.
/// </summary>
/// <param name="Element"></param>
public record SelectedVisualElementMessage(DependencyObject Element);

/// <summary>
/// Replies with true if the shortcut was handled.
/// </summary>
public class KeyDownMessage : RequestMessage<bool>
{
    public bool Ctrl { get; init; }
    public bool Shift { get; init; }
    public int KeyCode { get; init; }

    public KeyDownMessage(bool ctrl, bool shift, int keyCode)
    {
        Ctrl = ctrl;
        Shift = shift;
        KeyCode = keyCode;
    }
}
