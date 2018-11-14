using System;

namespace GeoLayout.NamingScheme {
    public class ProfileNamingScheme {

        private readonly Func<int, string> _getWaypointName;

        public ProfileNamingScheme(string schemeName, Func<int, string> getWaypointName) {
            SchemeName = schemeName;
            _getWaypointName = getWaypointName;
        }

        public string GetWaypointName(int waypointIndex) {
            return _getWaypointName(waypointIndex);
        }

        public string SchemeName { get; }

    }
}
