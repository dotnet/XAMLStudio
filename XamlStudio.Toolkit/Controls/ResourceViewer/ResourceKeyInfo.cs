using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XamlStudio.Toolkit.Controls
{
    /// <summary>
    /// Class to represent MetaData about resources in a <see cref="ResourceDictionary"/>.
    /// </summary>
    public class ResourceKeyInfo
    {
        public string Key { get; set; }

        public Type ResourceType { get; set; }

        public string ResourceTypeName => ResourceType?.Name;

        public object Value { get; set; }

        /// <summary>
        /// Gets the control used to display the value.  Defaults to the Value.
        /// </summary>
        public object DisplayedValueControl { get; set; }

        public ResourceKeyInfo(string key, object value)
        {
            Key = key;
            Value = value;
            DisplayedValueControl = value;
            ResourceType = value.GetType();
        }
    }
}
