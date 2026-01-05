// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.ComponentModel;

namespace XamlStudio.ViewModels;

// TODO WTS: This class exists purely as part of the example of how to launch a specific page in response to a protocol launch and pass it a value. It is expected that you will delete this class once you have changed the handling of a protocol launch to meet your needs and redirected to another of your pages.
public partial class UriSchemeExampleViewModel : ObservableObject
{
    // This property is just for displaying the passed in value
    [ObservableProperty]
    public partial string Secret { get; set; }
}
