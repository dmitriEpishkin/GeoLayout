
using System.Collections.Generic;
using GeoLayout.Domain.Data;

namespace GeoLayout.Domain.LayoutTemplates {
    public abstract class LayoutTemplate {

        protected readonly List<LayoutTemplateElement> Elements = new List<LayoutTemplateElement>();

        protected abstract void UpdateLayoutTemplate();

        public List<Waypoint> ApplyTo(Waypoint waypoint) {
            var layout = new List<Waypoint>();

            var xyBase = WgsUtmConverter.LatLonToUTMXY(waypoint.Location, 0);

            foreach (var element in Elements) {
                var xy = xyBase.Shift(element.Offset);
                var location = WgsUtmConverter.UTMXYToLatLon(xy, waypoint.Location.Latitude < 0);
                layout.Add(new Waypoint($"{waypoint.Name}-{element.Name}", location));
            }

            return layout;
        }   

    }
}
