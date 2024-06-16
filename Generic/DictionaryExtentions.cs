namespace Generic;

public static class DictionaryExtentions
{
    public static IDictionary<TKey, TValueNew> MapValue<TKey, TValue, TValueNew>(
        this IDictionary<TKey, TValue> dictionary,
        Func<TValue, TValueNew> mapper) where TKey : notnull
    {
            return dictionary
            .Select(kvp => new KeyValuePair<TKey,TValueNew>(kvp.Key, mapper(kvp.Value)))
            .ToDictionary();
        }
}
