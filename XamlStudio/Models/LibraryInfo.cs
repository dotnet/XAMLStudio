using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XamlStudio.Services;

namespace XamlStudio.Models
{
    public class LibraryInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("namespace")]
        public string Namespace { get; set; }

        [JsonProperty("docroot")]
        public string DocumentationRoot { get; set; }

        // Helper for Binding
        public List<Type> GetTypes()
        {
            var list = LibraryService.Instance.GetTypesForNamespace(Namespace);

            list.Sort((left, right) => left.Name.CompareTo(right.Name));

            return list;
        }
    }
}
