using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Topshelf.Caching
{
    /// <summary>
    /// A read-only view of a cache. Methods that are able to modify the cache contents are not
    /// available in this reduced interface. Methods on this interface will NOT invoke a missing
    /// item provider.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    internal interface IReadCache<TKey, TValue> : IEnumerable<TValue>
    {
        /// <summary>
        /// The number of items in the cache
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Calls the specified callback with each value in the cache
        /// </summary>
        /// <param name="callback">A callback that accepts the value for each item in the cache</param>
        void Each(Action<TValue> callback);

        /// <summary>
        /// Calls the specified callback with each item in the cache
        /// </summary>
        /// <param name="callback">A callback that accepts the key and value for each item in the cache</param>
        void Each(Action<TKey, TValue> callback);

        /// <summary>
        /// Uses a predicate to scan the cache for a matching value
        /// </summary>
        /// <param name="predicate">The predicate to run against each value</param>
        /// <returns>True if a matching value exists, otherwise false</returns>
        bool Exists(Predicate<TValue> predicate);

        /// <summary>
        /// Uses a predicate to scan the cache for a matching value
        /// </summary>
        /// <param name="predicate">The predicate to run against each value</param>
        /// <param name="result">The matching value</param>
        /// <returns>True if a matching value was found, otherwise false</returns>
        bool Find(Predicate<TValue> predicate, [NotNullWhen(true)] out TValue? result);

        /// <summary>
        /// Gets all values that are stored in the cache
        /// </summary>
        /// <returns>An array of every value in the dictionary</returns>
        TValue[] GetAll();

        /// <summary>
        /// Gets all keys that are stored in the cache
        /// </summary>
        /// <returns>An array of every key in the dictionary</returns>
        TKey[] GetAllKeys();

        /// <summary>
        /// Checks if the key exists in the cache
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>True if the key exists, otherwise false</returns>
        bool Has(TKey key);

        /// <summary>
        /// Checks if a value exists in the cache
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>True if the value exists, otherwise false</returns>
        bool HasValue(TValue value);
    }
}
