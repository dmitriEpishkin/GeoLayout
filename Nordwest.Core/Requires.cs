using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nordwest {
    public static class Requires {
        [DebuggerStepThrough]
        public static void NotNull<T>(T argument) where T : class {
            if (argument == null)
                throw new ArgumentNullException();
        }
        [DebuggerStepThrough]
        public static void NotDefault<T>(T argument) {
            if (Equals(argument, default(T)))
                throw new ArgumentNullException();
        }
        [DebuggerStepThrough]
        public static void Argument(bool condition, string message = null) {
            if (condition)
                return;

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException();
            throw new ArgumentException(message);
        }
        [DebuggerStepThrough]
        public static void Empty<T>(ICollection<T> collection) {
            if (collection.Count > 0)
                throw new ArgumentException();
        }
        [DebuggerStepThrough]
        public static void NotEmpty<T>(ICollection<T> collection) {
            if (!(collection.Count > 0))
                throw new ArgumentException();
        }
        [DebuggerStepThrough]
        public static void Range(bool condition, string message = null) {
            if (condition)
                return;

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentOutOfRangeException();
            throw new ArgumentOutOfRangeException(message);
        }
        [DebuggerStepThrough]
        public static void State(bool condition) {
            if (!condition)
                throw new InvalidOperationException();
        }
        [DebuggerStepThrough]
        public static void NotNullOrWhiteSpace(string argument) {
            if (string.IsNullOrWhiteSpace(argument))
                throw new ArgumentException();
        }
        [DebuggerStepThrough]
        public static void ForAll(int from, int count, Predicate<int> condition) {
            for (int i = from; i < from + count; i++)
                if (!condition(i))
                    throw new ArgumentException();
        }
        [DebuggerStepThrough]
        public static void ForAll<T>(ICollection<T> collection, Predicate<T> condition) {
            foreach (var element in collection)
                if (!condition(element))
                    throw new ArgumentException();
        }
    }
}
