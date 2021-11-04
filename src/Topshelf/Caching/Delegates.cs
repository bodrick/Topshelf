namespace Topshelf.Caching
{
    internal delegate void CacheItemCallback<in TKey, in TValue>(TKey key, TValue value);

    internal delegate TKey KeySelector<out TKey, in TValue>(TValue value);

    internal delegate TValue MissingValueProvider<in TKey, out TValue>(TKey key);
}
