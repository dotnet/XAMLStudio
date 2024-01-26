using System;
using Windows.UI.Xaml.Markup;

namespace XamlStudio.Helpers;

// Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/7633
[MarkupExtensionReturnType(ReturnType = typeof(object))]
public class EnumValueExtension : MarkupExtension
{
    public Type Type { get; set; }

    public string Member { get; set; }

    protected override object ProvideValue()
    {
        return Enum.Parse(Type, Member);
    }
}
