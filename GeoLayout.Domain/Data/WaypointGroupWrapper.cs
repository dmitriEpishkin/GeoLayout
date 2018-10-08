using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoLayout.Domain.Data {
    public class WaypointGroupWrapper {

        public WaypointGroupWrapper(Group group, Waypoint waypoint) {
            Group = group;
            Waypoint = waypoint;

            Waypoint.PropertyChanged += Waypoint_PropertyChanged;
        }

        private void Waypoint_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "IsSelected")
                Group.UpdateSelected();
            if (e.PropertyName == "IsVisible")
                Group.UpdateVisible();
        }

        public Group Group { get; }
        public Waypoint Waypoint { get; }
    }
}
