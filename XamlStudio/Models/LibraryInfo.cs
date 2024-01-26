using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using XamlStudio.Services;

namespace XamlStudio.Models;

public class LibraryInfo
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("namespace")]
    public string Namespace { get; set; }

    [JsonProperty("docroot")]
    public string DocumentationRoot { get; set; }

    [JsonProperty("typehints")]
    public List<string> TypeHints { get; set; }

    // Helper for Binding
    public List<Type> GetTypes()
    {
        var list = LibraryService.Instance.GetTypesForNamespace(Namespace);

        list.Sort((left, right) => left.Name.CompareTo(right.Name));

        return list;
    }
}
