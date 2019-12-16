using System;
using System.Collections;
using System.Collections.Generic;

namespace Svelto.DataStructures
{
    public struct FasterListEnumerator<T> : IEnumerator<T>
    {
        public T Current => _current;

        public FasterListEnumerator(in T[] buffer, uint size)
        {
            _size = size;
            _counter = 0;
            _buffer = buffer;
            _current = default;
        }

        object IEnumerator.Current => throw new NotImplementedException();

        T IEnumerator<T>.Current => _current;

        public void Dispose()
        {}

        public bool MoveNext()
        {
            if (_counter < _size)
            {
                _current = _buffer[(uint) _counter++];

                return true;
            }

            _current = default;

            return false;
        }

        public void Reset()
        {
            _counter = 0;
        }

        bool IEnumerator.MoveNext()
        {
            return MoveNext();
        }

        void IEnumerator.Reset()
        {
            Reset();
        }

        readonly T[]  _buffer;
        int           _counter;
        readonly uint _size;
        T             _current;
    }
}