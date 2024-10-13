using System.Collections.Generic;

namespace ESMetadata.Extensions
{
    public static class ListExtension
    {
        public static T Pop<T>(this List<T> coll, T item) => (item != null && coll.Remove(item)) ? item : default;
    }

    };
