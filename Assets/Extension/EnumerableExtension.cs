using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnumerableExtension
{
    public static IEnumerable<T> Except<T>(this IList<T> items, int indexToSkip)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (i != indexToSkip)
                yield return items[i];
        }
    }
}
