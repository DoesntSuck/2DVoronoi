using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets
{
    public static class HashSetExtension
    {
        public static bool ContainsAll<T>(this HashSet<T> set, IEnumerable<T> otherSet)
        {
            foreach (T item in otherSet)
            {
                if (!set.Contains(item))
                    return false;
            }

            return true;
        }
    }
}
