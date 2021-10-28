namespace Topshelf.Caching
{
    internal delegate TKey KeySelector<out TKey, in TValue>(TValue value);
}