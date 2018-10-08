using System;
using System.Collections;
using System.Collections.Generic;

namespace Nordwest.Collections {
    public class Mapping<T1, T2> : IEnumerable<KeyValuePair<T1, T2>> {
        private readonly List<T1> _list1 = new List<T1>();
        private readonly List<T2> _list2 = new List<T2>();

        public void Add(T1 item1, T2 item2) {
            if (ContainsKey(item1) || ContainsValue(item2))
                throw new ArgumentException();

            _list1.Add(item1);
            _list2.Add(item2);
        }

        public T1 GetKey(T2 value) {
            return _list1[_list2.IndexOf(value)];
        }
        public T2 GetValue(T1 key) {
            return _list2[_list1.IndexOf(key)];
        }

        public bool TryGetKey(T2 value, out T1 key) {
            var index = _list2.IndexOf(value);
            if (index == -1) {
                key = default(T1);
                return false;
            }

            key = _list1[index];

            return true;
        }
        public bool TryGetValue(T1 key, out T2 value) {
            var index = _list1.IndexOf(key);
            if (index == -1) {
                value = default(T2);
                return false;
            }

            value = _list2[index];

            return true;
        }

        public bool ContainsKey(T1 key) {
            return _list1.Contains(key);
        }
        public bool ContainsValue(T2 value) {
            return _list2.Contains(value);
        }

        public T2 this[T1 item] {
            get { return GetValue(item); }
        }

        public T2 RemoveByKey(T1 key) {
            int index = _list1.IndexOf(key);
            if (index == -1)
                return default(T2);

            T2 item = _list2[index];
            RemoveAt(index);
            return item;
        }
        public T1 RemoveByValue(T2 value) {
            int index = _list2.IndexOf(value);
            if (index == -1)
                return default(T1);

            T1 item = _list1[index];
            RemoveAt(index);
            return item;
        }
        private void RemoveAt(int index) {
            _list1.RemoveAt(index);
            _list2.RemoveAt(index);
        }

        public void Clear() {
            _list1.Clear();
            _list2.Clear();
        }

        public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator() {
            for (int i = 0; i < _list1.Count; i++)
                yield return new KeyValuePair<T1, T2>(_list1[i], _list2[i]);
        }
        IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }

    }
}