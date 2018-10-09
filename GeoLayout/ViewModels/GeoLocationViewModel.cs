
using System;
using System.Waf.Foundation;
using System.Windows;
using GeoLayout.Domain;
using GeoLayout.Domain.Data;
using GeoLayout.Services;
using GeoLayout.SettingEnums;
using GMap.NET;

namespace GeoLayout.ViewModels {
    public class GeoLocationViewModel : Model {

        private const string N = "N";
        private const string S = "S";
        private const string E = "E";
        private const string W = "W";
        
        private readonly SettingsService _settingsService;

        private GeoLocation _location;
        
        private string _latitude;
        private string _longitude;

        private string _zone;
        private string _x;
        private string _y;

        public GeoLocationViewModel() {
            _settingsService = ((App) Application.Current).SettingsService;

            _settingsService.PropertyChanged += _settingsService_PropertyChanged;
        }

        private void _settingsService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName != "CoordinatesRepresentationType")
                return;
            RaisePropertyChanged(nameof(IsLatLng));
            UpdateRepresentation();
        }

        public void UpdateRepresentation() {

            if (Location == null)
                return;

            switch (_settingsService.CoordinatesRepresentationType) {
                case CoordinatesRepresentationType.Degrees:
                    SetDegreesRepresentation();
                    break;
                case CoordinatesRepresentationType.DegMinSec:
                    SetDegMinSecRepresentation();
                    break;
                case CoordinatesRepresentationType.Meters:
                    SetMetersRepresentation();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool TrySetLocationFromLatLng() {
            switch (_settingsService.CoordinatesRepresentationType) {
                case CoordinatesRepresentationType.Degrees:
                    return TrySetLocationFromDegrees();
                case CoordinatesRepresentationType.DegMinSec:
                    return TrySetLocationFromDegMinSec();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetDegreesRepresentation() {

            var lat = Math.Abs(Location.Latitude);
            var lng = Math.Abs(Location.Longitude);

            var latSign = Location.Latitude > 0 ? N : S;
            var lngSign = Location.Longitude > 0 ? E : W;

            SetLatLngRepresentation(lat.ToString("F5") + " " + latSign, lng.ToString("F5") + " " + lngSign);

        }
   
        private bool TrySetLocationFromDegrees() {
            var latSplit = Latitude.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var lngSplit = Longitude.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (latSplit.Length != 2 || lngSplit.Length != 2)
                return false;

            if (latSplit[1] != N && latSplit[1] != S)
                return false;

            if (lngSplit[1] != E && lngSplit[1] != W)
                return false;

            if (!double.TryParse(latSplit[0], out double lat) || lat < 0)
                return false;

            if (!double.TryParse(lngSplit[0], out double lng) || lng < 0)
                return false;

            Location = new GeoLocation(latSplit[1] == N ? lat : -lat, lngSplit[1] == E ? lng : -lng, Location.Elevation);

            return true;
        }

        private void SetDegMinSecRepresentation() {
            var lat = Math.Abs(Location.Latitude);
            var lng = Math.Abs(Location.Longitude);

            var latSign = Location.Latitude > 0 ? N : S;
            var lngSign = Location.Longitude > 0 ? E : W;

            var latDeg = Math.Truncate(lat);
            var lngDeg = Math.Truncate(lng);

            var latPart = (lat - latDeg) * 60;
            var lngPart = (lng - lngDeg) * 60;

            var latMin = Math.Truncate(latPart);
            var lngMin = Math.Truncate(lngPart);

            var latSec = (latPart - latMin) * 60;
            var lngSec = (lngPart - lngMin) * 60;

            SetLatLngRepresentation($"{latDeg:00}°{latMin:00}'{latSec:00.0}\"{latSign}", $"{lngDeg:000}°{lngMin:00}'{lngSec:00.0}\"{lngSign}");

        }

        private bool TrySetLocationFromDegMinSec() {
            var latSplit = Latitude.Split(new[] { '°', '\'', '"' }, StringSplitOptions.RemoveEmptyEntries);
            var lngSplit = Longitude.Split(new[] { '°', '\'', '"' }, StringSplitOptions.RemoveEmptyEntries);

            if (latSplit.Length != 4 || lngSplit.Length != 4)
                return false;

            if (latSplit[3] != N && latSplit[3] != S)
                return false;

            if (lngSplit[3] != E && lngSplit[3] != W)
                return false;

            if (!int.TryParse(latSplit[0], out int latDeg) || latDeg < 0 || latDeg > 89)
                return false;

            if (!int.TryParse(lngSplit[0], out int lngDeg) || lngDeg < 0 || lngDeg > 179)
                return false;

            if (!int.TryParse(latSplit[1], out int latMin) || latMin < 0 || latMin > 59)
                return false;

            if (!int.TryParse(lngSplit[1], out int lngMin) || lngMin < 0 || lngMin > 59)
                return false;

            if (!double.TryParse(latSplit[2], out double latSec) || latSec < 0 || latSec > 59.9)
                return false;

            if (!double.TryParse(lngSplit[2], out double lngSec) || lngSec < 0 || lngSec > 59.9)
                return false;
            
            var lat = latDeg + latMin / 60.0 + latSec / 3600.0;
            var lng = lngDeg + lngMin / 60.0 + lngSec / 3600.0;

            Location = new GeoLocation(latSplit[3] == N ? lat : -lat, lngSplit[3] == E ? lng : -lng, Location.Elevation);

            return true;
        }

        private void SetMetersRepresentation() {
            var utm = WgsUtmConverter.LatLonToUTMXY(Location, 0);

            SetXYRepresentation(utm.X.ToString("F0"), utm.Y.ToString("F0"), utm.Zone.ToString());
        }

        private bool TrySetLocationFromMeters() {

            if (!int.TryParse(X, out int x) || !int.TryParse(Y, out int y) || !int.TryParse(Zone, out int zone))
                return false;

            if (zone < 1 || zone > 60)
                return false;

            Location = WgsUtmConverter.UTMXYToLatLon(new GeoLocationXY(zone, x, y, 0), Location.Latitude < 0);
            
            return true;
        }

        private void SetLatLngRepresentation(string lat, string lng) {

            if (_latitude != lat) {
                _latitude = lat;
                RaisePropertyChanged(nameof(Latitude));
            }

            if (_longitude != lng) {
                _longitude = lng;
                RaisePropertyChanged(nameof(Longitude));
            }

        }

        private void SetXYRepresentation(string x, string y, string zone) {

            bool xChanged = false;
            bool yChanged = false;
            bool zChanged = false;

            if (_x != x) {
                _x = x;
                xChanged = true;
            }

            if (_y != y) {
                _y = y;
                yChanged = true;
            }

            if (_zone != zone) {
                _zone = zone;
                zChanged = true;
            }

            if (xChanged)
                RaisePropertyChanged(nameof(X));
            if (yChanged)
                RaisePropertyChanged(nameof(Y));
            if (zChanged)
                RaisePropertyChanged(nameof(Zone));

        }

        public GeoLocation Location {
            get => _location;
            set {
                if (!(_location?.EqualsTo(value) ?? value == null)) {
                    _location = value;
                    UpdateRepresentation();
                    RaisePropertyChanged(nameof(Location));
                }
            }
        }

        public bool IsLatLng => _settingsService.CoordinatesRepresentationType != CoordinatesRepresentationType.Meters;

        public string Latitude {
            get => _latitude;
            set {
                if (_latitude != value) {

                    var oldValue = _latitude;
                    _latitude = value;

                    if (!TrySetLocationFromLatLng()) {
                        _latitude = oldValue;
                    }
                    RaisePropertyChanged(nameof(Latitude));
                }
            }
        }

        public string Longitude {
            get => _longitude;
            set {
                if (_longitude != value) {

                    var oldValue = _longitude;
                    _longitude = value;

                    if (!TrySetLocationFromLatLng()) {
                        _longitude = oldValue;
                    }
                    RaisePropertyChanged(nameof(Longitude));
                }
            }
        }

        public string Zone {
            get => _zone;
            set {
                if (_zone != value) {

                    var oldValue = _zone;
                    _zone = value;

                    RaisePropertyChanged(nameof(Zone));

                    if (!TrySetLocationFromMeters()) {
                        _zone = oldValue;
                        RaisePropertyChanged(nameof(Zone));
                    }
                    
                }
            }
        }

        public string X {
            get => _x;
            set {
                if (_x != value) {

                    var oldValue = _x;
                    _x = value;

                    RaisePropertyChanged(nameof(X));

                    if (!TrySetLocationFromMeters()) {
                        _x = oldValue;
                        RaisePropertyChanged(nameof(X));
                    }
                    
                }
            }
        }

        public string Y {
            get => _y;
            set {
                if (_y != value) {

                    var oldValue = _y;
                    _y = value;

                    RaisePropertyChanged(nameof(Y));

                    if (!TrySetLocationFromMeters()) {
                        _y = oldValue;
                        RaisePropertyChanged(nameof(Y));
                    }
                    
                }
            }
        }

    }
}
