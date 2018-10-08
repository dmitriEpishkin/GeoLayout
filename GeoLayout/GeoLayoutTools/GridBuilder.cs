using System;
using System.Collections.Generic;
using System.Waf.Foundation;
using System.Windows;
using GeoLayout.Domain;
using GeoLayout.Domain.Data;
using GeoLayout.Services;

namespace GeoLayout.GeoLayoutTools {
    public class GridBuilder : Model, IGeoLayoutTool {

        private string _gridName = "A";

        private Waypoint _corner;
        private double _profileStep = 500;
        private double _siteStep = 100;
        private double _profileLength = 5000;
        private double _gridWidth = 2000;
        private double _profileAngleDeg = 45;
        private double _gridAngleDeg = 90;

        public GridBuilder(WaypointsService waypointsService, GroupsService groupsService) {
            WaypointsService = waypointsService;
            GroupsService = groupsService;
        }

        public DataTemplate GetTemplate() {
            ResourceDictionary templates = ResourceUtil.GetRelativeResourceDictionary(@"Templates\GeoLayoutToolsTemplate.xaml");
            return (DataTemplate)templates["GridBuilderTemplate"];
        }

        public void Apply() {

            var group = new Group(GridName);

            var wpts = GridFactory.CreateGrid(Corner, ProfileStep, SiteStep, ProfileLength, GridWidth, ProfileAngleDeg / 180.0 * Math.PI, GridAngleDeg / 180.0 * Math.PI);

            wpts.ForEach(p => {
                WaypointsService.Waypoints.Add(p);
                group.Waypoints.Add(new WaypointGroupWrapper(group, p));
            });

            GroupsService.Groups.Add(group);
        }

        public void BuildInsidePolygone(List<Waypoint> wpts) {
            
            var group = new Group(GridName);

            var grid = GridFactory.CreateGridInsidePolygone(wpts, ProfileStep, SiteStep, ProfileAngleDeg, GridAngleDeg);

            grid.ForEach(p => {
                WaypointsService.Waypoints.Add(p);
                group.Waypoints.Add(new WaypointGroupWrapper(group, p));
            });

            GroupsService.Groups.Add(group);
        }

        public void SetupWithGridFrame(GridFrame frame) {
            Corner = new Waypoint("", frame.Corner);
            GridWidth = frame.GetGridWidthMeters();
            ProfileLength = frame.GetProfileLengthMeters();

            var c = WgsUtmConverter.LatLonToUTMXY(frame.Corner.Latitude, frame.Corner.Longitude, 0);
            var a1 = WgsUtmConverter.LatLonToUTMXY(frame.P1.Latitude, frame.P1.Longitude, 0);
            var a2 = WgsUtmConverter.LatLonToUTMXY(frame.P2.Latitude, frame.P2.Longitude, 0);
            GridAngleDeg = CalculateAngle(c.Item2, c.Item3, a1.Item2, a1.Item3) * 180.0 / Math.PI;
            ProfileAngleDeg = CalculateAngle(c.Item2, c.Item3, a2.Item2, a2.Item3) * 180.0 / Math.PI;
        }

        private double CalculateAngle(double x1, double y1, double x2, double y2) {
            var dx = x2 - x1;
            var dy = y2 - y1;
            var arctg = Math.Atan(dx/ dy);
            if (dx > 0 && dy > 0)
                return arctg;
            if (dx > 0 && dy < 0)
                return Math.PI + arctg;
            if (dx < 0 && dy > 0)
                return arctg;
            return Math.PI + arctg;
        }

        public string Name => "Построить сетку";

        public string GridName {
            get => _gridName;
            set {
                if (_gridName != value) {
                    _gridName = value;
                    RaisePropertyChanged(nameof(GridName));
                }
            }
        }

        public Waypoint Corner {
            get => _corner;
            set {
                if (_corner != value) {
                    _corner = value;
                    RaisePropertyChanged(nameof(Corner));
                }
            }
        }

        public double ProfileStep {
            get => _profileStep;
            set {
                if (_profileStep != value && value > 0) {
                    _profileStep = value;
                    RaisePropertyChanged(nameof(ProfileStep));
                }
            }
        }

        public double SiteStep {
            get => _siteStep;
            set {
                if (_siteStep != value && value > 0) {
                    _siteStep = value;
                    RaisePropertyChanged(nameof(SiteStep));
                }
            }
        }

        public double ProfileLength {
            get => _profileLength;
            set {
                if (_profileLength != value && value > 0) {
                    _profileLength = value;
                    RaisePropertyChanged(nameof(ProfileLength));
                }
            }
        }

        public double GridWidth {
            get => _gridWidth;
            set {
                if (_gridWidth != value && value > 0) {
                    _gridWidth = value;
                    RaisePropertyChanged(nameof(GridWidth));
                }
            }
        }

        public double ProfileAngleDeg {
            get => _profileAngleDeg;
            set {
                if (_profileAngleDeg != value) {
                    _profileAngleDeg = value;
                    RaisePropertyChanged(nameof(ProfileAngleDeg));
                }
            }
        }

        public double GridAngleDeg {
            get => _gridAngleDeg;
            set {
                if (_gridAngleDeg != value) {
                    _gridAngleDeg = value;
                    RaisePropertyChanged(nameof(GridAngleDeg));
                }
            }
        }

        public WaypointsService WaypointsService { get; }
        public GroupsService GroupsService { get; }

    }
}
