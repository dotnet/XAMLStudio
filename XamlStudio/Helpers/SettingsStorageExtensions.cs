// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Nito.AsyncEx;
using System;
using System.IO;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Storage.Streams;

namespace XamlStudio.Helpers;

// Use these extension methods to store and retrieve local and roaming app data
// For more info regarding storing and retrieving app data see documentation at
// https://docs.microsoft.com/windows/uwp/app-settings/store-and-retrieve-app-data
public static class SettingsStorageExtensions
{
    private static readonly AsyncLock _saveMutex = new();

    private const string FileExtension = ".json";

    public static bool IsRoamingStorageAvailable(this ApplicationData appData) => appData.RoamingStorageQuota == 0;

    public static async Task SaveAsync<T>(this StorageFolder folder, string name, T content)
    {
        using (await _saveMutex.LockAsync())
        {
            int saveAttempts = 3;

            while (saveAttempts > 0)
            {
                try
                {
                    // This ReplaceExisting flag seems to fail occassionally, guard by trying again.
                    var file = await folder.CreateFileAsync(GetFileName(name), CreationCollisionOption.ReplaceExisting);
                    var fileContent = await Json.StringifyAsync(content);

                    await FileIO.WriteTextAsync(file, fileContent);

                    break;
                }
                catch (Exception)
                {
                    await Task.Delay(50);
                }
                saveAttempts--;
            }
        }
    }

    public static async Task<T> ReadAsync<T>(this StorageFolder folder, string name)
    {
        using (await _saveMutex.LockAsync())
        {
            if (!File.Exists(Path.Combine(folder.Path, GetFileName(name))))
            {
                return default;
            }

            var file = await folder.GetFileAsync($"{name}.json");
            var fileContent = await FileIO.ReadTextAsync(file);

            return await Json.ToObjectAsync<T>(fileContent);
        }
    }

    public static async Task SaveAsync<T>(this ApplicationDataContainer settings, string key, T value) => settings.SaveString(key, await Json.StringifyAsync(value));

    public static void SaveString(this ApplicationDataContainer settings, string key, string value) => settings.Values[key] = value;

    public static async Task<T> ReadAsync<T>(this ApplicationDataContainer settings, string key)
    {
        if (settings.Values.TryGetValue(key, out object obj))
        {
            return await Json.ToObjectAsync<T>((string)obj);
        }

        return default;
    }

    public static async Task<object> ReadAsync(this ApplicationDataContainer settings, string key, Type type)
    {
        if (settings.Values.TryGetValue(key, out object obj))
        {
            return await Json.ToObjectAsync((string)obj, type);
        }

        return null;
    }

    public static async Task<StorageFile> SaveFileAsync(this StorageFolder folder, byte[] content, string fileName, CreationCollisionOption options = CreationCollisionOption.ReplaceExisting)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        if (string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentException("ExceptionSettingsStorageExtensionsFileNameIsNullOrEmpty".GetLocalized(), nameof(fileName));
        }

        using (await _saveMutex.LockAsync())
        {
            var storageFile = await folder.CreateFileAsync(fileName, options);
            await FileIO.WriteBytesAsync(storageFile, content);
            return storageFile;
        }
    }

    public static async Task<byte[]> ReadFileAsync(this StorageFolder folder, string fileName)
    {
        var item = await folder.TryGetItemAsync(fileName).AsTask().ConfigureAwait(false);

        if ((item != null) && item.IsOfType(StorageItemTypes.File))
        {
            var storageFile = await folder.GetFileAsync(fileName);
            byte[] content = await storageFile.ReadBytesAsync();
            return content;
        }

        return null;
    }

    public static async Task<byte[]> ReadBytesAsync(this StorageFile file)
    {
        if (file != null)
        {
            using (await _saveMutex.LockAsync())
            {
                using IRandomAccessStream stream = await file.OpenReadAsync();
                using var reader = new DataReader(stream.GetInputStreamAt(0));
                await reader.LoadAsync((uint)stream.Size);
                var bytes = new byte[stream.Size];
                reader.ReadBytes(bytes);
                return bytes;
            }
        }

        return null;
    }

    private static string GetFileName(string name) => string.Concat(name, FileExtension);
}
