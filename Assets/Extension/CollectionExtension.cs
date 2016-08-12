using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Linq
{
    public static class CollectionExtension
    {
        /// <summary>
        /// Creates a new array containing the elements in the specified portion of this array
        /// </summary>
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        /// <summary>
        /// Checks that this hash set contains all the items in the given enumeration
        /// </summary>
        public static bool ContainsAll<T>(this HashSet<T> set, IEnumerable<T> otherSet)
        {
            // Iterate through all items in other set
            foreach (T item in otherSet)
            {
                // If this set doesn't contain ANY one of the other items, then ContainsAll = false
                if (!set.Contains(item))
                    return false;
            }

            // This set contains ALL the items from other set
            return true;
        }
    }
}
