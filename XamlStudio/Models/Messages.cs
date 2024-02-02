
using CommunityToolkit.Mvvm.Messaging.Messages;
using XamlStudio.ViewModels;

namespace XamlStudio.Models;

public record OpenActivityChangedMessage(string NewActivity);

public record NavigateToLineMessage(uint Line); // TODO: Need document?

public record InsertTextMessage(string Text);

public record RenderXamlMessage();

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
