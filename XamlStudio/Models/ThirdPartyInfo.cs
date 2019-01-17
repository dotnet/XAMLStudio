using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XamlStudio.Helpers;
using XamlStudio.Services;

namespace XamlStudio.Models
{
    public class ThirdPartyInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("license")]
        public string License { get; set; }

        [JsonIgnore]
        public string LicenseAutomationName { get { return string.Format("License_Name_Format".GetLocalized(), License, Name); } }

        [JsonProperty("license_url")]
        public string LicenseUrl { get; set; }

        [JsonProperty("license_text")]
        public List<string> LicenseText { get; set; }

        [JsonIgnore]
        public string LicenseTextAutomationName { get { return string.Format("License_Text_Format".GetLocalized(), License, Name); } }

    }
}
