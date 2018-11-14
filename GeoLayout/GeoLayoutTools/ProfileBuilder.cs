using System;
using System.Collections.Generic;
using System.Linq;
using System.Waf.Foundation;
using System.Windows;
using GeoLayout.Domain;
using GeoLayout.Domain.Data;
using GeoLayout.NamingScheme;
using GeoLayout.Services;

namespace GeoLayout.GeoLayoutTools {
    public class ProfileBuilder : Model, IGeoLayoutTool {

        private Waypoint _startPoint;
       
        private double _step = 100;
        private double _angle = 45;

        private double _length = 1000;
        private int _n = 10;

        private string _profileName = "000";

        private string _prefix = "000-";
        private string _suffix = "";
        private int _digits = 3;

        private int _startIndex = 1;
        private int _indexIncrement = 1;

        private ProfileNamingScheme _selectedNamingScheme;

        private readonly ProfileNamingScheme _prefixSuffixScheme;
        private bool _prefixSuffixSchemeInUse = false;

        public ProfileBuilder(WaypointsService waypointsService, GroupsService groupsService) {
            WaypointsService = waypointsService;
            GroupsService = groupsService;

            _prefixSuffixScheme = new ProfileNamingScheme("[Префикс][Пикет][Суффикс]", index => (Prefix ?? "") + (index + 1).ToString("D" + Digits) + (Suffix ?? ""));

            ProfileNamingSchemes = new[] {
                new ProfileNamingScheme("[Профиль]-[Пикет]", index => (ProfileName ?? "") + "-" + (index + 1).ToString("D" + Digits)),
                new ProfileNamingScheme("[Пикет]-[Профиль]", index => (index + 1).ToString("D" + Digits) + "-" + (ProfileName ?? "")),
                _prefixSuffixScheme
            };

            StartPoint = WaypointsService.Waypoints.FirstOrDefault();
        }
        
        public DataTemplate GetTemplate() {
            ResourceDictionary templates = ResourceUtil.GetRelativeResourceDictionary(@"Templates\GeoLayoutToolsTemplate.xaml");
            return (DataTemplate) templates["ProfileBuilderTemplate"];
        }
        
        public void Apply() {

            var group = new Group(ProfileName);
            var wps = ProfileFactory.CreateWithFixedStep(StartPoint, LengthMeters, StepMeters, AngleDeg / 180.0 * Math.PI, SelectedNamingScheme.GetWaypointName);

            wps.ForEach(p => {
                WaypointsService.Waypoints.Add(p);
                group.Waypoints.Add(new WaypointGroupWrapper(group, p));
            });

            GroupsService.Groups.Add(group);
        }

        public void Build(List<Waypoint> points) {
            var group = new Group(ProfileName);
            var wps = ProfileFactory.CreateWithFixedStep(points, StepMeters, SelectedNamingScheme.GetWaypointName);

            wps.ForEach(p => {
                WaypointsService.Waypoints.Add(p);
                group.Waypoints.Add(new WaypointGroupWrapper(group, p));
            });

            GroupsService.Groups.Add(group);
        }

        public string Name => "Построить профиль";

        public ProfileNamingScheme[] ProfileNamingSchemes { get; }

        public ProfileNamingScheme SelectedNamingScheme {
            get => _selectedNamingScheme;
            set {
                if (_selectedNamingScheme != value) {
                    _selectedNamingScheme = value;
                    RaisePropertyChanged(nameof(SelectedNamingScheme));
                    PrefixSuffixSystemInUse = _selectedNamingScheme == _prefixSuffixScheme;
                }
            }
        }

        public bool PrefixSuffixSystemInUse {
            get => _prefixSuffixSchemeInUse;
            private set {
                if (_prefixSuffixSchemeInUse != value) {
                    _prefixSuffixSchemeInUse = value;
                    RaisePropertyChanged(nameof(PrefixSuffixSystemInUse));
                }
            }
        }

        public Waypoint StartPoint {
            get => _startPoint;
            set {
                if (_startPoint != value) {
                    _startPoint = value;
                    RaisePropertyChanged(nameof(StartPoint));
                }
            }
        }

        public double LengthMeters {
            get => _length;
            set {
                if (_length != value && value > 0) {
                    _length = value;
                    RaisePropertyChanged(nameof(LengthMeters));
                    N = (int) (LengthMeters / StepMeters);
                }
            }
        }

        public double StepMeters {
            get => _step;
            set {
                if (_step != value && value > 0) {
                    _step = value;
                    RaisePropertyChanged(nameof(StepMeters));
                }
            }
        }

        public double AngleDeg {
            get => _angle;
            set {
                if (_angle != value) {
                    _angle = value;
                    RaisePropertyChanged(nameof(AngleDeg));
                }
            }
        }

        public int N {
            get => _n;
            set {
                if (_n != value) {
                    _n = value;
                    RaisePropertyChanged(nameof(N));
                    LengthMeters = StepMeters * N;
                }
            }
        }

        public string ProfileName {
            get => _profileName;
            set {
                if (_profileName != value) {
                    _profileName = value;
                    RaisePropertyChanged(nameof(ProfileName));
                    Prefix = ProfileName + "-";
                }
            }
        }

        public string Prefix {
            get => _prefix;
            set {
                if (_prefix != value) {
                    _prefix = value;
                    RaisePropertyChanged(nameof(Prefix));
                }
            }
        }

        public string Suffix {
            get => _suffix;
            set {
                if (_suffix != value) {
                    _suffix = value;
                    RaisePropertyChanged(nameof(Suffix));
                }
            }
        }

        public int Digits {
            get => _digits;
            set {
                if (_digits != value && value > 0 && value <= 6) {
                    _digits = value;
                    RaisePropertyChanged(nameof(Digits));
                }
            }
        }

        public int StartIndex {
            get => _startIndex;
            set {
                if (_startIndex != value) {
                    _startIndex = value;
                    RaisePropertyChanged(nameof(StartIndex));
                }
            }
        }

        public int IndexIncrement {
            get => _indexIncrement;
            set {
                if (_indexIncrement != value) {
                    _indexIncrement = value;
                    RaisePropertyChanged(nameof(IndexIncrement));
                }
            }
        }
        
        public WaypointsService WaypointsService { get; }
        public GroupsService GroupsService { get; }

    }
}
