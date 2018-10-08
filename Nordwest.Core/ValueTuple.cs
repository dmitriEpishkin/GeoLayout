using System;
using System.Collections.Generic;

namespace Nordwest {
    public static class ValueTuple {
        public static ValueTuple<T1, T2> Create<T1, T2>(T1 i1, T2 i2) {
            return new ValueTuple<T1, T2>(i1, i2);
        }
        public static ValueTuple<T1, T2, T3> Create<T1, T2, T3>(T1 i1, T2 i2, T3 i3) {
            return new ValueTuple<T1, T2, T3>(i1, i2, i3);
        }
        public static ValueTuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 i1, T2 i2, T3 i3, T4 i4) {
            return new ValueTuple<T1, T2, T3, T4>(i1, i2, i3, i4);
        }
    }

    public struct ValueTuple<T1, T2> : IEquatable<ValueTuple<T1, T2>> {
        private readonly T1 _i1;
        private readonly T2 _i2;

        private static readonly EqualityComparer<T1> T1Comparer = EqualityComparer<T1>.Default;
        private static readonly EqualityComparer<T2> T2Comparer = EqualityComparer<T2>.Default;

        public ValueTuple(T1 i1, T2 i2) {
            _i1 = i1;
            _i2 = i2;
        }

        public T1 I1 { get { return _i1; } }
        public T2 I2 { get { return _i2; } }

        public bool Equals(ValueTuple<T1, T2> other) {
            return T1Comparer.Equals(_i1, other._i1)
                && T2Comparer.Equals(_i2, other._i2);
        }

        public override bool Equals(object obj) {
            return obj is ValueTuple<T1, T2> && Equals((ValueTuple<T1, T2>)obj);
        }
        public override int GetHashCode() {
            return unchecked(T1Comparer.GetHashCode(_i1) * (int)0xA5555529) + T2Comparer.GetHashCode(_i2);
        }

        public static bool operator ==(ValueTuple<T1, T2> left, ValueTuple<T1, T2> right) {
            return left.Equals(right);
        }
        public static bool operator !=(ValueTuple<T1, T2> left, ValueTuple<T1, T2> right) {
            return !left.Equals(right);
        }
    }

    public struct ValueTuple<T1, T2, T3> : IEquatable<ValueTuple<T1, T2, T3>> {
        private readonly T1 _i1;
        private readonly T2 _i2;
        private readonly T3 _i3;

        private static readonly EqualityComparer<T1> T1Comparer = EqualityComparer<T1>.Default;
        private static readonly EqualityComparer<T2> T2Comparer = EqualityComparer<T2>.Default;
        private static readonly EqualityComparer<T3> T3Comparer = EqualityComparer<T3>.Default;

        public ValueTuple(T1 i1, T2 i2, T3 i3) {
            _i1 = i1;
            _i2 = i2;
            _i3 = i3;
        }

        public T1 I1 { get { return _i1; } }
        public T2 I2 { get { return _i2; } }
        public T3 I3 { get { return _i3; } }

        public bool Equals(ValueTuple<T1, T2, T3> other) {
            return T1Comparer.Equals(_i1, other._i1)
                && T2Comparer.Equals(_i2, other._i2)
                && T3Comparer.Equals(_i3, other._i3);
        }

        public override bool Equals(object obj) {
            return obj is ValueTuple<T1, T2, T3> && Equals((ValueTuple<T1, T2, T3>)obj);
        }
        public override int GetHashCode() {
            return unchecked(((T1Comparer.GetHashCode(_i1) * (int)0xA5555529) + T2Comparer.GetHashCode(_i2)) * (int)0xA5555529) + T3Comparer.GetHashCode(_i3);
        }

        public static bool operator ==(ValueTuple<T1, T2, T3> left, ValueTuple<T1, T2, T3> right) {
            return left.Equals(right);
        }
        public static bool operator !=(ValueTuple<T1, T2, T3> left, ValueTuple<T1, T2, T3> right) {
            return !left.Equals(right);
        }
    }

    public struct ValueTuple<T1, T2, T3, T4> : IEquatable<ValueTuple<T1, T2, T3, T4>> {
        private readonly T1 _i1;
        private readonly T2 _i2;
        private readonly T3 _i3;
        private readonly T4 _i4;

        private static readonly EqualityComparer<T1> T1Comparer = EqualityComparer<T1>.Default;
        private static readonly EqualityComparer<T2> T2Comparer = EqualityComparer<T2>.Default;
        private static readonly EqualityComparer<T3> T3Comparer = EqualityComparer<T3>.Default;
        private static readonly EqualityComparer<T4> T4Comparer = EqualityComparer<T4>.Default;

        public ValueTuple(T1 i1, T2 i2, T3 i3, T4 i4) {
            _i1 = i1;
            _i2 = i2;
            _i3 = i3;
            _i4 = i4;
        }

        public T1 I1 { get { return _i1; } }
        public T2 I2 { get { return _i2; } }
        public T3 I3 { get { return _i3; } }
        public T4 I4 { get { return _i4; } }

        public bool Equals(ValueTuple<T1, T2, T3, T4> other) {
            return T1Comparer.Equals(_i1, other._i1)
                && T2Comparer.Equals(_i2, other._i2)
                && T3Comparer.Equals(_i3, other._i3)
                && T4Comparer.Equals(_i4, other._i4);
        }

        public override bool Equals(object obj) {
            return obj is ValueTuple<T1, T2, T3, T4> && Equals((ValueTuple<T1, T2, T3, T4>)obj);
        }
        public override int GetHashCode() {
            return unchecked((((T1Comparer.GetHashCode(_i1) * (int)0xA5555529) + T2Comparer.GetHashCode(_i2) * (int)0xA5555529) + T3Comparer.GetHashCode(_i3)) * (int)0xA5555529) + T4Comparer.GetHashCode(_i4);
        }

        public static bool operator ==(ValueTuple<T1, T2, T3, T4> left, ValueTuple<T1, T2, T3, T4> right) {
            return left.Equals(right);
        }
        public static bool operator !=(ValueTuple<T1, T2, T3, T4> left, ValueTuple<T1, T2, T3, T4> right) {
            return !left.Equals(right);
        }
    }

}
