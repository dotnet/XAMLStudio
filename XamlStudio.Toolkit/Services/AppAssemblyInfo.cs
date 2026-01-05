// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using XamlStudio.Toolkit.Helpers;

namespace XamlStudio.Toolkit.Services;

public class AppAssemblyInfo
{
    public static AppAssemblyInfo Instance => Singleton<AppAssemblyInfo>.Instance;

    private readonly AsyncLock _mutex = new AsyncLock();
    public bool IsLoaded { get; private set; }

    public IReadOnlyList<Assembly> LoadedAssemblies { get; private set; }
    public IReadOnlyList<Type> KnownTypes { get; private set; }
    public IReadOnlyDictionary<string, ReadOnlyCollection<Type>> TypesByNamespace { get; private set; }

    public async Task InitializeAsync(Assembly[] extraAssemblies = null)
    {
        using (await _mutex.LockAsync())
        {
            if (!IsLoaded)
            {
                await LoadAssembliesAsync(extraAssemblies);
                FindTypes();
                MapTypes();

                IsLoaded = true;
            }
        }
    }

    /// <summary>
    /// Get all extra assemblies.
    /// </summary>
    /// <returns></returns>
    private async Task LoadAssembliesAsync(Assembly[] extraAssemblies = null)
    {
        // Add any provided assemblies
        // Current workaround for Microsoft.UI.Xaml and this limitation:
        // https://social.msdn.microsoft.com/Forums/en-US/a78fdd8e-a108-4279-9e6b-6c87cd0a0f0f/assemblyload-of-winmd-file-possible
        var assemblies = new HashSet<Assembly>(extraAssemblies ?? Enumerable.Empty<Assembly>());

        // Add Windows Assembly
        assemblies.Add(typeof(FrameworkElement).GetTypeInfo().Assembly); // Windows.UI.Xaml

        // Add Other Assemblies (Debug Only)
        var files = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFilesAsync();
        if (files == null)
            return;

        foreach (var file in files.Where(file => file.FileType == ".dll" || file.FileType == ".exe")) // || file.FileType == ".winmd"))
        {
            try
            {
                assemblies.Add(Assembly.Load(new AssemblyName(file.DisplayName)));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        LoadedAssemblies = assemblies.ToList().AsReadOnly();
    }

    /// <summary>
    /// Get all known types from loaded assemblies.
    /// </summary>
    /// <returns></returns>
    private void FindTypes()
    {
        var types = new List<Type>();
        foreach (var assem in LoadedAssemblies)
        {
            types.AddRange(assem.GetExportedTypes());
            ////types.AddRange(assem.GetTypes());
        }

        KnownTypes = types.AsReadOnly();
    }

    private void MapTypes()
    {
        var mapping = new Dictionary<string, List<Type>>();
        foreach (var type in KnownTypes)
        {
            if (type.Namespace == null)
            {
                continue;
            }
            else if (!mapping.ContainsKey(type.Namespace))
            {
                mapping[type.Namespace] = new List<Type>();
            }

            mapping[type.Namespace].Add(type);
        }

        // Make Read Only
        TypesByNamespace = new ReadOnlyDictionary<string, ReadOnlyCollection<Type>>(mapping.ToDictionary(pair => pair.Key, pair => pair.Value.AsReadOnly()));
    }
}
