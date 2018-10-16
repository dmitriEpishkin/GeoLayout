
using System;
using System.Waf.Foundation;

namespace GeoLayout.Domain.Data {
    public class GeoOffset : Model {

        private double _offsetX;
        private double _offsetY;

        public GeoOffset(double offsetXMeters, double offsetYMeters) {
            OffsetXMeters = offsetXMeters;
            OffsetYMeters = offsetYMeters;
        }

        public GeoOffset AddOffset(double xMeters, double yMeters) {
            return new GeoOffset(OffsetXMeters + xMeters, OffsetYMeters + yMeters);
        }

        public GeoOffset AddOffset(GeoOffset offset) {
            return AddOffset(offset.OffsetXMeters, offset.OffsetYMeters);
        }

        public bool Equals(GeoOffset offset) {
            return OffsetXMeters == offset.OffsetXMeters && OffsetYMeters == offset.OffsetYMeters;
        }

        public double OffsetXMeters {
            get => _offsetX;
            set {
                if (_offsetX != value) {
                    _offsetX = value;
                    RaisePropertyChanged(nameof(OffsetXMeters));
                }
            }
        }

        public double OffsetYMeters {
            get => _offsetY;
            set {
                if (_offsetY != value) {
                    _offsetY = value;
                    RaisePropertyChanged(nameof(OffsetYMeters));
                }
            }
        }

    }
}
