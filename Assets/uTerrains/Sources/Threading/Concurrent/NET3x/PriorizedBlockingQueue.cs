#if !NET_4_6

using System.Collections.Generic;
using System.Threading;

namespace UltimateTerrains.Concurrent.NET3x
{
    public class PriorizedBlockingQueue<T> : IPriorizedBlockingQueue<T>
    {
        private readonly Queue<T> normalPriorityQueue;
        private readonly Queue<T> highPriorityQueue;
        private readonly object sync = new object();
        private bool closing;


        public bool IsEmpty {
            get { return normalPriorityQueue.Count == 0 && highPriorityQueue.Count == 0; }
        }

        public PriorizedBlockingQueue()
        {
            normalPriorityQueue = new Queue<T>(512);
            highPriorityQueue = new Queue<T>(512);
        }

        public void Add(T item)
        {
            lock (sync) {
                normalPriorityQueue.Enqueue(item);
                if (normalPriorityQueue.Count == 1) {
                    // wake up any blocked dequeue
                    Monitor.PulseAll(sync);
                }
            }
        }

        public void AddHighPriority(T item)
        {
            lock (sync) {
                highPriorityQueue.Enqueue(item);
                if (highPriorityQueue.Count == 1) {
                    // wake up any blocked dequeue
                    Monitor.PulseAll(sync);
                }
            }
        }

        public T BlockingTake()
        {
            lock (sync) {
                while (IsEmpty) {
                    if (closing) {
                        return default(T);
                    }

                    Monitor.Wait(sync);
                }

                return highPriorityQueue.Count > 0 ? highPriorityQueue.Dequeue() : normalPriorityQueue.Dequeue();
            }
        }

        public void Close()
        {
            lock (sync) {
                closing = true;
                Monitor.PulseAll(sync);
            }
        }
    }
}

#endif