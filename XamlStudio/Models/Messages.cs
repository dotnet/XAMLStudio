
using CommunityToolkit.Mvvm.Messaging.Messages;
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

public record ActiveDocumentViewModelChangedMessage(DocumentViewModel PreviousDocVM, DocumentViewModel NewDocVM);

public record DataSourceSetInFileMessage(string? FileName);

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
