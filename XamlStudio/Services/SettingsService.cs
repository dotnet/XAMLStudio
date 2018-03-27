using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using XamlStudio.Helpers;

namespace XamlStudio.Services
{
    public partial class SettingsService : Observable
    {
        // http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly SettingsService _instance = new SettingsService();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static SettingsService() { }
        private SettingsService() { }

        public static SettingsService Instance
        {
            get
            {
                return _instance;
            }
        }

        // Internal Cache
        private static Dictionary<string, object> _settings = new Dictionary<string, object>();

        protected async Task<T> Get<T>([CallerMemberName]string propertyName = null)
        {
            if (propertyName == null)
            {
                return default(T);
            }

            // If we don't have our setting in our cache, then fetch it from storage.
            if (!_settings.ContainsKey(propertyName))
            {
                var value = await ApplicationData.Current.LocalSettings.ReadAsync<T>(propertyName);
                // If we don't have a value yet, see if we've defined a default.
                if (value == null || value.Equals(default(T)) ||
                   (value is string && String.IsNullOrEmpty(value as string)))
                {
                    var attr = this.GetType().GetProperty(propertyName).GetCustomAttribute(typeof(DefaultValueAttribute)) as DefaultValueAttribute;
                    if (attr != null)
                    {
                        value = (T)attr.Value;
                    }
                }

                // Cache our property value.
                _settings[propertyName] = value;
            }

            return (T)_settings[propertyName];
        }

        protected async void Set<T>(T value, [CallerMemberName]string propertyName = null)
        {
            // Check if anything's changed.
            if (propertyName == null || _settings.ContainsKey(propertyName) && Equals(_settings[propertyName], value))
            {
                return;
            }

            // Store value in our cache.
            _settings[propertyName] = value;
            // Save it out to stroage.
            await ApplicationData.Current.LocalSettings.SaveAsync<T>(propertyName, value);

            // Notify others of change.
            OnPropertyChanged(propertyName);
        }
    }

    /// <summary>
    /// Provides a default value for a setting property.
    /// </summary>
    public class DefaultValueAttribute : Attribute
    {
        public object Value { get; set; }

        public DefaultValueAttribute(object value)
        {
            Value = value;
        }
    }
}
