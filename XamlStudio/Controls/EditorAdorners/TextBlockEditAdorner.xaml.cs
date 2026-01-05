// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Controls.Future;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using XamlStudio.Models;

namespace XamlStudio.Controls;

public sealed partial class TextBlockEditAdorner : UserControl
{
    public TextBlock AttachedElement { get; }

    private string _originalText;

    public TextBlockEditAdorner(FrameworkElement attachedElement)
    {
        AttachedElement = attachedElement as TextBlock;
        _originalText = AttachedElement.Text;

        InitializeComponent();
    }

    private void AcceptButton_Click(object sender, RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.Send<AddToXamlMessage>(new(AttachedElement, "Text", AttachedElement.Text.Replace("\"", "&quot;")));

        AdornerLayer.SetXaml(AttachedElement, null);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        AdornerLayer.SetXaml(AttachedElement, null);

        // TODO: We may want to do our trick to check if there's a binding here?
        AttachedElement.Text = _originalText;
    }
}
