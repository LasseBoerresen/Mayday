namespace Generic;

public static class KeyValuePairExtensions
{
    public static KeyValuePair<K, U> Map<K, T, U>(this KeyValuePair<K, T> pair, Func<T, U> mapper)
    {
        return new(pair.Key, mapper(pair.Value));
    }
}
