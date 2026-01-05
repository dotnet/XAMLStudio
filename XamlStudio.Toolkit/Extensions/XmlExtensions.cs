// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace XamlStudio.Toolkit.Extensions
{
    public static class XmlExtensions
    {
        public static XAttribute GetNamedItem(this IEnumerable<XAttribute> list, string name)
        {
            return list.Where(item => item.Name == name).FirstOrDefault();
        }

        public static IEnumerable<XmlNode> GetTypedEnumerator(this XmlNodeList list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                yield return list.Item(i);
            }
        }

        public static IEnumerable<XmlAttribute> GetTypedEnumerator(this XmlAttributeCollection list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                yield return list.Item(i) as XmlAttribute;
            }
        }
    }
}
