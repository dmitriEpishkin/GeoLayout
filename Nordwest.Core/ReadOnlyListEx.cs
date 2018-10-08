using System.Collections.Generic;

namespace Nordwest {
    public static class ReadOnlyListEx {
        public static int IndexOf<T>(this IReadOnlyList<T> list, T item) {
            Requires.NotNull(list);

            for (int i = 0; i < list.Count; i++)
                if (Equals(list[i], item))
                    return i;

            return -1;
        }
    }
}
