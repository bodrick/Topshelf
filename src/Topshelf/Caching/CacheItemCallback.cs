namespace Topshelf.Caching
{
    internal delegate void CacheItemCallback<in TKey, in TValue>(TKey key, TValue value);
}