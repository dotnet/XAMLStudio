using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XamlStudio.Toolkit.Helpers;
using XamlStudio.Toolkit.Models;

namespace XamlStudio.Toolkit.Services
{
    public class XamlAutocompleteService
    {
        public static XamlAutocompleteService Instance => Singleton<XamlAutocompleteService>.Instance;

        private const string RegexPattern_XmlnsNamespace = @"xmlns(|:(?<Prefix>[\w-]*))=""(?<Namespace>.*)""";

        private static Regex _namespaceSearcher = new Regex(RegexPattern_XmlnsNamespace, RegexOptions.Compiled);

        public IEnumerable<XmlnsNamespace> GetNamespaces(string content)
        {
            var match = XamlRenderService.GetInitialElement(content);
            if (match != null && match.Success)
            {
                var namespaces = new List<XmlnsNamespace>();

                foreach (Match m in _namespaceSearcher.Matches(match.Value))
                {
                    if (m.Success)
                    {
                        if (m.Groups["Prefix"].Length > 0)
                        {
                            namespaces.Add(new XmlnsNamespace(m.Groups["Prefix"].Value, m.Groups["Namespace"].Value));
                        }
                    }
                }

                return namespaces;
            }

            return Array.Empty<XmlnsNamespace>();
        }
    }
}
