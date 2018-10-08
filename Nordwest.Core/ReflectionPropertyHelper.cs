using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Nordwest {
    public static class ReflectionPropertyHelper<T> {
        private static readonly ReadOnlyCollection<PropertyInfo> _infos = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(info => info.CanRead && info.CanWrite)
            .ToList().AsReadOnly();

        public static ReadOnlyCollection<PropertyInfo> Infos {
            get { return _infos; }
        }

        public static IEnumerable<Tuple<string, object>> GetValues(T item) {
            foreach (var info in _infos)
                yield return Tuple.Create(info.Name, info.GetValue(item, null));
        }
        public static void SetValues(T item, IEnumerable<Tuple<string, object>> tuples) {
            foreach (var tuple in tuples)
                SetValue(item, tuple.Item1, tuple.Item2);
        }

        public static void CopyValues(T from, T to) {
            foreach (var info in _infos)
                info.SetValue(to, info.GetValue(from, null), null);
        }

        public static object GetValue(T target, string propertyName) {
            return _infos.First(i => i.Name == propertyName).GetValue(target, null);
        }
        public static void SetValue(T target, string propertyName, object value) {
            _infos.First(i => i.Name == propertyName).SetValue(target, value, null);
        }
    }
}