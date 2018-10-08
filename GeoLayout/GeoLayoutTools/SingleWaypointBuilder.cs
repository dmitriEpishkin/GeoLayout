
using System.Waf.Foundation;
using System.Windows;
using GeoLayout.Domain.Data;
using GeoLayout.Services;

namespace GeoLayout.GeoLayoutTools {
    public class SingleWaypointBuilder : Model, IGeoLayoutTool {
        
        private string _pointName;
        private double _latitude;
        private double _longitude;

        public SingleWaypointBuilder(WaypointsService waypointsService, GroupsService groupsService) {
            WaypointsService = waypointsService;
            GroupsService = groupsService;
        }

        public DataTemplate GetTemplate() {
            ResourceDictionary templates = ResourceUtil.GetRelativeResourceDictionary(@"Templates\GeoLayoutToolsTemplate.xaml");
            return (DataTemplate)templates["SingleWaypointBuilderTemplate"];
        }

        public void SetPosition(double lat, double lng) {

            bool changeLat = false;
            bool changeLng = false;

            if (_latitude != lat) {
                _latitude = lat;
                changeLat = true;
            }

            if (_longitude != lng) {
                _longitude = lng;
                changeLng = true;
            }

            if (changeLat)
                RaisePropertyChanged(nameof(Latitude));
            if (changeLng)
                RaisePropertyChanged(nameof(Longitude));
        }

        public void Apply() {
            WaypointsService.Waypoints.Add(new Waypoint(PointName, new GeoLocation(Latitude, Longitude, 0.0)));
        }
        
        public string Name {
            get => "Добавить точку";
        }

        public string PointName {
            get => _pointName;
            set {
                if (_pointName != value) {
                    _pointName = value;
                    RaisePropertyChanged(nameof(PointName));
                }
            }
        }

        public double Latitude {
            get => _latitude;
            set {
                if (_latitude != value) {
                    _latitude = value;
                    RaisePropertyChanged(nameof(Latitude));
                }
            }
        }

        public double Longitude {
            get => _longitude;
            set {
                if (_longitude != value) {
                    _longitude = value;
                    RaisePropertyChanged(nameof(Longitude));
                }
            }
        }
        
        public WaypointsService WaypointsService { get; }
        public GroupsService GroupsService { get; }

    }
}