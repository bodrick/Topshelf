using System;
using System.Collections;
using System.Collections.Generic;

namespace Topshelf.Caching
{
    internal class GenericTypeCache<TInterface> : ICache<Type, TInterface>
    {
        private readonly ICache<Type, TInterface> _cache;

        /// <summary>
        /// Constructs a cache for the specified generic type
        /// </summary>
        /// <param name="genericType">The generic type to close</param>
        public GenericTypeCache(Type genericType) : this(genericType, new ConcurrentCache<Type, TInterface?>(DefaultMissingValueProvider(genericType)))
        {
        }

        /// <summary>
        /// Constructs a cache for the specified generic type.
        /// </summary>
        /// <param name="genericType">The generic type to close</param>
        /// <param name="missingValueProvider">The implementation provider, which must close the generic type with the passed type</param>
        public GenericTypeCache(Type genericType, MissingValueProvider<Type, TInterface> missingValueProvider)
            : this(genericType, new ConcurrentCache<Type, TInterface>(missingValueProvider))
        {
        }

        private GenericTypeCache(Type genericType, ICache<Type, TInterface> cache)
        {
            if (!genericType.IsGenericType)
            {
                throw new ArgumentException("The type specified must be a generic type", nameof(genericType));
            }

            if (genericType.GetGenericArguments().Length != 1)
            {
                throw new ArgumentException("The generic type must have a single generic argument");
            }

            GenericType = genericType;
            _cache = cache;
        }

        public int Count => _cache.Count;

        public CacheItemCallback<Type, TInterface> DuplicateValueAdded
        {
            set => _cache.DuplicateValueAdded = value;
        }

        public Type GenericType { get; }

        public KeySelector<Type, TInterface> KeySelector
        {
            set => _cache.KeySelector = value;
        }

        public MissingValueProvider<Type, TInterface> MissingValueProvider
        {
            set => _cache.MissingValueProvider = value;
        }

        public CacheItemCallback<Type, TInterface> ValueAddedCallback
        {
            set => _cache.ValueAddedCallback = value;
        }

        public CacheItemCallback<Type, TInterface> ValueRemovedCallback
        {
            set => _cache.ValueRemovedCallback = value;
        }

        public TInterface this[Type key]
        {
            get => _cache[key];
            set => _cache[key] = value;
        }

        public void Add(Type key, TInterface value) => _cache.Add(key, value);

        public void AddValue(TInterface value) => _cache.AddValue(value);

        public void Clear() => _cache.Clear();

        public void Each(Action<TInterface> callback) => _cache.Each(callback);

        public void Each(Action<Type, TInterface> callback) => _cache.Each(callback);

        public bool Exists(Predicate<TInterface> predicate) => _cache.Exists(predicate);

        public void Fill(IEnumerable<TInterface> values) => _cache.Fill(values);

        public bool Find(Predicate<TInterface> predicate, out TInterface result) => _cache.Find(predicate, out result);

        public TInterface Get(Type key) => _cache.Get(key);

        public TInterface Get(Type key, MissingValueProvider<Type, TInterface> missingValueProvider) =>
            _cache.Get(key, missingValueProvider);

        public TInterface[] GetAll() => _cache.GetAll();

        public Type[] GetAllKeys() => _cache.GetAllKeys();

        public IEnumerator<TInterface> GetEnumerator() => _cache.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public TInterface GetValue(Type key, TInterface defaultValue) => _cache.GetValue(key, defaultValue);

        public TInterface GetValue(Type key, Func<TInterface> defaultValueProvider) => _cache.GetValue(key, defaultValueProvider);

        public bool Has(Type key) => _cache.Has(key);

        public bool HasValue(TInterface value) => _cache.HasValue(value);

        public void Remove(Type key) => _cache.Remove(key);

        public void RemoveValue(TInterface value) => _cache.RemoveValue(value);

        public bool TryGetValue(Type key, out TInterface value) => _cache.TryGetValue(key, out value);

        public bool WithValue(Type key, Action<TInterface> callback) => _cache.WithValue(key, callback);

        public TResult WithValue<TResult>(Type key,
            Func<TInterface, TResult> callback,
            TResult defaultValue) => _cache.WithValue(key, callback, defaultValue);

        public TResult WithValue<TResult>(Type key, Func<TInterface, TResult> callback, Func<Type, TResult> defaultValue) =>
            _cache.WithValue(key, callback, defaultValue);

        private static MissingValueProvider<Type, TInterface> DefaultMissingValueProvider(Type genericType) => type =>
        {
            var buildType = genericType.MakeGenericType(type);
            return (TInterface)Activator.CreateInstance(buildType);
        };
    }
}
