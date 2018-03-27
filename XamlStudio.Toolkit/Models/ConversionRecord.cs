using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XamlStudio.Toolkit.Models
{
    /// <summary>
    /// Record of a Binding/Conversion Occuring
    /// </summary>
    public class ConversionRecord
    {
        public DateTime TimeStamp { get; } = DateTime.Now;

        public object Value { get; private set; }

        public object Result { get; private set; }

        public bool HasResult { get; private set; }

        public bool IsSuccessful { get; private set; }

        public Exception ExceptionObject { get; private set; }

        /// <summary>
        /// If there was no converter, then it's just a value that was passed thru.
        /// </summary>
        /// <param name="value"></param>
        public ConversionRecord(object value)
        {
            this.Value = value;
            this.IsSuccessful = true;
        }

        /// <summary>
        /// Value was successfully converted to the specified Result.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="result"></param>
        public ConversionRecord(object value, object result)
        {
            this.Value = value;
            this.Result = result;
            this.HasResult = true;
            this.IsSuccessful = true;
        }

        /// <summary>
        /// There was an error converting the given value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="error"></param>
        public ConversionRecord(object value, Exception error)
        {
            this.Value = value;
            this.ExceptionObject = error;
        }
    }
}
