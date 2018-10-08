using System;
using System.Collections.Generic;

namespace Nordwest {
    public struct Range<T> where T : struct, IEquatable<T>, IComparable<T> {
        public static Range<T> Empty = default(Range<T>);

        private static readonly Comparer<T> DefaultComparer = Comparer<T>.Default;

        public Range(T minimum, T maximum) {
            Requires.Argument(DefaultComparer.Compare(minimum, maximum) <= 0);

            Minimum = minimum;
            Maximum = maximum;
        }

        public T Minimum { get; }
        public T Maximum { get; }

        public override bool Equals(object obj) {
            return obj is Range<T> && Equals((Range<T>)obj);
        }
        public bool Equals(Range<T> range) {
            return Equals(this, range);
        }
        public static bool Equals(Range<T> range1, Range<T> range2) {
            return range1.Minimum.Equals(range2.Minimum) && range1.Maximum.Equals(range2.Maximum);
        }

        public override int GetHashCode() {
            return unchecked(Minimum.GetHashCode() * (int)0xA5555529 + Maximum.GetHashCode());
        }

        public static bool operator ==(Range<T> range1, Range<T> range2) {
            return Equals(range1, range2);
        }
        public static bool operator !=(Range<T> range1, Range<T> range2) {
            return !(range1 == range2);
        }
    }
}
