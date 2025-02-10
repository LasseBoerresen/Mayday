using LanguageExt;

namespace Generic;

public static class EnumerableExtensions
{
    public static Option<T> SingleOption<T>(this IEnumerable<T> enumerable)
    {
        return !enumerable.Any() ? Option<T>.None : enumerable.Single();
    }

    public static Option<T> SingleOption<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
    {
        return enumerable.Where(predicate).SingleOption();
    }

    public static Option<T> FirstOption<T>(this IEnumerable<T> enumerable)
    {
        return !enumerable.Any() ? Option<T>.None : enumerable.First();
    }

    public static Option<T> LastOption<T>(this IEnumerable<T> enumerable)
    {
        return !enumerable.Any() ? Option<T>.None : enumerable.Last();
    }

    public static IEnumerable<T> ExtendWithSelectMany<T>(
        this IEnumerable<T> enumerable, 
        Func<T, IEnumerable<T>> extension)
    {
        return enumerable.SelectMany(e => extension(e).Append(e));
    }

    public static IEnumerable<(T current, T next)> GetCurrentAndNextAsPair<T>(this IEnumerable<T> enumerable)
    {
        return enumerable.Zip(enumerable.Skip(1), (current, next) => (current, next));
    }

    public static IEnumerable<(T, U, Y)> ZipWithTwo<T, U, Y>(
        this IEnumerable<T> enumerable0,
        IEnumerable<U> enumerable1,
        IEnumerable<Y> enumerable2)
    {
        return enumerable0.Zip(enumerable1).Zip(enumerable2)
            .Select(tuple => (
                First: tuple.First.First, 
                Second: tuple.First.Second, 
                Third: tuple.Second));
    }

    public static IEnumerable<(T, T)> ZipWithNext<T>(this IEnumerable<T> enumerable)
    {
        var enumerableOfFirsts = Enumerable.SkipLast(enumerable, 1);
        var enumerableOfLasts = enumerable.Skip(1);

        return enumerableOfFirsts.Zip(enumerableOfLasts);
    }

    public static bool IsDistinct<T>(this IEnumerable<T> enumerable)
    {
        return enumerable.Count() == enumerable.Distinct().Count();
    }

    public static IEnumerable<T> FilterForSome<T>(this IEnumerable<Option<T>> enumerable)
    {
        return enumerable
            .Where(o => o.IsSome)
            .Select(o => o.IfNone(() => throw new InvalidOperationException()));
    }

    public static bool ContainsAll<T>(this IEnumerable<T> actual, IEnumerable<T> expected)
    {
        return expected.All(actual.Contains);
    }

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (var item in enumerable)
            action(item);
    }
    
    public static IEnumerable<R> Map<T, R>(this IEnumerable<T> enumerable, Func<T, R> func)
    {
        return enumerable.Select(func);
    }
}
