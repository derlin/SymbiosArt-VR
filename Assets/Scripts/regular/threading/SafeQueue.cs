using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace symbiosart.threading
{
    public class SafeQueue<T>
    {
        private Queue<T> _queue = new Queue<T>();
        private object _lock = new object();

        public T Dequeue()
        {
            lock (_lock)
            {
                return _queue.Count == 0 ? default(T) : _queue.Dequeue();
            }
        }

        public void Enqueue(T t)
        {
            lock (_lock)
            {
                 _queue.Enqueue(t);
            }
        }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _queue.Count;
                }
            }
        }
    }
}
