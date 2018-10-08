using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Waf.Foundation;
using GeoLayout.SettingEnums;

namespace GeoLayout.Services {
    public class SettingsService : Model {

        private CoordinatesRepresentationType _coordinatesRepresentationType = CoordinatesRepresentationType.DegMinSec;

        public CoordinatesRepresentationType CoordinatesRepresentationType {
            get => _coordinatesRepresentationType;
            set {
                if (_coordinatesRepresentationType != value) {
                    _coordinatesRepresentationType = value;
                    RaisePropertyChanged(nameof(CoordinatesRepresentationType));
                }
            }
        }

    }
}
