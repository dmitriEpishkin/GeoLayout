using System;

namespace Nordwest {
    public class Accessor<T1, T2> : GetAccessor<T1, T2> {
        public Accessor(Func<T1, T2> get, Action<T1, T2> set)
            : base(get) {
            Requires.NotNull(set);
            Set = set;
        }

        public Action<T1, T2> Set { get; }
    }

    public class GetAccessor<T1, T2> {
        public GetAccessor(Func<T1, T2> get) {
            Requires.NotNull(get);
            Get = get;
        }

        public Func<T1, T2> Get { get; }
    }
}
