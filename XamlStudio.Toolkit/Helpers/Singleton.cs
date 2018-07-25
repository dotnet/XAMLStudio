using System;
using System.Collections.Concurrent;

namespace XamlStudio.Toolkit.Helpers
{
    /// <summary>
    /// Provides a thread-safe Singleton Pattern.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Singleton<T>
        where T : new()
    {
        private static ConcurrentDictionary<Type, T> _instances = new ConcurrentDictionary<Type, T>();

        public static T Instance
        {
            get
            {
                return _instances.GetOrAdd(typeof(T), (t) => new T());
            }
        }
    }
}
