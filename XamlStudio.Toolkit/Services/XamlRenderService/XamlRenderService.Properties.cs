// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace XamlStudio.Toolkit.Services
{
    public partial class XamlRenderService
    {
        /// <summary>
        /// Prefix used for xmlns Namespaces.
        /// </summary>
        public const string XmlnsPrefix = "xmlns";

        /// <summary>
        /// Path of required XAML xmlns Namespace for parsing.
        /// </summary>
        public const string XmlnsRequiredPath = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

        public const string XmlnsPathX = "http://schemas.microsoft.com/winfx/2006/xaml";

        public const string XmlnsPrefixXstc = "xstc";

        public const string XmlnsPathXstc = "using:XamlStudio.Toolkit.Converters";

        /// <summary>
        /// Unique Id for this Service.
        /// </summary>
        public int Id { get; } = IdGenerator.Next();
    }
}
