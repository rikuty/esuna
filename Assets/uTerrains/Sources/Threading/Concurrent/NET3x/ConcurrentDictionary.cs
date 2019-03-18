#if !NET_4_6

using System.Collections;
using System.Collections.Generic;

namespace UltimateTerrains.Concurrent.NET3x
{
    public class ConcurrentDictionary<K, V> : IEnumerable<KeyValuePair<K, V>>
    {
        private readonly Dictionary<K, V> dico;

        public ConcurrentDictionary()
        {
            dico = new Dictionary<K, V>();
        }

        public ConcurrentDictionary(IEqualityComparer<K> comparer)
        {
            dico = new Dictionary<K, V>(comparer);
        }

        public int Count {
            get {
                lock (dico)
                    return dico.Count;
            }
        }

        public bool TryGetValue(K key, out V value)
        {
            lock (dico)
                return dico.TryGetValue(key, out value);
        }

        public V GetOrAdd(K key, V value)
        {
            lock (dico) {
                V existingValue;
                if (dico.TryGetValue(key, out existingValue)) {
                    return existingValue;
                }
                dico.Add(key, value);
                return value;
            }
        }

        // THIS IS NOT THREAD SAFE
        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return dico.GetEnumerator();
        }

        // THIS IS NOT THREAD SAFE
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<K, V>>) dico).GetEnumerator();
        }
    }
}

#endif