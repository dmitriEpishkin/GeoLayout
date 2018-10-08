using System;
using System.Collections.Generic;

namespace Nordwest.TimeSeries {
    public class TimeSeriesBase : ITimeSeries<float> {
        private readonly float _sampleRate;
        private readonly DateTime _startTime;
        private readonly IList<float> _data;

        public TimeSeriesBase(float sampleRate, DateTime startTime, IList<float> data) {
            _sampleRate = sampleRate;
            _startTime = startTime;
            _data = data;
        }

        public float SampleRate {
            get { return _sampleRate; }
        }
        public DateTime StartTime {
            get { return _startTime; }
        }
        public IList<float> Data {
            get { return _data; }
        }
    }
}
