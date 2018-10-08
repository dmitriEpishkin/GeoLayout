
using System.Waf.Foundation;

namespace GeoLayout.Domain.Data {
    public class Waypoint : Model {

        private string _name;
        private GeoLocation _location;

        private bool _isVisible = true;
        private bool _isSelected = false;

        public Waypoint(GeoLocation location) : this("", location) { }

        public Waypoint(string name, GeoLocation location) {
            Name = name;
            Location = location;
        }

        public string Name {
            get => _name;
            set {
                if (_name != value) {
                    _name = value;
                    RaisePropertyChanged(nameof(Name));
                }
            }
        }
        
        public GeoLocation Location {
            get => _location;
            set {
                if (_location != value) {
                    _location = value;
                    RaisePropertyChanged(nameof(Location));
                }
            }
        }

        public bool IsVisible {
            get => _isVisible;
            set {
                if (_isVisible != value) {
                    _isVisible = value;
                    RaisePropertyChanged(nameof(IsVisible));
                    IsSelected &= value;
                }
            }
        }

        public bool IsSelected {
            get => _isSelected;
            set {
                if (_isSelected != value) {
                    _isSelected = value;
                    RaisePropertyChanged(nameof(IsSelected));
                    IsVisible |= value;
                }
            }
        }

    }
}
