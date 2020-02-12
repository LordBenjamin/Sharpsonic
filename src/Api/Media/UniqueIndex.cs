using System;
using System.Collections.Generic;

namespace Sharpsonic.Api.Media {
    public class UniqueIndex<TKey, TValue> where TValue : class {
        private readonly Func<TValue, TKey> getKey;
        private readonly Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

        public UniqueIndex(Func<TValue, TKey> getKey) {
            this.getKey = getKey ?? throw new ArgumentNullException(nameof(getKey));
        }

        public void Add(TValue value) {
            dictionary.Add(getKey(value), value);
        }

        public TValue Get(TKey key) {
            if (dictionary.TryGetValue(key, out TValue value)) {
                return value;
            } else {
                return null;
            }
        }

        internal void Clear() {
            dictionary.Clear();
        }
    }
}
