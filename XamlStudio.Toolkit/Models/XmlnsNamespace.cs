// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Newtonsoft.Json;
using System.ComponentModel;

namespace XamlStudio.Toolkit.Models
{
    public class XmlnsNamespace : IEditableObject
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        private bool _inEdit = false;
        private XmlnsNamespace _original;

        public XmlnsNamespace(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public void BeginEdit()
        {
            if (!_inEdit)
            {
                _original = new XmlnsNamespace(Name, Path);
                _inEdit = true;
            }
        }

        public void CancelEdit()
        {
            if (_inEdit)
            {
                Name = _original.Name;
                Path = _original.Path;
                _inEdit = false;
            }
        }

        public void EndEdit()
        {
            if (_inEdit)
            {
                _inEdit = false;
                _original = null;
            }
        }
    }
}
