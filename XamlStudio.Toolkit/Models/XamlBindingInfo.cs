using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.UI.Xaml.Data;
using XamlStudio.Toolkit.Services;

namespace XamlStudio.Toolkit.Models
{
    /// <summary>
    /// Record to store information about a Binding and its history.
    /// </summary>
    public class XamlBindingInfo : INotifyPropertyChanged
    {
        public enum XamlBindingState
        {
            NotBound,
            Successful,
            ConversionError,
        }

        public delegate void BindingUpdatedHandler(XamlBindingInfo sender, ConversionRecord record, object newValue);

        public event BindingUpdatedHandler BindingUpdated;

        public string OriginalBindingString { get; set; }

        public IValueConverter Converter { get; set; }

        public string ConverterKey { get; set; }

        public object ConverterParameter { get; set; }

        public uint Line { get; private set; }
        public uint Column { get; private set; }
        public int Length { get { return this.OriginalBindingString.Length; } }

        public XAttribute PropertyAttribute { get; set; }
        public string PropertyName { get; set; }
        public string ElementTypeName { get; set; }
        public string ElementName { get; set; }

        // TODO: Create a Binding Log window in Xaml Studio which shows all binding events and lets you group/sort/search by time, value, status (etc...)
        public ObservableCollection<ConversionRecord> BindingHistory { get; set; } = new ObservableCollection<ConversionRecord>();

        // TODO: Make Observable?

        #region Helper Properties
        public int Id { get; } = IdGenerator.Next();

        public XamlRenderService Service { get; internal set; }

        public DateTime FirstSetTime { get { return this.BindingHistory.First().TimeStamp; } }

        public bool HasBinded { get { return this.BindingHistory.Count > 0; } }

        public object LastConvertedValue { get { return this.BindingHistory.LastOrDefault()?.Value; } }

        public object LastConvertedResult { get { return this.BindingHistory.LastOrDefault()?.Result; } }

        public object LastConvertedResultOrValue
        {
            get
            {
                if (!HasBinded)
                {
                    return null;
                }

                return (bool)this.BindingHistory.LastOrDefault()?.HasResult ? this.BindingHistory.LastOrDefault()?.Result : this.BindingHistory.LastOrDefault()?.Value;
            }
        }

        public string LastConvertedResultOrValueString
        {
            get { return LastConvertedResultOrValue?.ToString() ?? string.Empty; }
        }

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
            var record = new ConversionRecord(this, value);

            this.BindingHistory.Add(record);

            BindingUpdated?.Invoke(this, record, value);

            OnPropertyChanged(nameof(HasBinded));
            OnPropertyChanged(nameof(FirstSetTime));
            OnPropertyChanged(nameof(LastConvertedValue));
            OnPropertyChanged(nameof(LastConvertedResultOrValue));
            OnPropertyChanged(nameof(LastConvertedResultOrValueString));
            OnPropertyChanged(nameof(LastKnownBindingState));

            return value;
        }

        public object NewConversion(object value, object result)
        {
            var record = new ConversionRecord(this, value, result);

            this.BindingHistory.Add(record);

            BindingUpdated?.Invoke(this, record, result);

            OnPropertyChanged(nameof(HasBinded));
            OnPropertyChanged(nameof(FirstSetTime));
            OnPropertyChanged(nameof(LastConvertedValue));
            OnPropertyChanged(nameof(LastConvertedResult));
            OnPropertyChanged(nameof(LastConvertedResultOrValue));
            OnPropertyChanged(nameof(LastConvertedResultOrValueString));
            OnPropertyChanged(nameof(LastKnownBindingState));

            return result;
        }

        public object NewException(object value, Exception error)
        {
            var record = new ConversionRecord(this, value, error);

            this.BindingHistory.Add(record);

            BindingUpdated?.Invoke(this, record, error);

            OnPropertyChanged(nameof(HasBinded));
            OnPropertyChanged(nameof(FirstSetTime));
            OnPropertyChanged(nameof(LastExceptionMessage));
            OnPropertyChanged(nameof(LastKnownBindingState));

            // Pass null in so binding can use default null fallback if it exists on binding fail?
            return null;
        }

        public override string ToString()
        {
            var start = string.IsNullOrWhiteSpace(ElementName) ? ElementTypeName : ElementName + "[" + ElementTypeName + "]";

            return start + "." + PropertyName;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
