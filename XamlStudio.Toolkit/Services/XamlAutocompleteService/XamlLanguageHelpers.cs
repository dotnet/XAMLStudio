using System.Collections.Generic;
using System.Text.RegularExpressions;

//// Adapted From: https://mono.software/2017/04/11/custom-intellisense-with-monaco-editor/
//// MIT: https://github.com/isimic413/monaco-editor-custom-intellisense/blob/master/LICENSE.md

namespace XamlStudio.Toolkit.Services
{
    internal static class XamlLanguageHelpers
    {
        public static (bool IsCompletionAvailable, string ClearedText) GetAreaInfo(string text)
        {
            // opening for strings, comments and CDATA
            var items = new string[] { "\"", "\'", "<!--", "<![CDATA["};
            var isCompletionAvailable = true;

            // remove all comments, strings and CDATA
            var rgx = new Regex("\"([^\"\\]* (\\.[^\"\\]*)*)\"|\'([^\'\\]*(\\.[^\'\\]*)*)\'|<!--([\\s\\S])*?-->|<!\\[CDATA\\[(.*?)\\]\\]>"); // TODO: Cache

            text = rgx.Replace(text, "");

	        for (var i = 0; i < items.Length; i++)
            {
		        var itemIdx = text.IndexOf(items[i]);
		        if (itemIdx > -1)
                {
			        // we are inside one of unavailable areas, so we remote that area
			        // from our clear text
			        text = text.Substring(0, itemIdx);

			        // and the completion is not available
			        isCompletionAvailable = false;
		        }
            }

	        return (isCompletionAvailable, text);
        }

        public static (string TagName, bool IsAttributeSearch)? GetLastOpenedTag(string text)
        {
            // get all tags inside of the content
            var rgx = new Regex("<\\/*(?=\\S*)([a-zA-Z-:]+)"); // TODO: Cache

            var tags = rgx.Matches(text);
	        if (tags.Count == 0)
            {
		        return null;
	        }

	        // we need to know which tags are closed
	        var closingTags = new List<string>();
	        for (var i = tags.Count - 1; i >= 0; i--)
            {
                var tagv = tags[i].Value;

		        if (tagv.IndexOf("</") == 0)
                {
			        closingTags.Add(tagv.Substring("</".Length));
		        }
		        else
                {
			        // get the last position of the tag
			        var tagPosition = text.LastIndexOf(tags[i].Value);
			        var tag = tagv.Substring("<".Length);
			        var closingBracketIdx = text.IndexOf("/>", tagPosition);

			        // if the tag wasn't closed
			        if (closingBracketIdx == -1)
                    {
				        // if there are no closing tags or the current tag wasn't closed
				        if (closingTags.Count == 0 || closingTags[closingTags.Count - 1] != tag) {
					        // we found our tag, but let's get the information if we are looking for
					        // a child element or an attribute
					        text = text.Substring(tagPosition);
					        return (
                                tag,
                                text.Contains(" ")
						        ////text.IndexOf('<') > text.IndexOf('>')
					        );
				        }

                        // remove the last closed tag
                        closingTags.RemoveAt(closingTags.Count - 1);
			        }

			        // remove the last checked tag and continue processing the rest of the content
			        text = text.Substring(0, tagPosition);
		        }
	        }

            return null;
        }
    }
}
