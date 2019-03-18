#if NET_4_6
using System.Collections.Concurrent;
using System.Threading;

namespace UltimateTerrains.Concurrent.NET4x
{
    public sealed class PriorizedBlockingQueue<T> : IPriorizedBlockingQueue<T>
    {
        private readonly BlockingCollection<T> normalPriorityQueue;
        private readonly BlockingCollection<T> highPriorityQueue;
        private readonly BlockingCollection<T>[] queues;
        private readonly CancellationTokenSource cancellationTokenSource;

        public bool IsEmpty => normalPriorityQueue.Count == 0 && highPriorityQueue.Count == 0;

        public PriorizedBlockingQueue()
        {
            normalPriorityQueue = new BlockingCollection<T>(new ConcurrentQueue<T>());
            highPriorityQueue = new BlockingCollection<T>(new ConcurrentQueue<T>());
            this.queues = new[] {
                normalPriorityQueue,
                highPriorityQueue
            };
            cancellationTokenSource = new CancellationTokenSource();
        }

        public void Add(T item)
        {
            normalPriorityQueue.Add(item);
        }

        public void AddHighPriority(T item)
        {
            highPriorityQueue.Add(item);
        }

        public T BlockingTake()
        {
            T item;
            if (highPriorityQueue.TryTake(out item)) {
                return item;
            }

            BlockingCollection<T>.TakeFromAny(queues, out item, cancellationTokenSource.Token);
            return item;
        }

        public void Close()
        {
            cancellationTokenSource.Cancel();
            normalPriorityQueue.CompleteAdding();
        }
    }
}

#endif