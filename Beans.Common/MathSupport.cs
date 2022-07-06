namespace Beans.Common;

// This code adapted from here: https://stackoverflow.com/questions/4140719/calculate-median-in-c-sharp
public static class MathSupport
{
    public static void Swap<T>(this List<T> list, int i, int j)
    {
        if (i != j)
        {
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private static int Partition<T>(this List<T> list, int start, int end, Random? rnd = null) where T : IComparable<T>
    {
        if (rnd is not null)
        {
            list.Swap(end, rnd.Next(start, end + 1));
        }
        var pivot = list[end];
        var lastLow = start - 1;
        for (var i = start; i < end; i++)
        {
            if (list[i].CompareTo(pivot) <= 0)
            {
                list.Swap(i, ++lastLow);
            }
        }
        list.Swap(end, ++lastLow);
        return lastLow;
    }

    private static T NthOrderStatistic<T>(this List<T> list, int n, Random? rnd = null) where T : IComparable<T> =>
        NthOrderStatistic(list, n, 0, list.Count - 1, rnd);

    private static T NthOrderStatistic<T>(this List<T> list, int n, int start, int end, Random? rnd = null) where T : IComparable<T>
    {
        while (true)
        {
            var pivotIndex = list.Partition(start, end, rnd);
            if (pivotIndex == n)
            {
                return list[pivotIndex];
            }
            if (n < pivotIndex)
            {
                end = pivotIndex - 1;
            }
            else
            {
                start = pivotIndex + 1;
            }
        }
    }

    public static T Median<T>(this List<T> list) where T : IComparable<T> => list.NthOrderStatistic((list.Count - 1) / 2);

    public static TTarget Median<TSource, TTarget>(this IEnumerable<TSource> sequence, Func<TSource, TTarget> getValue)
        where TTarget : IComparable<TTarget>
    {
        var list = sequence.Select(getValue).ToList();
        var mid = (list.Count - 1) / 2;
        return list.NthOrderStatistic(mid);
    }
}
