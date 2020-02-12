using System;
using System.Collections.Generic;

namespace Sharpsonic.Api.Media {
    public class NonUniqueIndex<TKey, TValue> where TValue : class {
        private readonly Func<TValue, TKey> getKey;
        private readonly Dictionary<TKey, List<TValue>> dictionary = new Dictionary<TKey, List<TValue>>();

        public NonUniqueIndex(Func<TValue, TKey> getKey) {
            this.getKey = getKey ?? throw new ArgumentNullException(nameof(getKey));
        }

        public void Add(TValue value) {
            TKey key = getKey(value);
            List<TValue> list = dictionary.GetValueOrDefault(key);

            if(list == null) {
                list = new List<TValue>();
                dictionary.Add(key, list);
            }

            list.Add(value);
        }

        public List<TValue> Get(TKey key) {
            if (dictionary.TryGetValue(key, out List<TValue> list)) {
                return list;
            }
            else {
                return null;
            }
        }

        internal void Clear() {
            dictionary.Clear();
        }
    }
}
