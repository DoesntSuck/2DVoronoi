using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnumerableExtension
{

    public static IEnumerable<T> Except<T>(this T[] items, int indexToSkip)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (i != indexToSkip)
                yield return items[i];
        }
    }
}
