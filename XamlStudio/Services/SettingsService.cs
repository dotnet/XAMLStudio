// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using XamlStudio.Helpers;
using XamlStudio.Toolkit.Helpers;

namespace XamlStudio.Services;

public partial class SettingsService : ObservableObject
{
    public static SettingsService Instance => Singleton<SettingsService>.Instance;

    private readonly AsyncLock _initializeMutex = new();
    private bool _isInitialized = false;

    public SettingsService()
    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        InitializeAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }

    public async Task InitializeAsync()
    {
        using (await _initializeMutex.LockAsync())
        {
            if (!_isInitialized)
            {
                foreach (var prop in GetType().GetProperties())
                {
                    if (!_settings.ContainsKey(prop.Name))
                    {
                        var value = await ApplicationData.Current.LocalSettings.ReadAsync(prop.Name, prop.PropertyType);
                        // If we don't have a value yet, see if we've defined a default.
                        if (value == null || value.Equals(prop.PropertyType.GetDefault()) ||
                           (value is string && string.IsNullOrEmpty(value as string)))
                        {
                            // If we don't have a value yet, see if we've defined a default.
                            var attr = prop.GetCustomAttribute(typeof(DefaultValueAttribute)) as DefaultValueAttribute;
                            if (attr != null)
                            {
                                if (attr.LoadFromUri)
                                {
                                    // Load from our application resources.
                                    var uri = new Uri(attr.Value.ToString());

                                    if (uri != null)
                                    {
                                        var file = await StorageFile.GetFileFromApplicationUriAsync(uri);

                                        var text = await FileIO.ReadTextAsync(file);

                                        _settings[prop.Name] = JsonConvert.DeserializeObject(text, prop.PropertyType);
                                    }
                                }
                                else
                                {
                                    // Use the provided value.
                                    _settings[prop.Name] = attr.Value;
                                }
                            }
                        }
                        else
                        {
                            // Cache loaded value.
                            _settings[prop.Name] = value;
                        }
                    }
                }

                _isInitialized = true;
            }
        }
    }

    // Internal Cache
    private static Dictionary<string, object> _settings = [];

    public T Get<T>([CallerMemberName] string propertyName = null)
    {
        if (propertyName == null || !_settings.ContainsKey(propertyName))
        {
            return default;
        }

        return (T)_settings[propertyName];
    }

    public void Set<T>(T value, [CallerMemberName] string propertyName = null)
    {
        // Check if anything's changed.
        if (propertyName == null || _settings.ContainsKey(propertyName) && Equals(_settings[propertyName], value))
        {
            return;
        }

        // Store value in our cache.
        _settings[propertyName] = value;
        // Save it out to storage.
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        ApplicationData.Current.LocalSettings.SaveAsync<T>(propertyName, value);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

        // Notify others of change.
        OnPropertyChanged(propertyName);
    }

    public async Task SaveAsync(string propertyName = null)
    {
        if (propertyName != null && _settings.ContainsKey(propertyName))
        {
            await ApplicationData.Current.LocalSettings.SaveAsync(propertyName, _settings[propertyName]);
        }
    }

    public event EventHandler<StorageFile> RecentFilesChanged;

    public void RememberFileOrFolder(IStorageItem item)
    {
        StorageApplicationPermissions.MostRecentlyUsedList.Add(item);

        // TODO: Keep track of Recent Folders on Welcome as well.
        if (item is StorageFile file)
        {
            RecentFilesChanged?.Invoke(this, file);
        }
    }

    public async Task<IEnumerable<StorageFile>> GetRecentFilesAsync(int num)
    {
        var files = new List<StorageFile>();
        foreach (AccessListEntry entry in StorageApplicationPermissions.MostRecentlyUsedList.Entries)
        {
            string mruToken = entry.Token;
            string mruMetadata = entry.Metadata;
            try
            {
                IStorageItem item = await StorageApplicationPermissions.MostRecentlyUsedList.GetItemAsync(mruToken);

                if (item.IsOfType(StorageItemTypes.File))
                {
                    files.Add(item as StorageFile);
                }
            }
            catch (Exception)
            {
                // GetItemAsync threw exception?  Skip...
            }

            if (files.Count == num)
            {
                break;
            }
        }

        return files;
    }
}

/// <summary>
/// Provides a default value for a setting property.
/// </summary>
public class DefaultValueAttribute : Attribute
{
    public object Value { get; set; }

    public bool LoadFromUri { get; set; }

    public DefaultValueAttribute(object value)
    {
        Value = value;
    }
}
