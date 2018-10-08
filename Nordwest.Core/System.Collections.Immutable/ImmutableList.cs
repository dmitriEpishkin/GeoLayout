using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Validation;
namespace System.Collections.Immutable {
    /// <summary>Represents an immutable list, which is a strongly typed list of objects that can be accessed by index.</summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    [DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(ImmutableList<>.DebuggerProxy))]
    public sealed class ImmutableList<T> : IImmutableList<T>, IList<T>, ICollection<T>, IList, ICollection, IOrderedCollection<T>, IImmutableListQueries<T>, IReadOnlyList<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable {
        /// <summary>Enumerates the contents of a binary tree.</summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable, ISecurePooledObjectUser {
            private static readonly SecureObjectPool<Stack<RefAsValueType<IBinaryTree<T>>>, ImmutableList<T>.Enumerator> EnumeratingStacks = new SecureObjectPool<Stack<RefAsValueType<IBinaryTree<T>>>, ImmutableList<T>.Enumerator>();
            private readonly ImmutableList<T>.Builder builder;
            private readonly Guid poolUserId;
            private readonly int startIndex;
            private readonly int count;
            private int remainingCount;
            private bool reversed;
            private IBinaryTree<T> root;
            private SecurePooledObject<Stack<RefAsValueType<IBinaryTree<T>>>> stack;
            private IBinaryTree<T> current;
            private int enumeratingBuilderVersion;
            Guid ISecurePooledObjectUser.PoolUserId {
                get {
                    return this.poolUserId;
                }
            }
            /// <summary>Gets the element at the current position of the enumerator.</summary>
            /// <returns>The element at the current position of the enumerator.</returns>
            public T Current {
                get {
                    this.ThrowIfDisposed();
                    if (this.current != null) {
                        return this.current.Value;
                    }
                    throw new InvalidOperationException();
                }
            }
            /// <summary>Gets the current element in the immutable list.</summary>
            /// <returns>The current element in the immutable list.</returns>
            object IEnumerator.Current {
                get {
                    return this.Current;
                }
            }
            internal Enumerator(IBinaryTree<T> root, ImmutableList<T>.Builder builder = null, int startIndex = -1, int count = -1, bool reversed = false) {
                Requires.NotNull<IBinaryTree<T>>(root, "root");
                Requires.Range(startIndex >= -1, "startIndex", null);
                Requires.Range(count >= -1, "count", null);
                Requires.Argument(reversed || count == -1 || ((startIndex == -1) ? 0 : startIndex) + count <= root.Count);
                Requires.Argument(!reversed || count == -1 || ((startIndex == -1) ? (root.Count - 1) : startIndex) - count + 1 >= 0);
                this.root = root;
                this.builder = builder;
                this.current = null;
                this.startIndex = ((startIndex >= 0) ? startIndex : (reversed ? (root.Count - 1) : 0));
                this.count = ((count == -1) ? root.Count : count);
                this.remainingCount = this.count;
                this.reversed = reversed;
                this.enumeratingBuilderVersion = ((builder != null) ? builder.Version : -1);
                this.poolUserId = Guid.NewGuid();
                if (this.count > 0) {
                    this.stack = null;
                    if (!ImmutableList<T>.Enumerator.EnumeratingStacks.TryTake(this, out this.stack)) {
                        this.stack = ImmutableList<T>.Enumerator.EnumeratingStacks.PrepNew(this, new Stack<RefAsValueType<IBinaryTree<T>>>(root.Height));
                    }
                } else {
                    this.stack = null;
                }
                this.Reset();
            }
            /// <summary>Releases the resources used by the current instance of the <see cref="T:System.Collections.Immutable.ImmutableList`1.Enumerator" /> class.</summary>
            public void Dispose() {
                this.root = null;
                this.current = null;
                if (this.stack != null && this.stack.Owner == this.poolUserId) {
                    using (SecurePooledObject<Stack<RefAsValueType<IBinaryTree<T>>>>.SecurePooledObjectUser securePooledObjectUser = this.stack.Use<ImmutableList<T>.Enumerator>(this)) {
                        securePooledObjectUser.Value.Clear();
                    }
                    ImmutableList<T>.Enumerator.EnumeratingStacks.TryAdd(this, this.stack);
                }
                this.stack = null;
            }
            /// <summary>Advances enumeration to the next element of the immutable list.</summary>
            /// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the list.</returns>
            public bool MoveNext() {
                this.ThrowIfDisposed();
                this.ThrowIfChanged();
                if (this.stack != null) {
                    using (SecurePooledObject<Stack<RefAsValueType<IBinaryTree<T>>>>.SecurePooledObjectUser securePooledObjectUser = this.stack.Use<ImmutableList<T>.Enumerator>(this)) {
                        if (this.remainingCount > 0 && securePooledObjectUser.Value.Count > 0) {
                            IBinaryTree<T> value = securePooledObjectUser.Value.Pop().Value;
                            this.current = value;
                            this.PushNext(this.NextBranch(value));
                            this.remainingCount--;
                            return true;
                        }
                    }
                }
                this.current = null;
                return false;
            }
            /// <summary>Sets the enumerator to its initial position, which is before the first element in the immutable list.</summary>
            public void Reset() {
                this.ThrowIfDisposed();
                this.enumeratingBuilderVersion = ((this.builder != null) ? this.builder.Version : -1);
                this.remainingCount = this.count;
                if (this.stack != null) {
                    using (SecurePooledObject<Stack<RefAsValueType<IBinaryTree<T>>>>.SecurePooledObjectUser securePooledObjectUser = this.stack.Use<ImmutableList<T>.Enumerator>(this)) {
                        securePooledObjectUser.Value.Clear();
                        IBinaryTree<T> binaryTree = this.root;
                        int num = this.reversed ? (this.root.Count - this.startIndex - 1) : this.startIndex;
                        while (!binaryTree.IsEmpty && num != this.PreviousBranch(binaryTree).Count) {
                            if (num < this.PreviousBranch(binaryTree).Count) {
                                securePooledObjectUser.Value.Push(new RefAsValueType<IBinaryTree<T>>(binaryTree));
                                binaryTree = this.PreviousBranch(binaryTree);
                            } else {
                                num -= this.PreviousBranch(binaryTree).Count + 1;
                                binaryTree = this.NextBranch(binaryTree);
                            }
                        }
                        if (!binaryTree.IsEmpty) {
                            securePooledObjectUser.Value.Push(new RefAsValueType<IBinaryTree<T>>(binaryTree));
                        }
                    }
                }
            }
            private IBinaryTree<T> NextBranch(IBinaryTree<T> node) {
                if (!this.reversed) {
                    return node.Right;
                }
                return node.Left;
            }
            private IBinaryTree<T> PreviousBranch(IBinaryTree<T> node) {
                if (!this.reversed) {
                    return node.Left;
                }
                return node.Right;
            }
            private void ThrowIfDisposed() {
                if (this.root == null) {
                    throw new ObjectDisposedException(base.GetType().FullName);
                }
                if (this.stack != null) {
                    this.stack.ThrowDisposedIfNotOwned<ImmutableList<T>.Enumerator>(this);
                }
            }
            private void ThrowIfChanged() {
                if (this.builder != null && this.builder.Version != this.enumeratingBuilderVersion) {
                    throw new InvalidOperationException(@"Collection was modified; enumeration operation may not execute.");
                }
            }

