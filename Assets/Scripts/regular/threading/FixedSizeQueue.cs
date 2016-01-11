using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace symbiosart.threading
{
    public class FixedSizedSafeQueue<T>
    {

        private List<T> _queue = new List<T>();
        public int Capacity { get; set; }

        public FixedSizedSafeQueue(int cap)
        {
            Capacity = cap;
        }

        public T Dequeue()
        {
            lock (_queue)
            {
                while (_queue.Count == 0) Monitor.Wait(_queue);
                return pop();
            }
        }

        public T TryDequeue()
        {
            lock (_queue)
            {
                return _queue.Count == 0 ? default(T) : pop();
            }
        }

        public void Enqueue(T t)
        {
            lock (_queue)
            {
                if (_queue.Contains(t)) return;

                _queue.Add(t);
                if (_queue.Count == 1)
                {
                    Monitor.Pulse(_queue);
                }
                else if (_queue.Count > Capacity)
                {
                    while (_queue.Count > Capacity) _queue.RemoveAt(0);
                }
            }
        }

        public int Count
        {
            get
            {
                lock (_queue)
                {
                    return _queue.Count;
                }
            }
        }


        private T pop()
        {
            var res = _queue.Last();
            _queue.RemoveAt(_queue.Count - 1);
            return res;
        }
    }
}
