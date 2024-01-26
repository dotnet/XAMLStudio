
using XamlStudio.ViewModels;

namespace XamlStudio.Models;

public record OpenActivityChangedMessaged(string NewActivity);

public record NavigateToLineMessage(uint Line); // TODO: Need document?

public record InsertTextMessage(string Text);

public record RenderXamlMessage();

public record ActiveDocumentViewModelChangedMessage(DocumentViewModel PreviousDocVM, DocumentViewModel NewDocVM);
