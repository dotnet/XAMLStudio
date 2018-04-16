using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using XamlStudio.Toolkit.Services;

namespace XamlStudio.Toolkit.Converters
{
    [Bindable]
    public sealed class XamlBindingWrapperConverter : IValueConverter
    {
        // TODO: Create Wrapper which has access to this class (for location contexts)
        // TODO: Wrapper should have BindingState (NotBound, Error, Success) & BindingError (dependency properties) and be exposed with location from here, so app can highlight binding locations and change color when triggered
        // TODO: Binding Wrapper should check type of conversion and throw error (and try/catch actual converter too)
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var bindingId = parameter.ToString();

            // Retrieve the id of the binding object we need to fetch.
            if (int.TryParse(bindingId, out int id))
            {
                // Actually this shouldn't happen, just coding in backwards order...
                if (!XamlBindingWrapperManager.Instance.ContainsKey(id))
                {
                    Debugger.Break();
                    throw new ArgumentException("Unexpected Paramater: BindingInfo Id Specified Isn't Registered with XamlBindingWrapperManager.");
                    //XamlBindingWrapperManager.Instance.Add(id, new Models.XamlBindingInfo(bindingInfo.Substring(start)));
                }

                var binding = XamlBindingWrapperManager.Instance[id];

                // TODO: Intercept uris in value/result for storage redirection.  As in L234 of XamlRenderService.  Uri.TryCreate

                if (binding.Converter != null)
                {
                    // If we have a converter, see if we can successfully convert the value, otherwise, catch the exception.
                    try
                    {
                        var result = binding.Converter.Convert(value, targetType, binding.ConverterParameter, language);

                        return binding.NewConversion(value, result);
                    } catch (Exception e)
                    {
                        return binding.NewException(value, e);
                    }
                }
                else
                {
                    // If there was no converter, we'll simply record it and pass the value thru.
                    return binding.NewValue(value);
                }
            }
            else
            {
                Debugger.Break();
                // This shouldn't happen as we should have converted all bindings to use our converter and include a generated id.
                throw new ArgumentException("Unexpected Parameter: Missing Integer Id Before Original Binding Expression.");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
