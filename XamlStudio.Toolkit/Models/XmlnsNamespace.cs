using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XamlStudio.Toolkit.Models
{
    public struct XmlnsNamespace
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        public XmlnsNamespace(string name, string path)
        {
            Name = name;
            Path = path;
        }
    }
}
