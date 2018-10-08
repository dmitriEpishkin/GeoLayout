using GMap.NET.WindowsPresentation;
using System.ComponentModel;
using System.Windows.Media;

namespace Nordwest.Wpf.Controls {
    public class MapSegment : INotifyPropertyChanged {

        private GMapRoute _route;
        private Brush _brush;
        private bool _isVisible;
        
        public MapSegment(GMapRoute route, Brush brush) {
            _route = route;
            _brush = brush;
        }

        public GMapRoute Route {
            get { return _route; }
            set {
                if (_route != value) {
                    _route = value;
                    OnPropertyChanged(nameof(Route));
                }
            }
        }

        public Brush Brush {
            get { return _brush; }
            set {
                if (_brush != value) {
                    _brush = value;
                    OnPropertyChanged(nameof(Brush));
                }
            }
        }

        public bool IsVisible {
            get { return _isVisible; }
            set {
                if (_isVisible != value) {
                    _isVisible = value;
                    OnPropertyChanged(nameof(IsVisible));
                }
            }
        }

        private void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
