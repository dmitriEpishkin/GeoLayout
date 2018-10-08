using System;
using System.Collections.Generic;
using Validation;
namespace System.Collections.Immutable {
    /// <summary>Provides a set of initialization methods for instances of the <see cref="T:System.Collections.Immutable.ImmutableList`1" /> class.</summary>
    public static class ImmutableList {
        /// <summary>Creates an empty immutable list.</summary>
        /// <returns>An empty .</returns>
        /// <typeparam name="T">The type of items to be stored in the .</typeparam>
        public static ImmutableList<T> Create<T>() {
            return ImmutableList<T>.Empty;
        }
        /// <summary>Creates a new immutable list that contains the specified item.</summary>
        /// <returns>A new  that contains the specified item.</returns>
        /// <param name="item">The item to prepopulate the list with.</param>
        /// <typeparam name="T">The type of items in the .</typeparam>
        public static ImmutableList<T> Create<T>(T item) {
            return ImmutableList<T>.Empty.Add(item);
        }
        /// <summary>Creates a new immutable list that contains the specified items.</summary>
        /// <returns>Returns an immutable list that contains the specified items.</returns>
        /// <param name="items">The items to add to the list.</param>
        /// <typeparam name="T">The type of items in the .</typeparam>
        public static ImmutableList<T> CreateRange<T>(IEnumerable<T> items) {
            return ImmutableList<T>.Empty.AddRange(items);
        }
        /// <summary>Creates a new immutable list that contains the specified array of items.</summary>
        /// <returns>A new immutable list that contains the specified items.</returns>
        /// <param name="items">An array that contains the items to prepopulate the list with.</param>
        /// <typeparam name="T">The type of items in the .</typeparam>
        public static ImmutableList<T> Create<T>(params T[] items) {
            return ImmutableList<T>.Empty.AddRange(items);
        }
        /// <summary>Creates a new immutable list builder.</summary>
        /// <returns>The immutable collection builder.</returns>
        /// <typeparam name="T">The type of items stored by the collection.</typeparam>
        public static ImmutableList<T>.Builder CreateBuilder<T>() {
            return ImmutableList.Create<T>().ToBuilder();
        }
        /// <summary>Enumerates a sequence and produces an immutable list of its contents.</summary>
        /// <returns>An immutable list that contains the items in the specified sequence.</returns>
        /// <param name="source">The sequence to enumerate.</param>
        /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
        public static ImmutableList<TSource> ToImmutableList<TSource>(this IEnumerable<TSource> source) {
            ImmutableList<TSource> immutableList = source as ImmutableList<TSource>;
            if (immutableList != null) {
                return immutableList;
            }
            return ImmutableList<TSource>.Empty.AddRange(source);
        }
        /// <summary>Replaces the first equal element in the list with the specified element.</summary>
        /// <returns>The new list -- even if the value being replaced is equal to the new value for that position.</returns>
        /// <param name="list">The list to search.</param>
        /// <param name="oldValue">The element to replace.</param>
        /// <param name="newValue">The element to replace the old element with.</param>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <exception cref="T:System.ArgumentException">Thrown when the old value does not exist in the list.</exception>
        public static IImmutableList<T> Replace<T>(this IImmutableList<T> list, T oldValue, T newValue) {
            Requires.NotNull<IImmutableList<T>>(list, "list");
            return list.Replace(oldValue, newValue, EqualityComparer<T>.Default);
        }
        /// <summary>Removes the specified value from this list.</summary>
        /// <returns>A new immutable list with the element removed, or this list if the element is not in this list.</returns>
        /// <param name="list">The list to search.</param>
        /// <param name="value">The value to remove.</param>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        public static IImmutableList<T> Remove<T>(this IImmutableList<T> list, T value) {
            Requires.NotNull<IImmutableList<T>>(list, "list");
            return list.Remove(value, EqualityComparer<T>.Default);
        }
        /// <summary>Removes the specified values from this list.</summary>
        /// <returns>A new immutable list with the elements removed.</returns>
        /// <param name="list">The list to search.</param>
        /// <param name="items">The items to remove if matches are found in this list.</param>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        public static IImmutableList<T> RemoveRange<T>(this IImmutableList<T> list, IEnumerable<T> items) {
            Requires.NotNull<IImmutableList<T>>(list, "list");
            return list.RemoveRange(items, EqualityComparer<T>.Default);
        }
        /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the list.</summary>
        /// <returns>The zero-based index of the first occurrence of item within the range of elements in the list that extends from index to the last element, if found; otherwise, –1.</returns>
        /// <param name="list">The list to search.</param>
        /// <param name="item">The object to locate in the list. The value can be null for reference types.</param>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        public static int IndexOf<T>(this IImmutableList<T> list, T item) {
            Requires.NotNull<IImmutableList<T>>(list, "list");
            return list.IndexOf(item, 0, list.Count, EqualityComparer<T>.Default);
        }
        /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the list.</summary>
        /// <returns>The zero-based index of the first occurrence of item within the range of elements in the immutable list that extends from index to the last element, if found; otherwise, –1.</returns>
        /// <param name="list">The list to search.</param>
        /// <param name="item">The object to locate in the Immutable list. The value can be null for reference types.</param>
        /// <param name="equalityComparer">The equality comparer to use in the search.</param>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        public static int IndexOf<T>(this IImmutableList<T> list, T item, IEqualityComparer<T> equalityComparer) {
            Requires.NotNull<IImmutableList<T>>(list, "list");
            return list.IndexOf(item, 0, list.Count, equalityComparer);
        }
        /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the immutable list that extends from the specified index to the last element.</summary>
        /// <returns>The zero-based index of the first occurrence of item within the range of elements in the Immutable list that extends from index to the last element, if found; otherwise, –1.</returns>
        /// <param name="list">The list to search.</param>
        /// <param name="item">The object to locate in the Immutable list. The value can be null for reference types.</param>
        /// <param name="startIndex">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        public static int IndexOf<T>(this IImmutableList<T> list, T item, int startIndex) {
            Requires.NotNull<IImmutableList<T>>(list, "list");
            return list.IndexOf(item, startIndex, list.Count - startIndex, EqualityComparer<T>.Default);
        }
        /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the immutable list that extends from the specified index to the last element.</summary>
        /// <returns>The zero-based index of the first occurrence of item within the range of elements in the Immutable list that extends from index to the last element, if found; otherwise, –1.</returns>
        /// <param name="list">The list to search.</param>
        /// <param name="item">The object to locate in the Immutable list. The value can be null for reference types.</param>
        /// <param name="startIndex">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        public static int IndexOf<T>(this IImmutableList<T> list, T item, int startIndex, int count) {
            Requires.NotNull<IImmutableList<T>>(list, "list");
            return list.IndexOf(item, startIndex, count, EqualityComparer<T>.Default);
        }
        /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the entire immutable list.</summary>
        /// <returns>The zero-based index of the last occurrence of item within the entire the Immutable list, if found; otherwise, –1.</returns>
        /// <param name="list">The list to search.</param>
        /// <param name="item">The object to locate in the Immutable list. The value can be null for reference types.</param>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        public static int LastIndexOf<T>(this IImmutableList<T> list, T item) {
            Requires.NotNull<IImmutableList<T>>(list, "list");
            if (list.Count == 0) {
                return -1;
            }
            return list.LastIndexOf(item, list.Count - 1, list.Count, EqualityComparer<T>.Default);
        }
        /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the entire immutable list.</summary>
        /// <returns>The zero-based index of the last occurrence of item within the entire the Immutable list, if found; otherwise, –1.</returns>
        /// <param name="list">The list to search.</param>
        /// <param name="item">The object to locate in the Immutable list. The value can be null for reference types.</param>
        /// <param name="equalityComparer">The equality comparer to use in the search.</param>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        public static int LastIndexOf<T>(this IImmutableList<T> list, T item, IEqualityComparer<T> equalityComparer) {
            Requires.NotNull<IImmutableList<T>>(list, "list");
            if (list.Count == 0) {
                return -1;
            }
            return list.LastIndexOf(item, list.Count - 1, list.Count, equalityComparer);
        }
        /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the immutable list that extends from the first element to the specified index.</summary>
        /// <returns>The zero-based index of the last occurrence of item within the range of elements in the Immutable list that extends from the first element to index, if found; otherwise, –1.</returns>
        /// <param name="list">The list to search.</param>
        /// <param name="item">The object to locate in the Immutable list. The value can be null for reference types.</param>
        /// <param name="startIndex">The zero-based starting index of the backward search.</param>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        public static int LastIndexOf<T>(this IImmutableList<T> list, T item, int startIndex) {
            Requires.NotNull<IImmutableList<T>>(list, "list");
            if (list.Count == 0 && startIndex == 0) {
                return -1;
            }
            return list.LastIndexOf(item, startIndex, startIndex + 1, EqualityComparer<T>.Default);
        }
        /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the immutable list that extends from the first element to the specified index.</summary>
        /// <returns>The zero-based index of the last occurrence of item within the range of elements in the Immutable list that extends from the first element to index, if found; otherwise, –1.</returns>
        /// <param name="list">The list to search.</param>
        /// <param name="item">The object to locate in the Immutable list. The value can be null for reference types.</param>
        /// <param name="startIndex">The zero-based starting index of the backward search.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        public static int LastIndexOf<T>(this IImmutableList<T> list, T item, int startIndex, int count) {
            Requires.NotNull<IImmutableList<T>>(list, "list");
            return list.LastIndexOf(item, startIndex, count, EqualityComparer<T>.Default);
        }
    }
}
