namespace Topshelf.Caching
{
    internal delegate TValue MissingValueProvider<in TKey, out TValue>(TKey key);
}