            private void PushNext(IBinaryTree<T> node) {
                Requires.NotNull<IBinaryTree<T>>(node, "node");
                if (!node.IsEmpty) {
                    using (SecurePooledObject<Stack<RefAsValueType<IBinaryTree<T>>>>.SecurePooledObjectUser securePooledObjectUser = this.stack.Use<ImmutableList<T>.Enumerator>(this)) {
                        while (!node.IsEmpty) {
                            securePooledObjectUser.Value.Push(new RefAsValueType<IBinaryTree<T>>(node));
                            node = this.PreviousBranch(node);
                        }
                    }
                }
            }
        }
        [DebuggerDisplay("{key}")]
        internal sealed class Node : IBinaryTree<T>, IEnumerable<T>, IEnumerable {
            internal static readonly ImmutableList<T>.Node EmptyNode = new ImmutableList<T>.Node();
            private T key;
            private bool frozen;
            private int height;
            private int count;
            private ImmutableList<T>.Node left;
            private ImmutableList<T>.Node right;
            public bool IsEmpty {
                get {
                    return this.left == null;
                }
            }
            int IBinaryTree<T>.Height {
                get {
                    return this.height;
                }
            }
            IBinaryTree<T> IBinaryTree<T>.Left {
                get {
                    return this.left;
                }
            }
            IBinaryTree<T> IBinaryTree<T>.Right {
                get {
                    return this.right;
                }
            }
            T IBinaryTree<T>.Value {
                get {
                    return this.key;
                }
            }
            public int Count {
                get {
                    return this.count;
                }
            }
            internal T Key {
                get {
                    return this.key;
                }
            }
            internal T this[int index] {
                get {
                    Requires.Range(index >= 0 && index < this.Count, "index", null);
                    if (index < this.left.count) {
                        return this.left[index];
                    }
                    if (index > this.left.count) {
                        return this.right[index - this.left.count - 1];
                    }
                    return this.key;
                }
            }
            private Node() {
                this.frozen = true;
            }
            private Node(T key, ImmutableList<T>.Node left, ImmutableList<T>.Node right, bool frozen = false) {
                Requires.NotNull<ImmutableList<T>.Node>(left, "left");
                Requires.NotNull<ImmutableList<T>.Node>(right, "right");
                this.key = key;
                this.left = left;
                this.right = right;
                this.height = 1 + Math.Max(left.height, right.height);
                this.count = 1 + left.count + right.count;
                this.frozen = frozen;
            }
            public ImmutableList<T>.Enumerator GetEnumerator() {
                return new ImmutableList<T>.Enumerator(this, null, -1, -1, false);
            }
            [ExcludeFromCodeCoverage]
            IEnumerator<T> IEnumerable<T>.GetEnumerator() {
                return this.GetEnumerator();
            }
            [ExcludeFromCodeCoverage]
            IEnumerator IEnumerable.GetEnumerator() {
                return this.GetEnumerator();
            }
            internal ImmutableList<T>.Enumerator GetEnumerator(ImmutableList<T>.Builder builder) {
                return new ImmutableList<T>.Enumerator(this, builder, -1, -1, false);
            }
            internal static ImmutableList<T>.Node NodeTreeFromList(IOrderedCollection<T> items, int start, int length) {
                Requires.NotNull<IOrderedCollection<T>>(items, "items");
                Requires.Range(start >= 0, "start", null);
                Requires.Range(length >= 0, "length", null);
                if (length == 0) {
                    return ImmutableList<T>.Node.EmptyNode;
                }
                int num = (length - 1) / 2;
                int num2 = length - 1 - num;
                ImmutableList<T>.Node node = ImmutableList<T>.Node.NodeTreeFromList(items, start, num2);
                ImmutableList<T>.Node node2 = ImmutableList<T>.Node.NodeTreeFromList(items, start + num2 + 1, num);
                return new ImmutableList<T>.Node(items[start + num2], node, node2, true);
            }
            internal ImmutableList<T>.Node Add(T key) {
                return this.Insert(this.count, key);
            }
            internal ImmutableList<T>.Node Insert(int index, T key) {
                Requires.Range(index >= 0 && index <= this.Count, "index", null);
                if (this.IsEmpty) {
                    return new ImmutableList<T>.Node(key, this, this, false);
                }
                ImmutableList<T>.Node tree;
                if (index <= this.left.count) {
                    ImmutableList<T>.Node node = this.left.Insert(index, key);
                    tree = this.Mutate(node, null);
                } else {
                    ImmutableList<T>.Node node2 = this.right.Insert(index - this.left.count - 1, key);
                    tree = this.Mutate(null, node2);
                }
                return ImmutableList<T>.Node.MakeBalanced(tree);
            }
            internal ImmutableList<T>.Node RemoveAt(int index) {
                Requires.Range(index >= 0 && index < this.Count, "index", null);
                ImmutableList<T>.Node node;
                if (index == this.left.count) {
                    if (this.right.IsEmpty && this.left.IsEmpty) {
                        node = ImmutableList<T>.Node.EmptyNode;
                    } else {
                        if (this.right.IsEmpty && !this.left.IsEmpty) {
                            node = this.left;
                        } else {
                            if (!this.right.IsEmpty && this.left.IsEmpty) {
                                node = this.right;
                            } else {
                                ImmutableList<T>.Node node2 = this.right;
                                while (!node2.left.IsEmpty) {
                                    node2 = node2.left;
                                }
                                ImmutableList<T>.Node node3 = this.right.RemoveAt(0);
                                node = node2.Mutate(this.left, node3);
                            }
                        }
                    }
                } else {
                    if (index < this.left.count) {
                        ImmutableList<T>.Node node4 = this.left.RemoveAt(index);
                        node = this.Mutate(node4, null);
                    } else {
                        ImmutableList<T>.Node node5 = this.right.RemoveAt(index - this.left.count - 1);
                        node = this.Mutate(null, node5);
                    }
                }
                if (!node.IsEmpty) {
                    return ImmutableList<T>.Node.MakeBalanced(node);
                }
                return node;
            }
            internal ImmutableList<T>.Node RemoveAll(Predicate<T> match) {
                Requires.NotNull<Predicate<T>>(match, "match");
                ImmutableList<T>.Node node = this;
                int num = 0;
                foreach (T current in this) {
                    if (match(current)) {
                        node = node.RemoveAt(num);
                    } else {
                        num++;
                    }
                }
                return node;
            }
            internal ImmutableList<T>.Node ReplaceAt(int index, T value) {
                Requires.Range(index >= 0 && index < this.Count, "index", null);
                ImmutableList<T>.Node result;
                if (index == this.left.count) {
                    result = this.Mutate(value);
                } else {
                    if (index < this.left.count) {
                        ImmutableList<T>.Node node = this.left.ReplaceAt(index, value);
                        result = this.Mutate(node, null);
                    } else {
                        ImmutableList<T>.Node node2 = this.right.ReplaceAt(index - this.left.count - 1, value);
                        result = this.Mutate(null, node2);
                    }
                }
                return result;
            }
            internal ImmutableList<T>.Node Reverse() {
                return this.Reverse(0, this.Count);
            }
            internal ImmutableList<T>.Node Reverse(int index, int count) {
                Requires.Range(index >= 0, "index", null);
                Requires.Range(count >= 0, "count", null);
                Requires.Range(index + count <= this.Count, "index", null);
                ImmutableList<T>.Node node = this;
                int i = index;
                int num = index + count - 1;
                while (i < num) {
                    T value = node[i];
                    T value2 = node[num];
                    node = node.ReplaceAt(num, value).ReplaceAt(i, value2);
                    i++;
                    num--;
                }
                return node;
            }
            internal ImmutableList<T>.Node Sort() {
                return this.Sort(Comparer<T>.Default);
            }
            internal ImmutableList<T>.Node Sort(Comparison<T> comparison) {
                Requires.NotNull<Comparison<T>>(comparison, "comparison");
                T[] array = new T[this.Count];
                this.CopyTo(array);
                Array.Sort<T>(array, comparison);
                return ImmutableList<T>.Node.NodeTreeFromList(array.AsOrderedCollection<T>(), 0, this.Count);
            }
            internal ImmutableList<T>.Node Sort(IComparer<T> comparer) {
                Requires.NotNull<IComparer<T>>(comparer, "comparer");
                return this.Sort(0, this.Count, comparer);
            }
            internal ImmutableList<T>.Node Sort(int index, int count, IComparer<T> comparer) {
                Requires.Range(index >= 0, "index", null);
                Requires.Range(count >= 0, "count", null);
                Requires.Argument(index + count <= this.Count);
                Requires.NotNull<IComparer<T>>(comparer, "comparer");
                T[] array = new T[this.Count];
                this.CopyTo(array);
                Array.Sort<T>(array, index, count, comparer);
                return ImmutableList<T>.Node.NodeTreeFromList(array.AsOrderedCollection<T>(), 0, this.Count);
            }
            internal int BinarySearch(int index, int count, T item, IComparer<T> comparer) {
                Requires.Range(index >= 0, "index", null);
                Requires.Range(count >= 0, "count", null);
                comparer = (comparer ?? Comparer<T>.Default);
                if (this.IsEmpty || count <= 0) {
                    return ~index;
                }
                int num = this.left.Count;
                if (index + count <= num) {
                    return this.left.BinarySearch(index, count, item, comparer);
                }
                if (index > num) {
                    int num2 = this.right.BinarySearch(index - num - 1, count, item, comparer);
                    int num3 = num + 1;
                    if (num2 >= 0) {
                        return num2 + num3;
                    }
                    return num2 - num3;
                } else {
                    int num4 = comparer.Compare(item, this.key);
                    if (num4 == 0) {
                        return num;
                    }
                    if (num4 > 0) {
                        int num5 = count - (num - index) - 1;
                        int num6 = (num5 < 0) ? -1 : this.right.BinarySearch(0, num5, item, comparer);
                        int num7 = num + 1;
                        if (num6 >= 0) {
                            return num6 + num7;
                        }
                        return num6 - num7;
                    } else {
                        if (index == num) {
                            return ~index;
                        }
                        return this.left.BinarySearch(index, count, item, comparer);
                    }
                }
            }
            internal int IndexOf(T item, IEqualityComparer<T> equalityComparer) {
                return this.IndexOf(item, 0, this.Count, equalityComparer);
            }
            internal int IndexOf(T item, int index, int count, IEqualityComparer<T> equalityComparer) {
                Requires.Range(index >= 0, "index", null);
                Requires.Range(count >= 0, "count", null);
                Requires.Range(count <= this.Count, "count", null);
                Requires.Range(index + count <= this.Count, "count", null);
                Requires.NotNull<IEqualityComparer<T>>(equalityComparer, "equalityComparer");
                using (ImmutableList<T>.Enumerator enumerator = new ImmutableList<T>.Enumerator(this, null, index, count, false)) {
                    while (enumerator.MoveNext()) {
                        if (equalityComparer.Equals(item, enumerator.Current)) {
                            return index;
                        }
                        index++;
                    }
                }
                return -1;
            }
            internal int LastIndexOf(T item, int index, int count, IEqualityComparer<T> equalityComparer) {
                Requires.NotNull<IEqualityComparer<T>>(equalityComparer, "ValueComparer");
                Requires.Range(index >= 0, "index", null);
                Requires.Range(count >= 0 && count <= this.Count, "count", null);
                Requires.Argument(index - count + 1 >= 0);
                using (ImmutableList<T>.Enumerator enumerator = new ImmutableList<T>.Enumerator(this, null, index, count, true)) {
                    while (enumerator.MoveNext()) {
                        if (equalityComparer.Equals(item, enumerator.Current)) {
                            return index;
                        }
                        index--;
                    }
                }
                return -1;
            }
            internal void CopyTo(T[] array) {
                Requires.NotNull<T[]>(array, "array");
                Requires.Argument(array.Length >= this.Count);
                int num = 0;
                foreach (T current in this) {
                    array[num++] = current;
                }
            }
            internal void CopyTo(T[] array, int arrayIndex) {
                Requires.NotNull<T[]>(array, "array");
                Requires.Range(arrayIndex >= 0, "arrayIndex", null);
                Requires.Range(arrayIndex <= array.Length, "arrayIndex", null);
                Requires.Argument(arrayIndex + this.Count <= array.Length);
                foreach (T current in this) {
                    array[arrayIndex++] = current;
                }
            }
            internal void CopyTo(int index, T[] array, int arrayIndex, int count) {
                Requires.NotNull<T[]>(array, "array");
                Requires.Range(index >= 0, "index", null);
                Requires.Range(count >= 0, "count", null);
                Requires.Range(index + count <= this.Count, "count", null);
                Requires.Range(arrayIndex >= 0, "arrayIndex", null);
                Requires.Range(arrayIndex + count <= array.Length, "arrayIndex", null);
                using (ImmutableList<T>.Enumerator enumerator = new ImmutableList<T>.Enumerator(this, null, index, count, false)) {
                    while (enumerator.MoveNext()) {
                        array[arrayIndex++] = enumerator.Current;
                    }
                }
            }
            internal void CopyTo(Array array, int arrayIndex) {
                Requires.NotNull<Array>(array, "array");
                Requires.Range(arrayIndex >= 0, "arrayIndex", null);
                Requires.Range(array.Length >= arrayIndex + this.Count, "arrayIndex", null);
                foreach (T current in this) {
                    array.SetValue(current, new int[]
					{
						arrayIndex++
					});
                }
            }
            internal ImmutableList<TOutput>.Node ConvertAll<TOutput>(Func<T, TOutput> converter) {
                ImmutableList<TOutput>.Node node = ImmutableList<TOutput>.Node.EmptyNode;
                foreach (T current in this) {
                    node = node.Add(converter(current));
                }
                return node;
            }
            internal bool TrueForAll(Predicate<T> match) {
                foreach (T current in this) {
                    if (!match(current)) {
                        return false;
                    }
                }
                return true;
            }
            internal bool Exists(Predicate<T> match) {
                Requires.NotNull<Predicate<T>>(match, "match");
                foreach (T current in this) {
                    if (match(current)) {
                        return true;
                    }
                }
                return false;
            }
            internal T Find(Predicate<T> match) {
                Requires.NotNull<Predicate<T>>(match, "match");
                foreach (T current in this) {
                    if (match(current)) {
                        return current;
                    }
                }
                return default(T);
            }
            internal ImmutableList<T> FindAll(Predicate<T> match) {
                Requires.NotNull<Predicate<T>>(match, "match");
                ImmutableList<T>.Builder builder = ImmutableList<T>.Empty.ToBuilder();
                foreach (T current in this) {
                    if (match(current)) {
                        builder.Add(current);
                    }
                }
                return builder.ToImmutable();
            }
            internal int FindIndex(Predicate<T> match) {
                Requires.NotNull<Predicate<T>>(match, "match");
                return this.FindIndex(0, this.count, match);
            }
            internal int FindIndex(int startIndex, Predicate<T> match) {
                Requires.Range(startIndex >= 0, "startIndex", null);
                Requires.Range(startIndex <= this.Count, "startIndex", null);
                Requires.NotNull<Predicate<T>>(match, "match");
                return this.FindIndex(startIndex, this.Count - startIndex, match);
            }
            internal int FindIndex(int startIndex, int count, Predicate<T> match) {
                Requires.Range(startIndex >= 0, "startIndex", null);
                Requires.Range(count >= 0, "count", null);
                Requires.Argument(startIndex + count <= this.Count);
                Requires.NotNull<Predicate<T>>(match, "match");
                using (ImmutableList<T>.Enumerator enumerator = new ImmutableList<T>.Enumerator(this, null, startIndex, count, false)) {
                    int num = startIndex;
                    while (enumerator.MoveNext()) {
                        if (match(enumerator.Current)) {
                            return num;
                        }
                        num++;
                    }
                }
                return -1;
            }
            internal T FindLast(Predicate<T> match) {
                Requires.NotNull<Predicate<T>>(match, "match");
                using (ImmutableList<T>.Enumerator enumerator = new ImmutableList<T>.Enumerator(this, null, -1, -1, true)) {
                    while (enumerator.MoveNext()) {
                        if (match(enumerator.Current)) {
                            return enumerator.Current;
                        }
                    }
                }
                return default(T);
            }
            internal int FindLastIndex(Predicate<T> match) {
                Requires.NotNull<Predicate<T>>(match, "match");
                if (this.IsEmpty) {
                    return -1;
                }
                return this.FindLastIndex(this.Count - 1, this.Count, match);
            }
            internal int FindLastIndex(int startIndex, Predicate<T> match) {
                Requires.NotNull<Predicate<T>>(match, "match");
                Requires.Range(startIndex >= 0, "startIndex", null);
                Requires.Range(startIndex == 0 || startIndex < this.Count, "startIndex", null);
                if (this.IsEmpty) {
                    return -1;
                }
                return this.FindLastIndex(startIndex, startIndex + 1, match);
            }
            internal int FindLastIndex(int startIndex, int count, Predicate<T> match) {
                Requires.NotNull<Predicate<T>>(match, "match");
                Requires.Range(startIndex >= 0, "startIndex", null);
                Requires.Range(count <= this.Count, "count", null);
                Requires.Argument(startIndex - count + 1 >= 0);
                using (ImmutableList<T>.Enumerator enumerator = new ImmutableList<T>.Enumerator(this, null, startIndex, count, true)) {
                    int num = startIndex;
                    while (enumerator.MoveNext()) {
                        if (match(enumerator.Current)) {
                            return num;
                        }
                        num--;
                    }
                }
                return -1;
            }
            internal void Freeze() {
                if (!this.frozen) {
                    this.left.Freeze();
                    this.right.Freeze();
                    this.frozen = true;
                }
            }
            private static ImmutableList<T>.Node RotateLeft(ImmutableList<T>.Node tree) {
                Requires.NotNull<ImmutableList<T>.Node>(tree, "tree");
                if (tree.right.IsEmpty) {
                    return tree;
                }
                ImmutableList<T>.Node node = tree.right;
                return node.Mutate(tree.Mutate(null, node.left), null);
            }
            private static ImmutableList<T>.Node RotateRight(ImmutableList<T>.Node tree) {
                Requires.NotNull<ImmutableList<T>.Node>(tree, "tree");
                if (tree.left.IsEmpty) {
                    return tree;
                }
                ImmutableList<T>.Node node = tree.left;
                return node.Mutate(null, tree.Mutate(node.right, null));
            }
            private static ImmutableList<T>.Node DoubleLeft(ImmutableList<T>.Node tree) {
                Requires.NotNull<ImmutableList<T>.Node>(tree, "tree");
                if (tree.right.IsEmpty) {
                    return tree;
                }
                ImmutableList<T>.Node tree2 = tree.Mutate(null, ImmutableList<T>.Node.RotateRight(tree.right));
                return ImmutableList<T>.Node.RotateLeft(tree2);
            }
            private static ImmutableList<T>.Node DoubleRight(ImmutableList<T>.Node tree) {
                Requires.NotNull<ImmutableList<T>.Node>(tree, "tree");
                if (tree.left.IsEmpty) {
                    return tree;
                }
                ImmutableList<T>.Node tree2 = tree.Mutate(ImmutableList<T>.Node.RotateLeft(tree.left), null);
                return ImmutableList<T>.Node.RotateRight(tree2);
            }
            private static int Balance(ImmutableList<T>.Node tree) {
                Requires.NotNull<ImmutableList<T>.Node>(tree, "tree");
                return tree.right.height - tree.left.height;
            }
            private static bool IsRightHeavy(ImmutableList<T>.Node tree) {
                Requires.NotNull<ImmutableList<T>.Node>(tree, "tree");
                return ImmutableList<T>.Node.Balance(tree) >= 2;
            }
            private static bool IsLeftHeavy(ImmutableList<T>.Node tree) {
                Requires.NotNull<ImmutableList<T>.Node>(tree, "tree");
                return ImmutableList<T>.Node.Balance(tree) <= -2;
            }
            private static ImmutableList<T>.Node MakeBalanced(ImmutableList<T>.Node tree) {
                Requires.NotNull<ImmutableList<T>.Node>(tree, "tree");
                if (ImmutableList<T>.Node.IsRightHeavy(tree)) {
                    if (!ImmutableList<T>.Node.IsLeftHeavy(tree.right)) {
                        return ImmutableList<T>.Node.RotateLeft(tree);
                    }
                    return ImmutableList<T>.Node.DoubleLeft(tree);
                } else {
                    if (!ImmutableList<T>.Node.IsLeftHeavy(tree)) {
                        return tree;
                    }
                    if (!ImmutableList<T>.Node.IsRightHeavy(tree.left)) {
                        return ImmutableList<T>.Node.RotateRight(tree);
                    }
                    return ImmutableList<T>.Node.DoubleRight(tree);
                }
            }
            private ImmutableList<T>.Node Mutate(ImmutableList<T>.Node left = null, ImmutableList<T>.Node right = null) {
                if (this.frozen) {
                    return new ImmutableList<T>.Node(this.key, left ?? this.left, right ?? this.right, false);
                }
                if (left != null) {
                    this.left = left;
                }
                if (right != null) {
                    this.right = right;
                }
                this.height = 1 + Math.Max(this.left.height, this.right.height);
                this.count = 1 + this.left.count + this.right.count;
                return this;
            }
            private ImmutableList<T>.Node Mutate(T value) {
                if (this.frozen) {
                    return new ImmutableList<T>.Node(value, this.left, this.right, false);
                }
                this.key = value;
                return this;
            }
        }
        [ExcludeFromCodeCoverage]
        private class DebuggerProxy {
            private readonly ImmutableList<T>.Node list;
            private T[] cachedContents;
            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public T[] Contents {
                get {
                    if (this.cachedContents == null) {
                        this.cachedContents = this.list.ToArray(this.list.Count);
                    }
                    return this.cachedContents;
                }
            }
            public DebuggerProxy(ImmutableList<T> list) {
                Requires.NotNull<ImmutableList<T>>(list, "list");
                this.list = list.root;
            }
            public DebuggerProxy(ImmutableList<T>.Builder builder) {
                Requires.NotNull<ImmutableList<T>.Builder>(builder, "builder");
                this.list = builder.Root;
            }
        }
        /// <summary>Represents a list that mutates with little or no memory allocations and that can produce or build on immutable list instances very efficiently.</summary>
        [DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(ImmutableList<>.DebuggerProxy))]
        public sealed class Builder : IList<T>, ICollection<T>, IList, ICollection, IOrderedCollection<T>, IImmutableListQueries<T>, IReadOnlyList<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable {
            private ImmutableList<T>.Node root = ImmutableList<T>.Node.EmptyNode;
            private ImmutableList<T> immutable;
            private int version;
            private object syncRoot;
            /// <summary>Gets the number of elements in this immutable list.</summary>
            /// <returns>The number of elements in this list.</returns>
            public int Count {
                get {
                    return this.Root.Count;
                }
            }
            bool ICollection<T>.IsReadOnly {
                get {
                    return false;
                }
            }
            internal int Version {
                get {
                    return this.version;
                }
            }
            internal ImmutableList<T>.Node Root {
                get {
                    return this.root;
                }
                private set {
                    this.version++;
                    if (this.root != value) {
                        this.root = value;
                        this.immutable = null;
                    }
                }
            }
            /// <summary>Gets or sets the value for a given index in the list.</summary>
            /// <returns>The value at the specified index.</returns>
            /// <param name="index">The index of the item to get or set.</param>
            public T this[int index] {
                get {
                    return this.Root[index];
                }
                set {
                    this.Root = this.Root.ReplaceAt(index, value);
                }
            }
            T IReadOnlyList<T>.this[int index] { // MIKANT :: CONTRACT STUB
                get { return this[index]; }
            }
            T IOrderedCollection<T>.this[int index] {
                get {
                    return this[index];
                }
            }
            /// <summary>Gets a value indicating whether the list has a fixed size.</summary>
            /// <returns>true if the list has a fixed size; otherwise, false.</returns>
            bool IList.IsFixedSize {
                get {
                    return false;
                }
            }
            /// <summary>Gets a value indicating whether the list is read-only.</summary>
            /// <returns>Always false.</returns>
            bool IList.IsReadOnly {
                get {
                    return false;
                }
            }
            /// <summary>Gets or sets the <see cref="T:System.Object" /> at the specified index.</summary>
            /// <returns>The object at the specified index.</returns>
            /// <param name="index">The index of the item to get or set.</param>
            object IList.this[int index] {
                get {
                    return this[index];
                }
                set {
                    this[index] = (T)((object)value);
                }
            }
            /// <summary>Gets a value indicating whether access to the list is synchronized (thread safe).</summary>
            /// <returns>true if access to the list is synchronized (thread safe); otherwise, false.</returns>
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool ICollection.IsSynchronized {
                get {
                    return false;
                }
            }
            /// <summary>Gets an object that can be used to synchronize access to the list.</summary>
            /// <returns>An object that can be used to synchronize access to the list.</returns>
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            object ICollection.SyncRoot {
                get {
                    if (this.syncRoot == null) {
                        Interlocked.CompareExchange<object>(ref this.syncRoot, new object(), null);
                    }
                    return this.syncRoot;
                }
            }
            internal Builder(ImmutableList<T> list) {
                Requires.NotNull<ImmutableList<T>>(list, "list");
                this.root = list.root;
                this.immutable = list;
            }
            /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the immutable list.</summary>
            /// <returns>The zero-based index of the first occurrence of <paramref name="item" /> within the range of elements in the immutable list, if found; otherwise, –1.</returns>
            /// <param name="item">The object to locate in the immutable list. The value can be null for reference types.</param>
            public int IndexOf(T item) {
                return this.Root.IndexOf(item, EqualityComparer<T>.Default);
            }
            /// <summary>Inserts an item to the immutable list at the specified index.</summary>
            /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
            /// <param name="item">The object to insert into the immutable list.</param>
            public void Insert(int index, T item) {
                this.Root = this.Root.Insert(index, item);
            }
            /// <summary>Removes the item at the specified index of the immutable list.</summary>
            /// <param name="index">The zero-based index of the item to remove from the list.</param>
            public void RemoveAt(int index) {
                this.Root = this.Root.RemoveAt(index);
            }
            void IList.RemoveAt(int index) { // MIKANT :: CONTRACT STUB
                RemoveAt(index);
            }
            /// <summary>Adds an item to the immutable list.</summary>
            /// <param name="item">The item to add to the list.</param>
            public void Add(T item) {
                this.Root = this.Root.Add(item);
            }
            /// <summary>Removes all items from the immutable list.</summary>
            public void Clear() {
                this.Root = ImmutableList<T>.Node.EmptyNode;
            }
            /// <summary>Determines whether the immutable list contains a specific value.</summary>
            /// <returns>true if item is found in the list; otherwise, false.</returns>
            /// <param name="item">The object to locate in the list.</param>
            public bool Contains(T item) {
                return this.IndexOf(item) >= 0;
            }
            /// <summary>Removes the first occurrence of a specific object from the immutable list.</summary>
            /// <returns>true if item was successfully removed from the list; otherwise, false. This method also returns false if item is not found in the list.</returns>
            /// <param name="item">The object to remove from the list.</param>
            public bool Remove(T item) {
                int num = this.IndexOf(item);
                if (num < 0) {
                    return false;
                }
                this.Root = this.Root.RemoveAt(num);
                return true;
            }
            /// <summary>Returns an enumerator that iterates through the collection.</summary>
            /// <returns>An enumerator that can be used to iterate through the list.</returns>
            public ImmutableList<T>.Enumerator GetEnumerator() {
                return this.Root.GetEnumerator(this);
            }
            IEnumerator<T> IEnumerable<T>.GetEnumerator() {
                return this.GetEnumerator();
            }
            /// <summary>Returns an enumerator that iterates through the collection.</summary>
            /// <returns>An enumerator that can be used to iterate through the collection.</returns>
            IEnumerator IEnumerable.GetEnumerator() {
                return this.GetEnumerator();
            }
            /// <summary>Performs the specified action on each element of the list.</summary>
            /// <param name="action">The delegate to perform on each element of the list.</param>
            public void ForEach(Action<T> action) {
                Requires.NotNull<Action<T>>(action, "action");
                foreach (T current in this) {
                    action(current);
                }
            }
            /// <summary>Copies the entire immutable list to a compatible one-dimensional array, starting at the beginning of the target array.</summary>
            /// <param name="array">The one-dimensional array that is the destination of the elements copied from the immutable list. The array must have zero-based indexing.</param>
            public void CopyTo(T[] array) {
                Requires.NotNull<T[]>(array, "array");
                Requires.Range(array.Length >= this.Count, "array", null);
                this.root.CopyTo(array);
            }
            /// <summary>Copies the entire immutable list to a compatible one-dimensional array, starting at the specified index of the target array.</summary>
            /// <param name="array">The one-dimensional array that is the destination of the elements copied from the immutable list. The array must have zero-based indexing.</param>
            /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
            public void CopyTo(T[] array, int arrayIndex) {
                Requires.NotNull<T[]>(array, "array");
                Requires.Range(array.Length >= arrayIndex + this.Count, "arrayIndex", null);
                this.root.CopyTo(array, arrayIndex);
            }
            void ICollection<T>.CopyTo(T[] array, int arrayIndex) { // MIKANT :: CONTRACT STUB
                CopyTo(array, arrayIndex);
            }
            /// <summary>Copies the entire immutable list to a compatible one-dimensional array, starting at the specified index of the target array.</summary>
            /// <param name="index">The zero-based index in the source immutable list at which copying begins.</param>
            /// <param name="array">The one-dimensional array that is the destination of the elements copied from the immutable list. The array must have zero-based indexing.</param>
            /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
            /// <param name="count">The number of elements to copy.</param>
            public void CopyTo(int index, T[] array, int arrayIndex, int count) {
                this.root.CopyTo(index, array, arrayIndex, count);
            }
            /// <summary>Creates a shallow copy of a range of elements in the source immutable list.</summary>
            /// <returns>A shallow copy of a range of elements in the source immutable list.</returns>
            /// <param name="index">The zero-based index at which the range starts.</param>
            /// <param name="count">The number of elements in the range.</param>
            public ImmutableList<T> GetRange(int index, int count) {
                Requires.Range(index >= 0, "index", null);
                Requires.Range(count >= 0, "count", null);
                Requires.Range(index + count <= this.Count, "count", null);
                return ImmutableList<T>.WrapNode(ImmutableList<T>.Node.NodeTreeFromList(this, index, count));
            }
            /// <summary>Creates a new immutable list from the list represented by this builder by using the converter function.</summary>
            /// <returns>A new immutable list from the list represented by this builder.</returns>
            /// <param name="converter">The converter function.</param>
            /// <typeparam name="TOutput">The type of the output of the delegate converter function.</typeparam>
            public ImmutableList<TOutput> ConvertAll<TOutput>(Func<T, TOutput> converter) {
                Requires.NotNull<Func<T, TOutput>>(converter, "converter");
                return ImmutableList<TOutput>.WrapNode(this.root.ConvertAll<TOutput>(converter));
            }
            /// <summary>Determines whether the immutable list contains elements that match the conditions defined by the specified predicate.</summary>
            /// <returns>true if the immutable list contains one or more elements that match the conditions defined by the specified predicate; otherwise, false.</returns>
            /// <param name="match">The delegate that defines the conditions of the elements to search for.</param>
            public bool Exists(Predicate<T> match) {
                Requires.NotNull<Predicate<T>>(match, "match");
                return this.root.Exists(match);
            }
            /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the first occurrence within the entire immutable list.</summary>
            /// <returns>The first element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type <paramref name="T" />.</returns>
            /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
            public T Find(Predicate<T> match) {
                Requires.NotNull<Predicate<T>>(match, "match");
                return this.root.Find(match);
            }
            /// <summary>Retrieves all the elements that match the conditions defined by the specified predicate.</summary>
            /// <returns>An immutable list containing all the elements that match the conditions defined by the specified predicate, if found; otherwise, an empty immutable list.</returns>
            /// <param name="match">The delegate that defines the conditions of the elements to search for.</param>
            public ImmutableList<T> FindAll(Predicate<T> match) {
                Requires.NotNull<Predicate<T>>(match, "match");
                return this.root.FindAll(match);
            }
            /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the entire immutable list.</summary>
            /// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
            /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
            public int FindIndex(Predicate<T> match) {
                Requires.NotNull<Predicate<T>>(match, "match");
                return this.root.FindIndex(match);
            }
            /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the immutable list that extends from the specified index to the last element.</summary>
            /// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
            /// <param name="startIndex">The zero-based starting index of the search.</param>
            /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
            public int FindIndex(int startIndex, Predicate<T> match) {
                Requires.NotNull<Predicate<T>>(match, "match");
                Requires.Range(startIndex >= 0, "startIndex", null);
                Requires.Range(startIndex <= this.Count, "startIndex", null);
                return this.root.FindIndex(startIndex, match);
            }
            /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the immutable list that starts at the specified index and contains the specified number of elements.</summary>
            /// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
            /// <param name="startIndex">The zero-based starting index of the search.</param>
            /// <param name="count">The number of elements in the section to search.</param>
            /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
            public int FindIndex(int startIndex, int count, Predicate<T> match) {
                Requires.NotNull<Predicate<T>>(match, "match");
                Requires.Range(startIndex >= 0, "startIndex", null);
                Requires.Range(count >= 0, "count", null);
                Requires.Range(startIndex + count <= this.Count, "count", null);
                return this.root.FindIndex(startIndex, count, match);
            }
            /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the last occurrence within the entire immutable list.</summary>
            /// <returns>The last element that matches the conditions defined by the specified predicate, found; otherwise, the default value for type <paramref name="T" />.</returns>
            /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
            public T FindLast(Predicate<T> match) {
                Requires.NotNull<Predicate<T>>(match, "match");
                return this.root.FindLast(match);
            }
            /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the entire immutable list.</summary>
            /// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
            /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
            public int FindLastIndex(Predicate<T> match) {
                Requires.NotNull<Predicate<T>>(match, "match");
                return this.root.FindLastIndex(match);
            }
            /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the immutable list that extends from the first element to the specified index.</summary>
            /// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
            /// <param name="startIndex">The zero-based starting index of the backward search.</param>
            /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
            public int FindLastIndex(int startIndex, Predicate<T> match) {
                Requires.NotNull<Predicate<T>>(match, "match");
                Requires.Range(startIndex >= 0, "startIndex", null);
                Requires.Range(startIndex == 0 || startIndex < this.Count, "startIndex", null);
                return this.root.FindLastIndex(startIndex, match);
            }
            /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the immutable list that contains the specified number of elements and ends at the specified index.</summary>
            /// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
            /// <param name="startIndex">The zero-based starting index of the backward search.</param>
            /// <param name="count">The number of elements in the section to search.</param>
            /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
            public int FindLastIndex(int startIndex, int count, Predicate<T> match) {
                Requires.NotNull<Predicate<T>>(match, "match");
                Requires.Range(startIndex >= 0, "startIndex", null);
                Requires.Range(count <= this.Count, "count", null);
                Requires.Range(startIndex - count + 1 >= 0, "startIndex", null);
                return this.root.FindLastIndex(startIndex, count, match);
            }
            /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the immutable list that extends from the specified index to the last element.</summary>
            /// <returns>The zero-based index of the first occurrence of item within the range of elements in the immutable list that extends from <paramref name="index" /> to the last element, if found; otherwise, –1.</returns>
            /// <param name="item">The object to locate in the immutable list. The value can be null for reference types.</param>
            /// <param name="index">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
            public int IndexOf(T item, int index) {
                return this.root.IndexOf(item, index, this.Count - index, EqualityComparer<T>.Default);
            }
            /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the immutable list that starts at the specified index and contains the specified number of elements.</summary>
            /// <returns>The zero-based index of the first occurrence of item within the range of elements in the immutable list that starts at <paramref name="index" /> and contains <paramref name="count" /> number of elements, if found; otherwise, –1.</returns>
            /// <param name="item">The object to locate in the immutable list. The value can be null for reference types.</param>
            /// <param name="index">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
            /// <param name="count">The number of elements in the section to search.</param>
            public int IndexOf(T item, int index, int count) {
                return this.root.IndexOf(item, index, count, EqualityComparer<T>.Default);
            }
            /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:System.Collections.Immutable.ImmutableList`1.Builder" /> that starts at the specified index and contains the specified number of elements.</summary>
            /// <returns>The zero-based index of the first occurrence of item within the range of elements in the immutable list that starts at <paramref name="index" /> and contains <paramref name="count" /> number of elements, if found; otherwise, –1</returns>
            /// <param name="item">The object to locate in the immutable list. The value can be null for reference types.</param>
            /// <param name="index">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
            /// <param name="count">The number of elements to search.</param>
            /// <param name="equalityComparer">The value comparer to use for comparing elements for equality.</param>
            public int IndexOf(T item, int index, int count, IEqualityComparer<T> equalityComparer) {
                Requires.NotNull<IEqualityComparer<T>>(equalityComparer, "equalityComparer");
                return this.root.IndexOf(item, index, count, equalityComparer);
            }
            /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the entire immutable list.</summary>
            /// <returns>The zero-based index of the last occurrence of <paramref name="item" /> within the entire immutable list, if found; otherwise, –1.</returns>
            /// <param name="item">The object to locate in the immutable list. The value can be null for reference types.</param>
            public int LastIndexOf(T item) {
                if (this.Count == 0) {
                    return -1;
                }
                return this.root.LastIndexOf(item, this.Count - 1, this.Count, EqualityComparer<T>.Default);
            }
            /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the immutable list that extends from the first element to the specified index.</summary>
            /// <returns>The zero-based index of the last occurrence of <paramref name="item" /> within the range of elements in the immutable list that extends from the first element to <paramref name="index" />, if found; otherwise, –1.</returns>
            /// <param name="item">The object to locate in the immutable list. The value can be null for reference types.</param>
            /// <param name="startIndex">The zero-based starting index of the backward search.</param>
            public int LastIndexOf(T item, int startIndex) {
                if (this.Count == 0 && startIndex == 0) {
                    return -1;
                }
                return this.root.LastIndexOf(item, startIndex, startIndex + 1, EqualityComparer<T>.Default);
            }
            /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the immutable list that contains the specified number of elements and ends at the specified index.</summary>
            /// <returns>The zero-based index of the last occurrence of <paramref name="item" /> within the range of elements in the immutable list that contains <paramref name="count" /> number of elements and ends at <paramref name="index" />, if found; otherwise, –1.</returns>
            /// <param name="item">The object to locate in the immutable list. The value can be null for reference types.</param>
            /// <param name="startIndex">The zero-based starting index of the backward search.</param>
            /// <param name="count">The number of elements in the section to search.</param>
            public int LastIndexOf(T item, int startIndex, int count) {
                return this.root.LastIndexOf(item, startIndex, count, EqualityComparer<T>.Default);
            }
            /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the immutable list that contains the specified number of elements and ends at the specified index.</summary>
            /// <returns>The zero-based index of the first occurrence of item within the range of elements in the immutable list that starts at <paramref name="index" /> and contains <paramref name="count" /> number of elements, if found; otherwise, –1</returns>
            /// <param name="item">The object to locate in the immutable list. The value can be null for reference types.</param>
            /// <param name="startIndex">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
            /// <param name="count">The number of elements to search.</param>
            /// <param name="equalityComparer">The value comparer to use for comparing elements for equality.</param>
            public int LastIndexOf(T item, int startIndex, int count, IEqualityComparer<T> equalityComparer) {
                return this.root.LastIndexOf(item, startIndex, count, equalityComparer);
            }
            /// <summary>Determines whether every element in the immutable list matches the conditions defined by the specified predicate.</summary>
            /// <returns>true if every element in the immutable list matches the conditions defined by the specified predicate; otherwise, false. If the list has no elements, the return value is true.</returns>
            /// <param name="match">The delegate that defines the conditions to check against the elements.</param>
            public bool TrueForAll(Predicate<T> match) {
                Requires.NotNull<Predicate<T>>(match, "match");
                return this.root.TrueForAll(match);
            }
            public void AddRange(IEnumerable<T> items) {
                Requires.NotNull<IEnumerable<T>>(items, "items");
                foreach (T current in items) {
                    this.Root = this.Root.Add(current);
                }
            }
            /// <summary>Inserts the elements of a collection into the immutable list at the specified index.</summary>
            /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
            /// <param name="items">The collection whose elements should be inserted into the immutable list. The collection itself cannot be null, but it can contain elements that are null, if type <paramref name="T" /> is a reference type.</param>
            public void InsertRange(int index, IEnumerable<T> items) {
                Requires.Range(index >= 0 && index <= this.Count, "index", null);
                Requires.NotNull<IEnumerable<T>>(items, "items");
                foreach (T current in items) {
                    this.Root = this.Root.Insert(index++, current);
                }
            }
            /// <summary>Removes all the elements that match the conditions defined by the specified predicate.</summary>
            /// <returns>The number of elements removed from the immutable list.</returns>
            /// <param name="match">The delegate that defines the conditions of the elements to remove.</param>
            public int RemoveAll(Predicate<T> match) {
                Requires.NotNull<Predicate<T>>(match, "match");
                int count = this.Count;
                this.Root = this.Root.RemoveAll(match);
                return count - this.Count;
            }
            /// <summary>Reverses the order of the elements in the entire immutable list.</summary>
            public void Reverse() {
                this.Reverse(0, this.Count);
            }
            /// <summary>Reverses the order of the elements in the specified range of the immutable list.</summary>
            /// <param name="index">The zero-based starting index of the range to reverse.</param>
            /// <param name="count">The number of elements in the range to reverse.</param>
            public void Reverse(int index, int count) {
                Requires.Range(index >= 0, "index", null);
                Requires.Range(count >= 0, "count", null);
                Requires.Range(index + count <= this.Count, "count", null);
                this.Root = this.Root.Reverse(index, count);
            }
            /// <summary>Sorts the elements in the entire immutable list by using the default comparer.</summary>
            public void Sort() {
                this.Root = this.Root.Sort();
            }
            /// <summary>Sorts the elements in the entire immutable list by using the specified comparison object.</summary>
            /// <param name="comparison">The object to use when comparing elements.</param>
            public void Sort(Comparison<T> comparison) {
                Requires.NotNull<Comparison<T>>(comparison, "comparison");
                this.Root = this.Root.Sort(comparison);
            }
            /// <summary>Sorts the elements in the entire immutable list by using the specified comparer.</summary>
            /// <param name="comparer">The implementation to use when comparing elements, or null to use the default comparer (<see cref="P:System.Collections.Generic.Comparer&lt;T&gt;.Default." />).</param>
            public void Sort(IComparer<T> comparer) {
                Requires.NotNull<IComparer<T>>(comparer, "comparer");
                this.Root = this.Root.Sort(comparer);
            }
            /// <summary>Sorts the elements in a range of elements in the immutable list  by using the specified comparer.</summary>
            /// <param name="index">The zero-based starting index of the range to sort.</param>
            /// <param name="count">The length of the range to sort.</param>
            /// <param name="comparer">The implementation to use when comparing elements, or null to use the default comparer (<see cref="P:System.Collections.Generic.Comparer&lt;T&gt;.Default." />).</param>
            public void Sort(int index, int count, IComparer<T> comparer) {
                Requires.Range(index >= 0, "index", null);
                Requires.Range(count >= 0, "count", null);
                Requires.Range(index + count <= this.Count, "count", null);
                Requires.NotNull<IComparer<T>>(comparer, "comparer");
                this.Root = this.Root.Sort(index, count, comparer);
            }
            /// <summary>Searches the entire <see cref="T:System.Collections.Immutable.ImmutableList`1.Builder" /> for an element using the default comparer and returns the zero-based index of the element.</summary>
            /// <returns>The zero-based index of item in the <see cref="T:System.Collections.Immutable.ImmutableList`1.Builder" />, if item is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than <paramref name="item" />.</returns>
            /// <param name="item">The object to locate. The value can be null for reference types.</param>
            public int BinarySearch(T item) {
                return this.BinarySearch(item, null);
            }
            /// <summary>Searches the entire <see cref="T:System.Collections.Immutable.ImmutableList`1.Builder" /> for an element using the specified comparer and returns the zero-based index of the element.</summary>
            /// <returns>The zero-based index of item in the <see cref="T:System.Collections.Immutable.ImmutableList`1.Builder" />, if item is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than <paramref name="item" />.</returns>
            /// <param name="item">The object to locate. This value can be null for reference types.</param>
            /// <param name="comparer">The implementation to use when comparing elements, or null for the default comparer.</param>
            public int BinarySearch(T item, IComparer<T> comparer) {
                return this.BinarySearch(0, this.Count, item, comparer);
            }
            /// <summary>Searches the specified range of the <see cref="T:System.Collections.Immutable.ImmutableList`1.Builder" /> for an element using the specified comparer and returns the zero-based index of the element.</summary>
            /// <returns>The zero-based index of item in the <see cref="T:System.Collections.Immutable.ImmutableList`1.Builder" />, if item is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than <paramref name="item" />.</returns>
            /// <param name="index">The zero-based starting index of the range to search.</param>
            /// <param name="count">The length of the range to search.</param>
            /// <param name="item">The object to locate. This value can be null for reference types.</param>
            /// <param name="comparer">The implementation to use when comparing elements, or null for the default comparer.</param>
            public int BinarySearch(int index, int count, T item, IComparer<T> comparer) {
                return this.Root.BinarySearch(index, count, item, comparer);
            }
            /// <summary>Creates an immutable list based on the contents of this instance.</summary>
            /// <returns>An immutable list.</returns>
            public ImmutableList<T> ToImmutable() {
                if (this.immutable == null) {
                    this.immutable = ImmutableList<T>.WrapNode(this.Root);
                }
                return this.immutable;
            }
            /// <summary>Adds an item to the list.</summary>
            /// <returns>The position into which the new element was inserted, or -1 to indicate that the item was not inserted into the collection.</returns>
            /// <param name="value">The object to add to the list.</param>
            /// <exception cref="T:System.NotImplementedException"></exception>
            int IList.Add(object value) {
                this.Add((T)((object)value));
                return this.Count - 1;
            }
            /// <summary>Removes all items from the list.</summary>
            /// <exception cref="T:System.NotImplementedException"></exception>
            void IList.Clear() {
                this.Clear();
            }
            /// <summary>Determines whether the list contains a specific value.</summary>
            /// <returns>true if the <see cref="T:System.Object" /> is found in the list; otherwise, false.</returns>
            /// <param name="value">The object to locate in the list.</param>
            /// <exception cref="T:System.NotImplementedException"></exception>
            bool IList.Contains(object value) {
                return this.Contains((T)((object)value));
            }
            /// <summary>Determines the index of a specific item in the list.</summary>
            /// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
            /// <param name="value">The object to locate in the list.</param>
            /// <exception cref="T:System.NotImplementedException"></exception>
            int IList.IndexOf(object value) {
                return this.IndexOf((T)((object)value));
            }
            /// <summary>Inserts an item to the list at the specified index.</summary>
            /// <param name="index">The zero-based index at which <paramref name="value" /> should be inserted.</param>
            /// <param name="value">The object to insert into the list.</param>
            /// <exception cref="T:System.NotImplementedException"></exception>
            void IList.Insert(int index, object value) {
                this.Insert(index, (T)((object)value));
            }
            /// <summary>Removes the first occurrence of a specific object from the list.</summary>
            /// <param name="value">The object to remove from the list.</param>
            /// <exception cref="T:System.NotImplementedException"></exception>
            void IList.Remove(object value) {
                this.Remove((T)((object)value));
            }
            /// <summary>Copies the elements of the list to an array, starting at a particular array index.</summary>
            /// <param name="array">The one-dimensional array that is the destination of the elements copied from the list. The array must have zero-based indexing.</param>
            /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
            /// <exception cref="T:System.NotImplementedException"></exception>
            void ICollection.CopyTo(Array array, int arrayIndex) {
                this.Root.CopyTo(array, arrayIndex);
            }
        }
        /// <summary>Gets an empty set with the default sort comparer.</summary>
        /// <returns>An empty immutable list that uses the default sort comparer.</returns>
        public static readonly ImmutableList<T> Empty = new ImmutableList<T>();
        private readonly ImmutableList<T>.Node root;
        /// <summary>Gets a value that indicates whether this list is empty.</summary>
        /// <returns>true if the list is empty; otherwise, false.</returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool IsEmpty {
            get {
                return this.root.IsEmpty;
            }
        }
        /// <summary>Gets the number of elements contained in the list.</summary>
        /// <returns>The number of elements in the list.</returns>
        public int Count {
            get {
                return this.root.Count;
            }
        }
        /// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</summary>
        /// <returns>An object that can be used to synchronize access to the list.</returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object ICollection.SyncRoot {
            get {
                return this;
            }
        }
        /// <summary>Gets a value that indicates whether access to the list is synchronized.</summary>
        /// <returns>true if the access to the list is synchronized; otherwise, false.</returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection.IsSynchronized {
            get {
                return true;
            }
        }
        /// <summary>Gets the element at the specified index of the list.</summary>
        /// <returns>The element at the specified index.</returns>
        /// <param name="index">The index of the element to retrieve.</param>
        public T this[int index] {
            get {
                return this.root[index];
            }
        }
        T IOrderedCollection<T>.this[int index] {
            get {
                return this[index];
            }
        }
        T IList<T>.this[int index] {
            get {
                return this[index];
            }
            set {
                throw new NotSupportedException();
            }
        }
        bool ICollection<T>.IsReadOnly {
            get {
                return true;
            }
        }
        /// <summary>Gets a value that indicates whether the list has a fixed size.</summary>
        /// <returns>true if the list has a fixed size; otherwise, false.</returns>
        bool IList.IsFixedSize {
            get {
                return true;
            }
        }
        /// <summary>Gets a value that indicates whether the immutable list is read-only.</summary>
        /// <returns>true if the list is read-only; otherwise, false.</returns>
        bool IList.IsReadOnly {
            get {
                return true;
            }
        }
        /// <summary>Gets or sets the element at the specified index.</summary>
        /// <returns>The element at the specified index.</returns>
        /// <param name="index">The index of the item to get or set.</param>
        object IList.this[int index] {
            get {
                return this[index];
            }
            set {
                throw new NotSupportedException();
            }
        }
        internal ImmutableList() {
            this.root = ImmutableList<T>.Node.EmptyNode;
        }
        private ImmutableList(ImmutableList<T>.Node root) {
            Requires.NotNull<ImmutableList<T>.Node>(root, "root");
            root.Freeze();
            this.root = root;
        }
        /// <summary>Removes all elements from the immutable list.</summary>
        /// <returns>An empty list that retains the same sort or unordered semantics that this instance has.</returns>
        public ImmutableList<T> Clear() {
            return ImmutableList<T>.Empty;
        }
        /// <summary> Searches the entire sorted list for an element using the default comparer and returns the zero-based index of the element. </summary>
        /// <returns>The zero-based index of item in the sorted List;, if item is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than item or, if there is no larger element, the bitwise complement of <see cref="P:System.Collections.ICollection.Count" />.</returns>
        /// <param name="item">The object to locate. The value can be null for reference types.</param>
        /// <exception cref="T:System.InvalidOperationException"> The default comparer cannot find a comparer implementation of the for type T. </exception>
        public int BinarySearch(T item) {
            return this.BinarySearch(item, null);
        }
        /// <summary>  Searches the entire sorted list for an element using the specified comparer and returns the zero-based index of the element. </summary>
        /// <returns>The zero-based index of item in the sorted List;, if item is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than item or, if there is no larger element, the bitwise complement of <see cref="P:System.Collections.ICollection.Count" />.</returns>
        /// <param name="item">The object to locate. The value can be null for reference types.</param>
        /// <param name="comparer"> The  comparer implementation to use when comparing elements or null to use the default comparer. </param>
        /// <exception cref="T:System.InvalidOperationException"> comparer is null, and the default comparer cannot find an comparer implementation for type T. </exception>
        public int BinarySearch(T item, IComparer<T> comparer) {
            return this.BinarySearch(0, this.Count, item, comparer);
        }
        /// <summary> Searches a range of elements in the sorted list for an element using the specified comparer and returns the zero-based index of the element. </summary>
        /// <returns> The zero-based index of item in the sorted list;, if item is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than item or, if there is no larger element, the bitwise complement of <paramref name="count" />. </returns>
        /// <param name="index">The zero-based starting index of the range to search.</param>
        /// <param name="count"> The length of the range to search.</param>
        /// <param name="item">The object to locate. The value can be null for reference types.</param>
        /// <param name="comparer"> The comparer implementation to use when comparing elements, or null to use the default comparer. </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">index is less than 0 or <paramref name="count" /> is less than 0. </exception>
        /// <exception cref="T:System.ArgumentException">index and <paramref name="count" /> do not denote a valid range in the list. </exception>
        /// <exception cref="T:System.InvalidOperationException">
        ///   <paramref name="comparer" /> is null, and the default comparer cannot find an comparer implementation for type T. </exception>
        public int BinarySearch(int index, int count, T item, IComparer<T> comparer) {
            return this.root.BinarySearch(index, count, item, comparer);
        }
        IImmutableList<T> IImmutableList<T>.Clear() {
            return this.Clear();
        }
        /// <summary>Creates a list that has the same contents as this list and can be efficiently mutated across multiple operations using standard mutable interfaces.</summary>
        /// <returns>The created list with the same contents as this list.</returns>
        public ImmutableList<T>.Builder ToBuilder() {
            return new ImmutableList<T>.Builder(this);
        }
        /// <summary>Adds the specified object to the end of the immutable list.</summary>
        /// <returns>A new immutable list with the object added, or the current list if it already contains the specified object.</returns>
        /// <param name="value">The object to add.</param>
        public ImmutableList<T> Add(T value) {
            ImmutableList<T>.Node node = this.root.Add(value);
            return this.Wrap(node);
        }
        /// <summary>Adds the elements of the specified collection to the end of the immutable list.</summary>
        /// <returns>A new immutable list with the elements added, or the current list if it already contains the specified elements.</returns>
        /// <param name="items">The collection whose elements will be added to the end of the list.</param>
        public ImmutableList<T> AddRange(IEnumerable<T> items) {
            Requires.NotNull<IEnumerable<T>>(items, "items");
            if (this.IsEmpty) {
                return this.FillFromEmpty(items);
            }
            ImmutableList<T>.Node node = this.root;
            foreach (T current in items) {
                node = node.Add(current);
            }
            return this.Wrap(node);
        }
        /// <summary>Inserts the specified object into the immutable list at the specified index.</summary>
        /// <returns>The new immutable list after the object is inserted.</returns>
        /// <param name="index">The zero-based index at which to insert the object.</param>
        /// <param name="item">The object to insert.</param>
        public ImmutableList<T> Insert(int index, T item) {
            Requires.Range(index >= 0 && index <= this.Count, "index", null);
            return this.Wrap(this.root.Insert(index, item));
        }
        /// <summary>Inserts the elements of a collection into the immutable list at the specified index.</summary>
        /// <returns>The new immutable list after the elements are inserted.</returns>
        /// <param name="index">The zero-based index at which to insert the elements.</param>
        /// <param name="items">The collection whose elements should be inserted.</param>
        public ImmutableList<T> InsertRange(int index, IEnumerable<T> items) {
            Requires.Range(index >= 0 && index <= this.Count, "index", null);
            Requires.NotNull<IEnumerable<T>>(items, "items");
            ImmutableList<T>.Node node = this.root;
            foreach (T current in items) {
                node = node.Insert(index++, current);
            }
            return this.Wrap(node);
        }
        /// <summary>Removes the first occurrence of the specified object from this immutable list.</summary>
        /// <returns>A new list with the object removed, or this list if the specified object is not in this list.</returns>
        /// <param name="value">The object to remove.</param>
        public ImmutableList<T> Remove(T value) {
            return this.Remove(value, EqualityComparer<T>.Default);
        }
        /// <summary>Removes the first occurrence of the object that matches the specified value from this immutable list.</summary>
        /// <returns>A new list with the object removed, or this list if the specified object is not in this list.</returns>
        /// <param name="value">The value of the element to remove from the list.</param>
        /// <param name="equalityComparer">The equality comparer to use in the search. </param>
        public ImmutableList<T> Remove(T value, IEqualityComparer<T> equalityComparer) {
            int num = this.IndexOf(value, equalityComparer);
            if (num >= 0) {
                return this.RemoveAt(num);
            }
            return this;
        }
        /// <summary>Removes a range of elements, starting from the specified index and containing the specified number of elements, from this immutable list.</summary>
        /// <returns>A new list with the elements removed.</returns>
        /// <param name="index">The starting index to begin removal.</param>
        /// <param name="count">The number of elements to remove.</param>
        public ImmutableList<T> RemoveRange(int index, int count) {
            Requires.Range(index >= 0 && (index < this.Count || (index == this.Count && count == 0)), "index", null);
            Requires.Range(count >= 0 && index + count <= this.Count, "count", null);
            ImmutableList<T>.Node node = this.root;
            int num = count;
            while (num-- > 0) {
                node = node.RemoveAt(index);
            }
            return this.Wrap(node);
        }
        /// <summary>Removes a range of elements from this immutable list.</summary>
        /// <returns>A new list with the elements removed.</returns>
        /// <param name="items">The collection whose elements should be removed if matches are found in this list.</param>
        public ImmutableList<T> RemoveRange(IEnumerable<T> items) {
            return this.RemoveRange(items, EqualityComparer<T>.Default);
        }
        /// <summary>Removes the specified values from this list.</summary>
        /// <returns>A new list with the elements removed.</returns>
        /// <param name="items">The items to remove if matches are found in this list.</param>
        /// <param name="equalityComparer">The equality comparer to use in the search.</param>
        public ImmutableList<T> RemoveRange(IEnumerable<T> items, IEqualityComparer<T> equalityComparer) {
            Requires.NotNull<IEnumerable<T>>(items, "items");
            Requires.NotNull<IEqualityComparer<T>>(equalityComparer, "equalityComparer");
            if (this.IsEmpty) {
                return this;
            }
            ImmutableList<T>.Node node = this.root;
            foreach (T current in items) {
                int num = node.IndexOf(current, equalityComparer);
                if (num >= 0) {
                    node = node.RemoveAt(num);
                }
            }
            return this.Wrap(node);
        }
        /// <summary>Removes the element at the specified index.</summary>
        /// <returns>A new list with the element removed.</returns>
        /// <param name="index">The zero-based index of the element to remove.</param>
        public ImmutableList<T> RemoveAt(int index) {
            Requires.Range(index >= 0 && index < this.Count, "index", null);
            ImmutableList<T>.Node node = this.root.RemoveAt(index);
            return this.Wrap(node);
        }
        /// <summary>Removes all the elements that match the conditions defined by the specified predicate.</summary>
        /// <returns>The new list with the elements removed.</returns>
        /// <param name="match">The delegate that defines the conditions of the elements to remove.</param>
        public ImmutableList<T> RemoveAll(Predicate<T> match) {
            Requires.NotNull<Predicate<T>>(match, "match");
            return this.Wrap(this.root.RemoveAll(match));
        }
        /// <summary>Replaces an element at a given position in the immutable list with the specified element.</summary>
        /// <returns>The new list with the replaced element, even if it is equal to the old element at that position.</returns>
        /// <param name="index">The position in the list of the element to replace.</param>
        /// <param name="value">The element to replace the old element with.</param>
        public ImmutableList<T> SetItem(int index, T value) {
            return this.Wrap(this.root.ReplaceAt(index, value));
        }
        /// <summary>Replaces the specified element in the immutable list with a new element.</summary>
        /// <returns>The new list with the replaced element, even if it is equal to the old element.</returns>
        /// <param name="oldValue">The element to replace.</param>
        /// <param name="newValue">The element to replace <paramref name="oldValue" /> with.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="oldValue" /> does not exist in the immutable list.</exception>
        public ImmutableList<T> Replace(T oldValue, T newValue) {
            return this.Replace(oldValue, newValue, EqualityComparer<T>.Default);
        }
        /// <summary>Replaces the specified element in the immutable list with a new element.</summary>
        /// <returns>A new list with the object replaced, or this list if the specified object is not in this list.</returns>
        /// <param name="oldValue">The element to replace in the list.</param>
        /// <param name="newValue">The element to replace <paramref name="oldValue" /> with.</param>
        /// <param name="equalityComparer">The comparer to use to check for equality.</param>
        public ImmutableList<T> Replace(T oldValue, T newValue, IEqualityComparer<T> equalityComparer) {
            Requires.NotNull<IEqualityComparer<T>>(equalityComparer, "equalityComparer");
            int num = this.IndexOf(oldValue, equalityComparer);
            if (num < 0) {
                throw new ArgumentException(@"Cannot find the old value", "oldValue");
            }
            return this.SetItem(num, newValue);
        }
        /// <summary>Reverses the order of the elements in the entire immutable list.</summary>
        /// <returns>The reversed list.</returns>
        public ImmutableList<T> Reverse() {
            return this.Wrap(this.root.Reverse());
        }
        /// <summary>Reverses the order of the elements in the specified range of the immutable list.</summary>
        /// <returns>The reversed list.</returns>
        /// <param name="index">The zero-based starting index of the range to reverse.</param>
        /// <param name="count">The number of elements in the range to reverse.</param>
        public ImmutableList<T> Reverse(int index, int count) {
            return this.Wrap(this.root.Reverse(index, count));
        }
        /// <summary>Sorts the elements in the entire immutable list using the default comparer.</summary>
        /// <returns>The sorted list.</returns>
        public ImmutableList<T> Sort() {
            return this.Wrap(this.root.Sort());
        }
        /// <summary>Sorts the elements in the entire immutable list using the specified <see cref="T:System.Comparison&lt;T&gt;" />.</summary>
        /// <returns>The sorted list.</returns>
        /// <param name="comparison">The delegate to use when comparing elements.</param>
        public ImmutableList<T> Sort(Comparison<T> comparison) {
            Requires.NotNull<Comparison<T>>(comparison, "comparison");
            return this.Wrap(this.root.Sort(comparison));
        }
        /// <summary>Sorts the elements in the entire immutable list using the specified comparer.</summary>
        /// <returns>The sorted list.</returns>
        /// <param name="comparer">The  implementation to use when comparing elements, or null to use the default comparer (<see cref="T:System.Collections.Generic.Comparer&lt;T&gt;.Default" />).</param>
        public ImmutableList<T> Sort(IComparer<T> comparer) {
            Requires.NotNull<IComparer<T>>(comparer, "comparer");
            return this.Wrap(this.root.Sort(comparer));
        }
        /// <summary>Sorts a range of elements in the immutable list using the specified comparer.</summary>
        /// <returns>The sorted list.</returns>
        /// <param name="index">The zero-based starting index of the range to sort.</param>
        /// <param name="count">The length of the range to sort.</param>
        /// <param name="comparer">The implementation to use when comparing elements, or null to use the default comparer (<see cref="T:System.Collections.Generic.Comparer&lt;T&gt;.Default" />).</param>
        public ImmutableList<T> Sort(int index, int count, IComparer<T> comparer) {
            Requires.Range(index >= 0, "index", null);
            Requires.Range(count >= 0, "count", null);
            Requires.Range(index + count <= this.Count, "count", null);
            Requires.NotNull<IComparer<T>>(comparer, "comparer");
            return this.Wrap(this.root.Sort(index, count, comparer));
        }
        /// <summary>Performs the specified action on each element of the immutable list.</summary>
        /// <param name="action">The delegate to perform on each element of the immutable list.</param>
        public void ForEach(Action<T> action) {
            Requires.NotNull<Action<T>>(action, "action");
            foreach (T current in this) {
                action(current);
            }
        }
        /// <summary>Copies the entire immutable list to a compatible one-dimensional array, starting at the beginning of the target array.</summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from the immutable list. The array must have zero-based indexing.</param>
        public void CopyTo(T[] array) {
            Requires.NotNull<T[]>(array, "array");
            Requires.Range(array.Length >= this.Count, "array", null);
            this.root.CopyTo(array);
        }
        /// <summary>Copies the entire immutable list to a compatible one-dimensional array, starting at the specified index of the target array.</summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from the immutable list. The array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex) {
            Requires.NotNull<T[]>(array, "array");
            Requires.Range(arrayIndex >= 0, "arrayIndex", null);
            Requires.Range(array.Length >= arrayIndex + this.Count, "arrayIndex", null);
            this.root.CopyTo(array, arrayIndex);
        }
        void ICollection<T>.CopyTo(T[] array, int arrayIndex) { // MIKANT :: CONTRACT STUB
            CopyTo(array, arrayIndex);
        }
        /// <summary>Copies a range of elements from the immutable list to a compatible one-dimensional array, starting at the specified index of the target array.</summary>
        /// <param name="index">The zero-based index in the source immutable list at which copying begins.</param>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from the immutable list. The array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <param name="count">The number of elements to copy.</param>
        public void CopyTo(int index, T[] array, int arrayIndex, int count) {
            this.root.CopyTo(index, array, arrayIndex, count);
        }
        /// <summary>Creates a shallow copy of a range of elements in the source immutable list.</summary>
        /// <returns>A shallow copy of a range of elements in the source immutable list.</returns>
        /// <param name="index">The zero-based index at which the range starts.</param>
        /// <param name="count">The number of elements in the range.</param>
        public ImmutableList<T> GetRange(int index, int count) {
            Requires.Range(index >= 0, "index", null);
            Requires.Range(count >= 0, "count", null);
            Requires.Range(index + count <= this.Count, "count", null);
            return this.Wrap(ImmutableList<T>.Node.NodeTreeFromList(this, index, count));
        }
        /// <summary>Converts the elements in the current immutable list to another type, and returns a list containing the converted elements.</summary>
        /// <param name="converter">A delegate that converts each element from one type to another type.</param>
        /// <typeparam name="TOutput">The type of the elements of the target array.</typeparam>
        public ImmutableList<TOutput> ConvertAll<TOutput>(Func<T, TOutput> converter) {
            Requires.NotNull<Func<T, TOutput>>(converter, "converter");
            return ImmutableList<TOutput>.WrapNode(this.root.ConvertAll<TOutput>(converter));
        }
        /// <summary>Determines whether the immutable list contains elements that match the conditions defined by the specified predicate.</summary>
        /// <returns>true if the immutable list contains one or more elements that match the conditions defined by the specified predicate; otherwise, false.</returns>
        /// <param name="match">The delegate that defines the conditions of the elements to search for.</param>
        public bool Exists(Predicate<T> match) {
            Requires.NotNull<Predicate<T>>(match, "match");
            return this.root.Exists(match);
        }
        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the first occurrence within the entire immutable list.</summary>
        /// <returns>The first element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type <paramref name="T" />.</returns>
        /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
        public T Find(Predicate<T> match) {
            Requires.NotNull<Predicate<T>>(match, "match");
            return this.root.Find(match);
        }
        /// <summary>Retrieves all the elements that match the conditions defined by the specified predicate.</summary>
        /// <returns>An immutable list that contains all the elements that match the conditions defined by the specified predicate, if found; otherwise, an empty immutable list.</returns>
        /// <param name="match">The delegate that defines the conditions of the elements to search for.</param>
        public ImmutableList<T> FindAll(Predicate<T> match) {
            Requires.NotNull<Predicate<T>>(match, "match");
            return this.root.FindAll(match);
        }
        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the entire immutable list.</summary>
        /// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by match, if found; otherwise, ?1.</returns>
        /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
        public int FindIndex(Predicate<T> match) {
            Requires.NotNull<Predicate<T>>(match, "match");
            return this.root.FindIndex(match);
        }
        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the immutable list that extends from the specified index to the last element.</summary>
        /// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by match, if found; otherwise, ?1.</returns>
        /// <param name="startIndex">The zero-based starting index of the search.</param>
        /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
        public int FindIndex(int startIndex, Predicate<T> match) {
            Requires.NotNull<Predicate<T>>(match, "match");
            Requires.Range(startIndex >= 0, "startIndex", null);
            Requires.Range(startIndex <= this.Count, "startIndex", null);
            return this.root.FindIndex(startIndex, match);
        }
        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the immutable list that starts at the specified index and contains the specified number of elements.</summary>
        /// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by match, if found; otherwise, ?1.</returns>
        /// <param name="startIndex">The zero-based starting index of the search.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
        public int FindIndex(int startIndex, int count, Predicate<T> match) {
            Requires.NotNull<Predicate<T>>(match, "match");
            Requires.Range(startIndex >= 0, "startIndex", null);
            Requires.Range(count >= 0, "count", null);
            Requires.Range(startIndex + count <= this.Count, "count", null);
            return this.root.FindIndex(startIndex, count, match);
        }
        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the last occurrence within the entire immutable list.</summary>
        /// <returns>The last element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type <paramref name="T" />.</returns>
        /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
        public T FindLast(Predicate<T> match) {
            Requires.NotNull<Predicate<T>>(match, "match");
            return this.root.FindLast(match);
        }
        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the entire immutable list.</summary>
        /// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, ?1.</returns>
        /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
        public int FindLastIndex(Predicate<T> match) {
            Requires.NotNull<Predicate<T>>(match, "match");
            return this.root.FindLastIndex(match);
        }
        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the immutable list that extends from the first element to the specified index.</summary>
        /// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, ?1.</returns>
        /// <param name="startIndex">The zero-based starting index of the backward search.</param>
        /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
        public int FindLastIndex(int startIndex, Predicate<T> match) {
            Requires.NotNull<Predicate<T>>(match, "match");
            Requires.Range(startIndex >= 0, "startIndex", null);
            Requires.Range(startIndex == 0 || startIndex < this.Count, "startIndex", null);
            return this.root.FindLastIndex(startIndex, match);
        }
        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the immutable list that contains the specified number of elements and ends at the specified index.</summary>
        /// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, ?1.</returns>
        /// <param name="startIndex">The zero-based starting index of the backward search.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
        public int FindLastIndex(int startIndex, int count, Predicate<T> match) {
            Requires.NotNull<Predicate<T>>(match, "match");
            Requires.Range(startIndex >= 0, "startIndex", null);
            Requires.Range(count <= this.Count, "count", null);
            Requires.Range(startIndex - count + 1 >= 0, "startIndex", null);
            return this.root.FindLastIndex(startIndex, count, match);
        }
        /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the list that starts at the specified index and contains the specified number of elements.</summary>
        /// <returns>The zero-based index of the first occurrence of item within the range of elements in the list that starts at index and contains count number of elements, if found; otherwise, –1.</returns>
        /// <param name="item">The object to locate in the list The value can be null for reference types.</param>
        /// <param name="index">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <param name="equalityComparer">The equality comparer to use in the search.</param>
        public int IndexOf(T item, int index, int count, IEqualityComparer<T> equalityComparer) {
            return this.root.IndexOf(item, index, count, equalityComparer);
        }
        /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the list that contains the specified number of elements and ends at the specified index.</summary>
        /// <returns>The zero-based index of the last occurrence of item within the range of elements in the list that contains count number of elements and ends at index, if found; otherwise, –1.</returns>
        /// <param name="item">The object to locate in the list. The value can be null for reference types.</param>
        /// <param name="index">The zero-based starting index of the backward search.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <param name="equalityComparer">The equality comparer to use in the search.</param>
        public int LastIndexOf(T item, int index, int count, IEqualityComparer<T> equalityComparer) {
            return this.root.LastIndexOf(item, index, count, equalityComparer);
        }
        /// <summary>Determines whether every element in the immutable list matches the conditions defined by the specified predicate.</summary>
        /// <returns>true if every element in the immutable list matches the conditions defined by the specified predicate; otherwise, false. If the list has no elements, the return value is true.</returns>
        /// <param name="match">The delegate that defines the conditions to check against the elements.</param>
        public bool TrueForAll(Predicate<T> match) {
            Requires.NotNull<Predicate<T>>(match, "match");
            return this.root.TrueForAll(match);
        }
        /// <summary>Determines whether this immutable list contains the specified value.</summary>
        /// <returns>true if the list contains the specified value; otherwise, false.</returns>
        /// <param name="value">The value to locate.</param>
        public bool Contains(T value) {
            return this.IndexOf(value) >= 0;
        }
        /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the entire immutable list.</summary>
        /// <returns>The zero-based index of the first occurrence of <paramref name="value" /> within the entire immutable list, if found; otherwise, ?1.</returns>
        /// <param name="value">The object to locate in the immutable list. The value can be <paramref name="null" /> for reference types.</param>
        public int IndexOf(T value) {
            return this.IndexOf(value, EqualityComparer<T>.Default);
        }
        [ExcludeFromCodeCoverage]
        IImmutableList<T> IImmutableList<T>.Add(T value) {
            return this.Add(value);
        }
        [ExcludeFromCodeCoverage]
        IImmutableList<T> IImmutableList<T>.AddRange(IEnumerable<T> items) {
            return this.AddRange(items);
        }
        [ExcludeFromCodeCoverage]
        IImmutableList<T> IImmutableList<T>.Insert(int index, T item) {
            return this.Insert(index, item);
        }
        [ExcludeFromCodeCoverage]
        IImmutableList<T> IImmutableList<T>.InsertRange(int index, IEnumerable<T> items) {
            return this.InsertRange(index, items);
        }
        [ExcludeFromCodeCoverage]
        IImmutableList<T> IImmutableList<T>.Remove(T value, IEqualityComparer<T> equalityComparer) {
            return this.Remove(value, equalityComparer);
        }
        [ExcludeFromCodeCoverage]
        IImmutableList<T> IImmutableList<T>.RemoveAll(Predicate<T> match) {
            return this.RemoveAll(match);
        }
        [ExcludeFromCodeCoverage]
        IImmutableList<T> IImmutableList<T>.RemoveRange(IEnumerable<T> items, IEqualityComparer<T> equalityComparer) {
            return this.RemoveRange(items, equalityComparer);
        }
        [ExcludeFromCodeCoverage]
        IImmutableList<T> IImmutableList<T>.RemoveRange(int index, int count) {
            return this.RemoveRange(index, count);
        }
        [ExcludeFromCodeCoverage]
        IImmutableList<T> IImmutableList<T>.RemoveAt(int index) {
            return this.RemoveAt(index);
        }
        [ExcludeFromCodeCoverage]
        IImmutableList<T> IImmutableList<T>.SetItem(int index, T value) {
            return this.SetItem(index, value);
        }
        [ExcludeFromCodeCoverage]
        IImmutableList<T> IImmutableList<T>.Replace(T oldValue, T newValue, IEqualityComparer<T> equalityComparer) {
            return this.Replace(oldValue, newValue, equalityComparer);
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            return this.GetEnumerator();
        }
        /// <summary>Returns an enumerator that iterates through the immutable list.</summary>
        /// <returns>An enumerator that can be used to iterate through the list.</returns>
        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }
        void IList<T>.Insert(int index, T item) {
            throw new NotSupportedException();
        }
        void IList<T>.RemoveAt(int index) {
            throw new NotSupportedException();
        }
        void ICollection<T>.Add(T item) {
            throw new NotSupportedException();
        }
        void ICollection<T>.Clear() {
            throw new NotSupportedException();
        }
        bool ICollection<T>.Remove(T item) {
            throw new NotSupportedException();
        }
        /// <summary>Copies the entire immutable list to a compatible one-dimensional array, starting at the specified array index.</summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from immutable list.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        void ICollection.CopyTo(Array array, int arrayIndex) {
            this.root.CopyTo(array, arrayIndex);
        }
        /// <summary>Adds an item to the immutable list.</summary>
        /// <returns>The position into which the new element was inserted, or -1 to indicate that the item was not inserted into the list.</returns>
        /// <param name="value">The object to add to the list.</param>
        /// <exception cref="T:System.NotImplementedException"></exception>
        int IList.Add(object value) {
            throw new NotSupportedException();
        }
        /// <summary>Removes the item at the specified index of the immutable list.</summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="T:System.NotSupportedException"></exception>
        void IList.RemoveAt(int index) {
            throw new NotSupportedException();
        }
        /// <summary>Removes all items from the immutable list.</summary>
        /// <exception cref="T:System.NotImplementedException"></exception>
        void IList.Clear() {
            throw new NotSupportedException();
        }
        /// <summary>Determines whether the immutable list contains a specific value.</summary>
        /// <returns>true if the object is found in the list; otherwise, false.</returns>
        /// <param name="value">The object to locate in the list.</param>
        /// <exception cref="T:System.NotImplementedException"></exception>
        bool IList.Contains(object value) {
            return this.Contains((T)((object)value));
        }
        /// <summary>Determines the index of a specific item in the immutable list.</summary>
        /// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
        /// <param name="value">The object to locate in the list.</param>
        /// <exception cref="T:System.NotImplementedException"></exception>
        int IList.IndexOf(object value) {
            return this.IndexOf((T)((object)value));
        }
        /// <summary>Inserts an item into the immutable list at the specified index.</summary>
        /// <param name="index">The zero-based index at which <paramref name="value" /> should be inserted.</param>
        /// <param name="value">The object to insert into the list.</param>
        /// <exception cref="T:System.NotImplementedException"></exception>
        void IList.Insert(int index, object value) {
            throw new NotSupportedException();
        }
        /// <summary>Removes the first occurrence of a specific object from the immutable list.</summary>
        /// <param name="value">The object to remove from the list.</param>
        /// <exception cref="T:System.NotImplementedException"></exception>
        void IList.Remove(object value) {
            throw new NotSupportedException();
        }
        /// <summary>Returns an enumerator that iterates through the immutable list.</summary>
        /// <returns>An enumerator  that can be used to iterate through the immutable list.</returns>
        public ImmutableList<T>.Enumerator GetEnumerator() {
            return new ImmutableList<T>.Enumerator(this.root, null, -1, -1, false);
        }
        private static ImmutableList<T> WrapNode(ImmutableList<T>.Node root) {
            if (!root.IsEmpty) {
                return new ImmutableList<T>(root);
            }
            return ImmutableList<T>.Empty;
        }
        private static bool TryCastToImmutableList(IEnumerable<T> sequence, out ImmutableList<T> other) {
            other = (sequence as ImmutableList<T>);
            if (other != null) {
                return true;
            }
            ImmutableList<T>.Builder builder = sequence as ImmutableList<T>.Builder;
            if (builder != null) {
                other = builder.ToImmutable();
                return true;
            }
            return false;
        }
        private ImmutableList<T> Wrap(ImmutableList<T>.Node root) {
            if (root == this.root) {
                return this;
            }
            if (!root.IsEmpty) {
                return new ImmutableList<T>(root);
            }
            return this.Clear();
        }
        private ImmutableList<T> FillFromEmpty(IEnumerable<T> items) {
            ImmutableList<T> result;
            if (ImmutableList<T>.TryCastToImmutableList(items, out result)) {
                return result;
            }
            IOrderedCollection<T> orderedCollection = items.AsOrderedCollection<T>();
            if (orderedCollection.Count == 0) {
                return this;
            }
            ImmutableList<T>.Node node = ImmutableList<T>.Node.NodeTreeFromList(orderedCollection, 0, orderedCollection.Count);
            return new ImmutableList<T>(node);
        }
    }
}
