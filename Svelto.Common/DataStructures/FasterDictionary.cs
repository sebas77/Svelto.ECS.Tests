﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Svelto.DataStructures
{
    /// <summary>
    /// This dictionary has been created for just one reason: I needed a dictionary that would have let me iterate
    /// over the values as an array, directly, without generating one or using an iterator.
    /// For this goal is N times faster than the standard dictionary. Faster dictionary is also faster than
    /// the standard dictionary for most of the operations, but the difference is negligible. The only slower operation
    /// is resizing the memory on add, as this implementation needs to use two separate arrays compared to the standard
    /// one
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    
    public interface IFasterDictionary<TKey, TValue> where TKey : IEquatable<TKey>
    {
        uint Count { get; }
        void Add(TKey key, in TValue value);
        void Set(TKey key, in TValue value);
        void Clear();
        void FastClear();
        bool ContainsKey(TKey key);
        bool TryGetValue(TKey key, out TValue result);
        ref TValue GetOrCreate(TKey key);
        ref TValue GetOrCreate(TKey key, System.Func<TValue> builder);
        ref TValue GetValueByRef(TKey key);
        void SetCapacity(uint size);

        TValue this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set;
        }

        bool Remove(TKey key);
        void Trim();
        bool TryFindIndex(TKey key, out uint findIndex);
        ref TValue GetDirectValue(uint index);
        uint GetIndex(TKey key);
    }
    
    public sealed class FasterDictionary<TKey, TValue> : IFasterDictionary<TKey, TValue>,
        IDisposable where TKey : IEquatable<TKey>
    {
        protected FasterDictionary(uint size)
        {
            _fasterDictionaryImplementation = new FasterDictionaryStruct<TKey, TValue>(size);
        }

        public FasterDictionary() : this(1)
        {}

        public void Dispose()
        {
            _fasterDictionaryImplementation.Dispose();
            
            GC.SuppressFinalize(this);
        }

        ~FasterDictionary()
        {
            Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(TKey key)
        {
            return _fasterDictionaryImplementation.Remove(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trim()
        {
            _fasterDictionaryImplementation.Trim();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindIndex(TKey key, out uint findIndex)
        {
            return _fasterDictionaryImplementation.TryFindIndex(key, out findIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetDirectValue(uint index)
        {
            return ref _fasterDictionaryImplementation.GetDirectValue(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex(TKey key)
        {
            return _fasterDictionaryImplementation.GetIndex(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue result)
        {
            return _fasterDictionaryImplementation.TryGetValue(key, out result);
        }

        public ref TValue GetOrCreate(TKey key)
        {
            return ref _fasterDictionaryImplementation.GetOrCreate(key);
        }

        public ref TValue GetOrCreate(TKey key, Func<TValue> builder)
        {
            return ref _fasterDictionaryImplementation.GetOrCreate(key, builder);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueByRef(TKey key)
        {
            return ref _fasterDictionaryImplementation.GetValueByRef(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCapacity(uint size)
        {
            _fasterDictionaryImplementation.SetCapacity(size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FasterDictionaryStruct<TKey, TValue>.FasterDictionaryKeyValueEnumerator GetEnumerator()
        {
            return _fasterDictionaryImplementation.GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(TKey key, in TValue value)
        {
            _fasterDictionaryImplementation.Set(key, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _fasterDictionaryImplementation.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(TKey key)
        {
            return _fasterDictionaryImplementation.ContainsKey(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FastClear()
        {
            _fasterDictionaryImplementation.FastClear();
        }

        public TValue[] unsafeValues 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _fasterDictionaryImplementation.unsafeValues;
        }

        public uint Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _fasterDictionaryImplementation.Count;
        }

        public TValue[] GetValuesArray(out uint count)
        {
            return _fasterDictionaryImplementation.GetValuesArray(out count);
        }

        public void Add(TKey key, TValue value)
        {
            _fasterDictionaryImplementation.Add(key, value);
        }

        public void Add(TKey key, in TValue value)
        {
            _fasterDictionaryImplementation.Add(key, in value);
        }

        public TValue this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _fasterDictionaryImplementation[key];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _fasterDictionaryImplementation[key] = value;
        }
        
        FasterDictionaryStruct<TKey, TValue> _fasterDictionaryImplementation;
        bool                                 _disposed;
    }

    public class FasterDictionaryException : Exception
    {
        public FasterDictionaryException(string keyAlreadyExisting) : base(keyAlreadyExisting) { }
    }
}