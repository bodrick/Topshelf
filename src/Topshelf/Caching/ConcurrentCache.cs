using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Topshelf.Caching
{
    [Serializable]
    internal class ConcurrentCache<TKey, TValue> : ICache<TKey, TValue> where TKey : notnull
    {
        private readonly ConcurrentDictionary<TKey, TValue> _values;

        private CacheItemCallback<TKey, TValue> _duplicateValueAdded = ThrowOnDuplicateValue;

        private KeySelector<TKey, TValue> _keySelector = DefaultKeyAccessor;

        private MissingValueProvider<TKey, TValue> _missingValueProvider = ThrowOnMissingValue;

        private CacheItemCallback<TKey, TValue> _valueAddedCallback = DefaultCacheItemCallback;

        private CacheItemCallback<TKey, TValue> _valueRemovedCallback = DefaultCacheItemCallback;

        public ConcurrentCache() => _values = new ConcurrentDictionary<TKey, TValue>();

        public ConcurrentCache(MissingValueProvider<TKey, TValue> missingValueProvider)
            : this() => _missingValueProvider = missingValueProvider;

        public ConcurrentCache(IEqualityComparer<TKey> equalityComparer) =>
            _values = new ConcurrentDictionary<TKey, TValue>(equalityComparer);

        public ConcurrentCache(KeySelector<TKey, TValue> keySelector)
        {
            _values = new ConcurrentDictionary<TKey, TValue>();
            _keySelector = keySelector;
        }

        public ConcurrentCache(KeySelector<TKey, TValue> keySelector, IEnumerable<TValue> values)
            : this(keySelector) => Fill(values);

        public ConcurrentCache(IEqualityComparer<TKey> equalityComparer,
            MissingValueProvider<TKey, TValue> missingValueProvider)
            : this(equalityComparer) => _missingValueProvider = missingValueProvider;

        public ConcurrentCache(IEnumerable<KeyValuePair<TKey, TValue>> values) => _values = new ConcurrentDictionary<TKey, TValue>(values);

        public ConcurrentCache(IDictionary<TKey, TValue> values,
            MissingValueProvider<TKey, TValue> missingValueProvider)
            : this(values) => _missingValueProvider = missingValueProvider;

        public ConcurrentCache(IEnumerable<KeyValuePair<TKey, TValue>> values, IEqualityComparer<TKey> equalityComparer) =>
            _values = new ConcurrentDictionary<TKey, TValue>(values, equalityComparer);

        public ConcurrentCache(IDictionary<TKey, TValue> values,
            IEqualityComparer<TKey> equalityComparer,
            MissingValueProvider<TKey, TValue> missingValueProvider)
            : this(values, equalityComparer) => _missingValueProvider = missingValueProvider;

        public int Count => _values.Count;

        public CacheItemCallback<TKey, TValue> DuplicateValueAdded
        {
            set => _duplicateValueAdded = value;
        }

        public KeySelector<TKey, TValue> KeySelector
        {
            set => _keySelector = value;
        }

        public MissingValueProvider<TKey, TValue> MissingValueProvider
        {
            set => _missingValueProvider = value;
        }

        public CacheItemCallback<TKey, TValue> ValueAddedCallback
        {
            set => _valueAddedCallback = value;
        }

        public CacheItemCallback<TKey, TValue> ValueRemovedCallback
        {
            set => _valueRemovedCallback = value;
        }

        public TValue this[TKey key]
        {
            get => Get(key);
            set
            {
                if (_values.TryGetValue(key, out var existingValue))
                {
                    _valueRemovedCallback(key, existingValue);
                    _values[key] = value;
                    _valueAddedCallback(key, value);
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        public void Add(TKey key, TValue value)
        {
            var added = _values.TryAdd(key, value);
            if (added)
            {
                _valueAddedCallback(key, value);
            }
            else
            {
                _duplicateValueAdded(key, value);
            }
        }

        public void AddValue(TValue value)
        {
            var key = _keySelector(value);
            Add(key, value);
        }

        public void Clear() => _values.Clear();

        public void Each(Action<TValue> callback)
        {
            foreach (var value in _values)
            {
                callback(value.Value);
            }
        }

        public void Each(Action<TKey, TValue> callback)
        {
            foreach (var value in _values)
            {
                callback(value.Key, value.Value);
            }
        }

        public bool Exists(Predicate<TValue> predicate) => _values.Any(value => predicate(value.Value));

        public void Fill(IEnumerable<TValue> values)
        {
            foreach (var value in values)
            {
                var key = _keySelector(value);
                Add(key, value);
            }
        }

        public bool Find(Predicate<TValue> predicate, out TValue result)
        {
            foreach (var value in _values.Where(value => predicate(value.Value)))
            {
                result = value.Value;
                return true;
            }

            result = default(TValue);
            return false;
        }

        public TValue Get(TKey key) => Get(key, _missingValueProvider);

        public TValue Get(TKey key, MissingValueProvider<TKey, TValue> missingValueProvider)
        {
            var added = false;

            var value = _values.GetOrAdd(key, x =>
            {
                added = true;
                return missingValueProvider(x);
            });

            if (added)
            {
                _valueAddedCallback(key, value);
            }

            return value;
        }

        public TValue[] GetAll() => _values.Values.ToArray();

        public TKey[] GetAllKeys() => _values.Keys.ToArray();

        public IEnumerator<TValue> GetEnumerator() => _values.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _values.Values.GetEnumerator();

        public TValue GetValue(TKey key, TValue defaultValue) => _values.TryGetValue(key, out var value) ? value : defaultValue;

        public TValue GetValue(TKey key, Func<TValue> defaultValueProvider) =>
            _values.TryGetValue(key, out var value) ? value : defaultValueProvider();

        public bool Has(TKey key) => _values.ContainsKey(key);

        public bool HasValue(TValue value) => Has(_keySelector(value));

        public void Remove(TKey key)
        {
            if (_values.TryRemove(key, out var existingValue))
            {
                _valueRemovedCallback(key, existingValue);
            }
        }

        public void RemoveValue(TValue value) => Remove(_keySelector(value));

        public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value)
        {
            value = _values.ContainsKey(key) ? _values[key] : default;
            return value is not null;
        }

        public bool WithValue(TKey key, Action<TValue> callback)
        {
            if (_values.TryGetValue(key, out var value))
            {
                callback(value);
                return true;
            }

            return false;
        }

        public TResult WithValue<TResult>(TKey key, Func<TValue, TResult> callback, TResult defaultValue) =>
            _values.TryGetValue(key, out var value) ? callback(value) : defaultValue;

        public TResult WithValue<TResult>(TKey key, Func<TValue, TResult> callback, Func<TKey, TResult> defaultValue) =>
            _values.TryGetValue(key, out var value) ? callback(value) : defaultValue(key);

        private static void DefaultCacheItemCallback(TKey key, TValue value)
        {
            // Method intentionally left empty.
        }

        private static TKey DefaultKeyAccessor(TValue value) =>
            throw new InvalidOperationException("No default key accessor has been specified");

        private static void ThrowOnDuplicateValue(TKey key, TValue value) => throw new ArgumentException(
            $"An item with the same key already exists in the cache: {key}", nameof(key));

        private static TValue ThrowOnMissingValue(TKey key) =>
            throw new KeyNotFoundException($"The specified element was not found: {key}");
    }
}
