using System;
using System.Runtime.CompilerServices;
namespace System.Collections.Generic {
    //[__DynamicallyInvokable, TypeDependency("System.SZArrayHelper")]
    public interface IReadOnlyCollection<out T> : IEnumerable<T>, IEnumerable {
        //[__DynamicallyInvokable]
        int Count {
            //[__DynamicallyInvokable]
            get;
        }
    }
}
