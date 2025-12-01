using LanguageExt;

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
    
    public static Option<TValue> LookFor<Tkey, TValue>(
        this IDictionary<Tkey, TValue> dictionary, 
        Tkey key)
    {
        if (!dictionary.ContainsKey(key))
            return Option<TValue>.None;

        return dictionary[key];
    }

    // Duplication Necessary, because IDictionary somehow does not implement
    // IReadonlyCollection 
    public static Option<TValue> LookFor<Tkey, TValue>(
        this IReadOnlyDictionary<Tkey, TValue> dictionary,
        Tkey key)
    {
        if (!dictionary.ContainsKey(key))
            return Option<TValue>.None;

        return dictionary[key];
    }

    public static void AppendElement<Tkey, TElement>(
        this IDictionary<Tkey, List<TElement>> dictionary,
        Tkey key,
        TElement element)
    {
        if (dictionary.ContainsKey(key))
            dictionary[key].Add(element);
        else
            dictionary[key] = [element];
    }
}
