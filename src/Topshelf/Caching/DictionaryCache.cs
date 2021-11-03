using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Topshelf.Caching
{
    [Serializable]
    internal class DictionaryCache<TKey, TValue> : ICache<TKey, TValue> where TKey : notnull
    {
        private readonly IDictionary<TKey, TValue> _values;
        private CacheItemCallback<TKey, TValue> _duplicateValueAdded = ThrowOnDuplicateValue;
        private KeySelector<TKey, TValue> _keySelector = DefaultKeyAccessor;
        private MissingValueProvider<TKey, TValue> _missingValueProvider = ThrowOnMissingValue;
        private CacheItemCallback<TKey, TValue> _valueAddedCallback = DefaultCacheItemCallback;
        private CacheItemCallback<TKey, TValue> _valueRemovedCallback = DefaultCacheItemCallback;

        public DictionaryCache() => _values = new Dictionary<TKey, TValue>();

        public DictionaryCache(MissingValueProvider<TKey, TValue> missingValueProvider) : this() =>
            _missingValueProvider = missingValueProvider;

        public DictionaryCache(IEqualityComparer<TKey> equalityComparer) => _values = new Dictionary<TKey, TValue>(equalityComparer);

        public DictionaryCache(KeySelector<TKey, TValue> keySelector)
        {
            _values = new Dictionary<TKey, TValue>();
            _keySelector = keySelector;
        }

        public DictionaryCache(KeySelector<TKey, TValue> keySelector, IEnumerable<TValue> values) : this(keySelector) => Fill(values);

        public DictionaryCache(IEqualityComparer<TKey> equalityComparer, MissingValueProvider<TKey, TValue> missingValueProvider) :
            this(equalityComparer) => _missingValueProvider = missingValueProvider;

        public DictionaryCache(IEnumerable<KeyValuePair<TKey, TValue>> values) => _values = values.ToDictionary(x => x.Key, x => x.Value);

        public DictionaryCache(IDictionary<TKey, TValue> values, bool copy = true) =>
            _values = copy
                ? new Dictionary<TKey, TValue>(values)
                : values;

        public DictionaryCache(IDictionary<TKey, TValue> values, MissingValueProvider<TKey, TValue> missingValueProvider) : this(values) =>
            _missingValueProvider = missingValueProvider;

        public DictionaryCache(IDictionary<TKey, TValue> values, MissingValueProvider<TKey, TValue> missingValueProvider, bool copy) :
            this(values, copy) => _missingValueProvider = missingValueProvider;

        public DictionaryCache(IEnumerable<KeyValuePair<TKey, TValue>> values, IEqualityComparer<TKey> equalityComparer) =>
            _values = values.ToDictionary(x => x.Key, x => x.Value, equalityComparer);

        public DictionaryCache(IDictionary<TKey, TValue> values, IEqualityComparer<TKey> equalityComparer) =>
            _values = new Dictionary<TKey, TValue>(values, equalityComparer);

        public DictionaryCache(IDictionary<TKey, TValue> values, IEqualityComparer<TKey> equalityComparer,
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
            try
            {
                _values.Add(key, value);
                _valueAddedCallback(key, value);
            }
            catch (ArgumentException)
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
            foreach (var (key, value) in _values)
            {
                callback(key, value);
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
            if (_values.TryGetValue(key, out var value))
            {
                return value;
            }

            value = missingValueProvider(key);

            Add(key, value);

            return value;
        }

        public TValue[] GetAll() => _values.Values.ToArray();

        public TKey[] GetAllKeys() => _values.Keys.ToArray();

        public IEnumerator<TValue> GetEnumerator() => _values.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _values.Values.GetEnumerator();

        public TValue GetValue(TKey key, TValue defaultValue) => _values.TryGetValue(key, out var value) ? value : defaultValue;

        public TValue GetValue(TKey key, Func<TValue> defaultValueProvider) => _values.TryGetValue(key, out var value) ? value : defaultValueProvider();

        public bool Has(TKey key) => _values.ContainsKey(key);

        public bool HasValue(TValue value) => Has(_keySelector(value));

        public void Remove(TKey key)
        {
            if (_values.TryGetValue(key, out var existingValue))
            {
                _valueRemovedCallback(key, existingValue);
                _values.Remove(key);
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

        public TResult WithValue<TResult>(TKey key, Func<TValue, TResult> callback, TResult defaultValue)
        {
            if (_values.TryGetValue(key, out var value))
            {
                return callback(value);
            }

            return defaultValue;
        }

        public TResult WithValue<TResult>(TKey key, Func<TValue, TResult> callback, Func<TKey, TResult> defaultValue)
        {
            if (_values.TryGetValue(key, out var value))
            {
                return callback(value);
            }

            return defaultValue(key);
        }

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
