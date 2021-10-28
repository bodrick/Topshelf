namespace Topshelf.Caching
{
    internal delegate TValue MissingValueProvider<TKey, TValue>(TKey key);
}