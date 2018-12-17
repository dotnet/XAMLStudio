using System.Globalization;
using Windows.Globalization.DateTimeFormatting;

namespace XamlStudio.Helpers
{
    public class CultureInfoHelper
    {
        public static CultureInfo GetCurrentCulture()
        {
            var cultureName = new DateTimeFormatter("longdate", new[] { "US" }).ResolvedLanguage;

            return new CultureInfo(cultureName);
        }
    }
}
