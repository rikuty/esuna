#if !NET_4_6

using System.Collections.Generic;

namespace UltimateTerrains.Concurrent.NET3x
{
    public class ConcurrentStack<T>
    {
        private readonly Stack<T> stack = new Stack<T>();

        public int Count {
            get {
                lock (stack) {
                    return stack.Count;
                }
            }
        }

        public bool IsEmpty {
            get {
                lock (stack) {
                    return stack.Count == 0;
                }
            }
        }

        public void Push(T item)
        {
            lock (stack)
                stack.Push(item);
        }

        public bool TryPop(out T item)
        {
            lock (stack) {
                if (stack.Count > 0) {
                    item = stack.Pop();
                    return true;
                }

                item = default(T);
                return false;
            }
        }

        public T PopOrFail()
        {
            lock (stack)
                return stack.Pop();
        }
    }
}

#endif