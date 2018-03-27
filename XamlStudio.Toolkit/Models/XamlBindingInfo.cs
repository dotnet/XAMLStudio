using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using XamlStudio.Toolkit.Services;

namespace XamlStudio.Toolkit.Models
{
    /// <summary>
    /// Record to store information about a Binding and its history.
    /// </summary>
    public class XamlBindingInfo
    {
        public enum XamlBindingState
        {
            NotBound,
            Successful,
            ConversionError,
        }

        public delegate void BindingUpdatedHandler(XamlBindingInfo sender, object newValue);

        public event BindingUpdatedHandler BindingUpdated;

        public string OriginalBindingString { get; set; }

        public IValueConverter Converter { get; set; }

        public string ConverterKey { get; set; }

        public object ConverterParameter { get; set; }

        public uint Line { get; private set; }
        public uint Column { get; private set; }
        public int Length { get { return this.OriginalBindingString.Length; } }

        // TODO: Create a Binding Log window in Xaml Studio which shows all binding events and lets you group/sort/search by time, value, status (etc...)
        public List<ConversionRecord> BindingHistory { get; set; } = new List<ConversionRecord>();

        // TODO: Make Observable?

        #region Helper Properties
        public int Id { get; } = IdGenerator.Next();

        public XamlRenderService Service { get; internal set; }

        public DateTime FirstSetTime { get { return this.BindingHistory.First().TimeStamp; } }

        public bool HasBinded { get { return this.BindingHistory.Count > 0; } }

        public object LastConvertedValue { get { return this.BindingHistory.LastOrDefault()?.Value; } }

        public object LastConvertedResult { get { return this.BindingHistory.LastOrDefault()?.Result; } }

        public object LastConvertedResultOrValue { get { return (bool)this.BindingHistory.LastOrDefault()?.HasResult ? this.BindingHistory.LastOrDefault()?.Result : this.BindingHistory.LastOrDefault()?.Value; } }

        public string LastExceptionMessage { get { return this.BindingHistory.LastOrDefault()?.ExceptionObject.Message;  } }

        public XamlBindingState LastKnownBindingState
        {
            get
            {
                if (HasBinded)
                {
                    // If we have bound, then grab the last entry and see if it was a success or not.
                    return this.BindingHistory.Last().IsSuccessful ? XamlBindingState.Successful : XamlBindingState.ConversionError;
                }
                else
                {
                    // If we haven't seen a binding yet, then we're not bound.
                    return XamlBindingState.NotBound;
                }
            }
        }

        public long BindingCount { get { return this.BindingHistory.Count; } }
        #endregion

        public XamlBindingInfo(uint line, uint column, string binding)
        {
            this.Line = line;
            this.Column = column;
            this.OriginalBindingString = binding;
        }

        public object NewValue(object value)
        {
            this.BindingHistory.Add(new ConversionRecord(value));

            BindingUpdated?.Invoke(this, value);

            return value;
        }

        public object NewConversion(object value, object result)
        {
            this.BindingHistory.Add(new ConversionRecord(value, result));

            BindingUpdated?.Invoke(this, result);

            return result;
        }

        public object NewException(object value, Exception error)
        {
            this.BindingHistory.Add(new ConversionRecord(value, error));

            BindingUpdated?.Invoke(this, error);

            // Pass null in so binding can use default null fallback if it exists on binding fail?
            return null;
        }
    }
}
