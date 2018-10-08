using System;
using System.Collections.Generic;

namespace Nordwest.TimeSeries {
    public interface ITimeSeries<T> {
        IList<T> Data { get; }
        DateTime StartTime { get; }
        float SampleRate { get; }
    }
}
