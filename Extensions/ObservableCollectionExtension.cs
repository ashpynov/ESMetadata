using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ESMetadata.Extensions
{
    public static class ObservableCollectionExtension
    {
        public static int RemoveAll<T>(
            this ObservableCollection<T> coll, Func<T, bool> condition)
        {
            List<T> itemsToRemove = coll.Where(condition).ToList();

            foreach (T itemToRemove in itemsToRemove)
            {
                coll.Remove(itemToRemove);
            }

            return itemsToRemove.Count;
        }
        public static int AddRange<T>(
            this ObservableCollection<T> coll, IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                coll.Add(item);
            }

            return items.Count();
        }
    }

    };
