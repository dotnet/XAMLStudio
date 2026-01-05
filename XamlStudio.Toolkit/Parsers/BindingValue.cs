// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace XamlStudio.Toolkit.Parsers
{
    public class BindingValue
    {
        /// <summary>
        ///     Gets or sets the path to the binding source property.
        ///
        /// Returns:
        ///     The property path for the source of the binding.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        ///     Gets or sets a value that indicates the direction of the data flow in the binding.
        ///
        /// Returns:
        ///     One of the BindingMode values. The default is **OneWay**: the source updates
        ///     the target, but changes to the target value do not update the source.
        /// </summary>
        public string Mode { get; set; }

        /// <summary>
        ///     Gets or sets the name of the element to use as the binding source for the Binding.
        ///
        /// Returns:
        ///     The value of the Name property or x:Name attribute for the element you want to
        ///     use as the binding source. The default is an empty string.
        /// </summary>
        public string ElementName { get; set; }

        /// <summary>
        ///     Gets or sets a parameter that can be used in the Converter logic after escaped value parsing.
        ///
        /// Returns:
        ///     A parameter to be passed to the Converter. This can be used in the conversion
        ///     logic. The default is **null**.
        /// </summary>
        public string ConverterParameter { get; set; }

        /// <summary>
        ///     Gets or sets the original string value for the converter parameter, helpful for replacement.
        /// </summary>
        public string ConverterParameterRaw { get; set; }

        /// <summary>
        ///     Gets or sets a value that names the language to pass to any converter specified
        ///     by the Converter property.
        ///
        /// Returns:
        ///     A string that names a language. Interpretation of this value is ultimately up
        ///     to the converter logic.
        /// </summary>
        public string ConverterLanguage { get; set; }

        // TODO: Should this be 'ConverterKey' and keep the original string intact?
        /// <summary>
        ///     Gets or sets the converter object that is called by the binding engine to modify
        ///     the data as it is passed between the source and target, or vice versa.
        ///
        /// Returns:
        ///     The IValueConverter object that modifies the data.
        /// </summary>
        public string Converter { get; set; }

        /// <summary>
        ///     Gets or sets the data source for the binding.
        ///
        /// Returns:
        ///     The source object that contains the data for the binding.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        ///     Gets or sets the binding source by specifying its location relative to the position
        ///     of the binding target. This is most often used in bindings within XAML control
        ///     templates.
        ///
        /// Returns:
        ///     The relative location of the binding source to use. The default is **null**.
        /// </summary>
        public string RelativeSource { get; set; }

        /// <summary>
        ///     Gets or sets a value that determines the timing of binding source updates for
        ///     two-way bindings.
        ///
        /// Returns:
        ///     One of the UpdateSourceTrigger values. The default is **Default**, which evaluates
        ///     as a **PropertyChanged** update behavior.
        /// </summary>
        public string UpdateSourceTrigger { get; set; }

        /// <summary>
        ///     Gets or sets the value that is used in the target when the value of the source
        ///     is **null**.
        ///
        /// Returns:
        ///     The value that is used in the binding target when the value of the source is
        ///     **null**.
        /// </summary>
        public string TargetNullValue { get; set; }

        /// <summary>
        ///     Gets or sets the value to use when the binding is unable to return a value.
        ///
        /// Returns:
        ///     The value to use when the binding is unable to return a value.
        /// </summary>
        public string FallbackValue { get; set; }
    }
}
