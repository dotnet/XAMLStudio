// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using XamlStudio.Toolkit.Helpers;
using XamlStudio.Toolkit.Models;

namespace XamlStudio.Toolkit.Services;

// TODO: Feel like this could have better data typing to serve it's purpose... should think about this more.
/// <summary>
/// Manages connections between <see cref="XamlRenderService"/> and <see cref="XamlBindingWrapperConverter"/>.
/// </summary>
public class XamlBindingWrapperManager : Dictionary<int, XamlBindingInfo>
{
    private readonly Dictionary<int, XamlRenderService> _renderers = [];
    private readonly Dictionary<int, List<int>> _tracker = [];

    public static XamlBindingWrapperManager Instance => Singleton<XamlBindingWrapperManager>.Instance;

    public void Register(int id, XamlRenderService service)
    {
        this._renderers.Add(id, service);
        this._tracker[id] = [];
    }

    public void AddNewBinding(int serviceId, XamlBindingInfo binding)
    {
        binding.Service = this._renderers[serviceId];
        this.Add(binding.Id, binding);
        this._tracker[serviceId].Add(binding.Id);
    }

    public IEnumerable<XamlBindingInfo> GetBindings(int serviceId)
    {
        foreach (var binding in this._tracker[serviceId])
        {
            yield return this[binding];
        }
    }

    /// <summary>
    /// Removes all previous bindings related to a XamlRenderService reference.
    /// </summary>
    /// <param name="serviceId"></param>
    public void Clear(int serviceId)
    {
        foreach (var binding in this._tracker[serviceId])
        {
            this.Remove(binding);
        }

        this._tracker[serviceId] = [];
    }
}
