#if !NET_4_6

using System.Collections.Generic;

namespace UltimateTerrains.Concurrent.NET3x
{
    public class ConcurrentQueue<T>
    {
        private readonly Queue<T> queue = new Queue<T>();

        public int Count {
            get {
                lock (queue) {
                    return queue.Count;
                }
            }
        }

        public bool IsEmpty {
            get {
                lock (queue) {
                    return queue.Count == 0;
                }
            }
        }

        public void Enqueue(T item)
        {
            lock (queue)
                queue.Enqueue(item);
        }

        public T DequeueOrFail()
        {
            lock (queue)
                return queue.Dequeue();
        }
    }
}

#endif