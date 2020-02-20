using System;
using System.Collections.Generic;

namespace Sharpsonic.DataAccess.InMemory {
    public class NonUniqueIndex<TKey, TValue> where TValue : class {
        private readonly Func<TValue, TKey> getKey;
        private readonly Dictionary<TKey, List<TValue>> dictionary = new Dictionary<TKey, List<TValue>>();

        public NonUniqueIndex(Func<TValue, TKey> getKey) {
            this.getKey = getKey ?? throw new ArgumentNullException(nameof(getKey));
        }

        public void Add(TValue value) {
            TKey key = getKey(value);
                ;

            if (!dictionary.TryGetValue(key, out List<TValue> list)) {
                list = new List<TValue>();
                dictionary.Add(key, list);
            }

            list.Add(value);
        }

        public List<TValue> Get(TKey key) {
            if (dictionary.TryGetValue(key, out List<TValue> list)) {
                return list;
            } else {
                return null;
            }
        }

        internal void Clear() {
            dictionary.Clear();
        }

        internal void Remove(TValue value) {
            if (dictionary.TryGetValue(getKey(value), out List<TValue> list)) {
                list.Remove(value);
            }
        }
    }
}
