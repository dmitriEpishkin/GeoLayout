using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Nordwest {
    public static class Linq {
        [Pure]
        public static T WithMin<T, U>(this IEnumerable<T> e, Func<T, U> selector) where U : IComparable<U> {
            Requires.NotNull(e);
            Requires.NotNull(selector);

            var comparer = Comparer<U>.Default;
            T result;

            IEnumerator<T> enumerator = null;
            try {
                enumerator = e.GetEnumerator();

                if (!enumerator.MoveNext())
                    throw new InvalidOperationException();

                result = enumerator.Current;

                var minimum = selector(result);
                while (enumerator.MoveNext()) {
                    var current = enumerator.Current;
                    var currentValue = selector(current);

                    if (comparer.Compare(minimum, currentValue) > 0) {
                        minimum = currentValue;
                        result = current;
                    }
                }

            } finally {
                enumerator?.Dispose();
            }

            return result;
        }
        [Pure]
        public static T WithMax<T, U>(this IEnumerable<T> e, Func<T, U> selector) where U : IComparable<U> {
            Requires.NotNull(e);
            Requires.NotNull(selector);

            var comparer = Comparer<U>.Default;
            T result;

            IEnumerator<T> enumerator = null;
            try {
                enumerator = e.GetEnumerator();

                if (!enumerator.MoveNext())
                    throw new InvalidOperationException();

                result = enumerator.Current;

                var maximum = selector(result);
                while (enumerator.MoveNext()) {
                    var current = enumerator.Current;
                    var currentValue = selector(current);

                    if (comparer.Compare(maximum, currentValue) < 0) {
                        maximum = currentValue;
                        result = current;
                    }
                }

            } finally {
                enumerator?.Dispose();
            }

            return result;
        }

        [Pure]
        public static bool FastAny<T>(this IEnumerable<T> source) {
            Requires.NotNull(source);

            var collection = source as ICollection;
            return collection != null ?
                collection.Count > 0 :
                source.Any();
        }

        [Pure]
        public static bool SequenceMatch<T>(this IEnumerable<T> e, Func<T, T, bool> match) {
            Requires.NotNull(e);
            Requires.NotNull(match);

            IEnumerator<T> enumerator = null;
            try {
                enumerator = e.GetEnumerator();

                if (!enumerator.MoveNext())
                    return true;

                var prev = enumerator.Current;
                while (enumerator.MoveNext()) {
                    if (!match(prev, enumerator.Current))
                        return false;
                    prev = enumerator.Current;
                }
            } finally {
                enumerator?.Dispose();
            }
            return true;
        }

        public static void SortBy<T, U>(this T[] array, Func<T, U> selector) where U : IComparable<U> {
            Requires.NotNull(array);
            Requires.NotNull(selector);

            var comparer = Comparer<U>.Default;

            Array.Sort(array, (x, y) => comparer.Compare(selector(x), selector(y)));
        }
        public static void SortBy<T, U>(this List<T> list, Func<T, U> selector) where U : IComparable<U> {
            Requires.NotNull(list);
            Requires.NotNull(selector);

            var comparer = Comparer<U>.Default;

            list.Sort((x, y) => comparer.Compare(selector(x), selector(y)));
        }
    }
}
