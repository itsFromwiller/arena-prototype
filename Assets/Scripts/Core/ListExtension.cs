using System.Collections.Generic;

public static class ListExtension
{
    public static void Randomize<T>(this IList<T> list)
    {
        int index = list.Count;
        while (index > 1)
        {
            --index;
            int swapIndex = UnityEngine.Random.Range(0, index + 1);
            // Modern C# tuple swap
            (list[swapIndex], list[index]) = (list[index], list[swapIndex]);
        }
    }
}