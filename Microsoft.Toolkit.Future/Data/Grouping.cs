// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;

namespace Microsoft.Toolkit.Future.Data
{
    public class Grouping<TKey, TElement> : IGrouping<TKey, TElement>, IEnumerable<TElement>, IEnumerable, ICollectionViewGroup
    {
        public object Group => Key;

        public IObservableVector<object> GroupItems => (IObservableVector<object>)new ObservableCollection<object>((IEnumerable<object>)this);

        public TKey Key { get; private set; }

        private IEnumerable<TElement> Items { get; }

        public Grouping(TKey key, IEnumerable<TElement> items)
        {
            Key = key;
            Items = items;
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        public override string ToString()
        {
            return Key?.ToString();
        }
    }

    public static class EnumerableExtensions
    {
        public static Grouping<TKey, TElement> ToGroup<TKey, TElement>(this IEnumerable<TElement> list, TKey key)
            => new Grouping<TKey, TElement>(key, list);

        public static IEnumerable<Grouping<TKey, TElement>> ToGroup<TKey, TElement>(this IEnumerable<TElement> list, Func<TElement, TKey> keySelector)
            => list.GroupBy(keySelector, (key, items) => new Grouping<TKey, TElement>(key, items));
    }

    public static class GroupingExtensions
    {
        // Converts a System Group to this Group.
        public static Grouping<TKey, TElement> ToGroup<TKey, TElement>(this IGrouping<TKey, TElement> group)
            => new Grouping<TKey, TElement>(group.Key, group.AsEnumerable());
    }
}
