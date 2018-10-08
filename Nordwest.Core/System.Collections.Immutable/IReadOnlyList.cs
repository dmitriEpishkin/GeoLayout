//using System;
//using System.Runtime.CompilerServices;
namespace System.Collections.Generic {
    //[__DynamicallyInvokable, TypeDependency("System.SZArrayHelper")]
    public interface IReadOnlyList<out T> : IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable {
        //[__DynamicallyInvokable]
        T this[int index] {
            //[__DynamicallyInvokable]
            get;
        }
    }
}
