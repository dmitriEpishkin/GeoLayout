using System;
using System.Collections.Generic;
using System.Waf.Foundation;
using System.Windows;
using GeoLayout.Domain.Data;

namespace GeoLayout.GeoLayoutTools {
    public class ShiftModifier : Model, IGeoLayoutTool {
        
        private Waypoint _selectedWaypoint;

        private GeoLocation _location;

        public string Name => "Сдвиг";

        public DataTemplate GetTemplate() {
            ResourceDictionary templates = ResourceUtil.GetRelativeResourceDictionary(@"Templates\GeoLayoutToolsTemplate.xaml");
            return (DataTemplate)templates["ShiftModifierTemplate"];
        }

        public void Apply() {
            SelectedWaypoint.Location = Location;
        }

        public Waypoint SelectedWaypoint {
            get => _selectedWaypoint;
            set {
                if (_selectedWaypoint != value) {
                    _selectedWaypoint = value;
                    RaisePropertyChanged(nameof(SelectedWaypoint));
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

    }
}
