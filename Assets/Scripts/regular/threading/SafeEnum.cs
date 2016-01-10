using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace symbiosart.threading
{
    /// <summary>
    /// A thread-safe IEnumerator implementation.
    /// See: http://www.codeproject.com/KB/cs/safe_enumerable.aspx
    /// </summary>
    public class SafeEnumerator<T> : IEnumerator<T>
    {
        // this is the (thread-unsafe)
        // enumerator of the underlying collection
        private readonly IEnumerator<T> _inner;

        // this is the object we shall lock on.
        private readonly object _lock;

        public SafeEnumerator(IEnumerator<T> inner, object @lock)
        {
            _inner = inner;
            _lock = @lock;

            // entering lock in constructor
            Monitor.Enter(_lock);
        }

        public T Current
        {
            get { return _inner.Current; }
        }

        object IEnumerator.Current
        {
            get
            {
                return _inner.Current;
            }
        }


        public void Dispose()
        {
            // .. and exiting lock on Dispose()
            // This will be called when foreach loop finishes
            Monitor.Exit(_lock);
        }

        /// <remarks>
        /// we just delegate actual implementation
        /// to the inner enumerator, that actually iterates
        /// over some collection
        /// </remarks>
        public bool MoveNext()
        {
            return _inner.MoveNext();
        }

        public void Reset()
        {
            _inner.Reset();
        }
    }
}
