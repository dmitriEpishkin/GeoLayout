
using System.Collections.Generic;
using GeoLayout.Domain.Data;

namespace GeoLayout.Domain {
    public static class GridFactory {
        
        public static List<Waypoint> CreateGrid(Waypoint corner, double profileStep, double siteStep, double profileLength, double gridWidth, double profileAngleRad, double gridAngleRad) {

            var starts = ProfileFactory.CreateWithFixedStep(corner, gridWidth, profileStep, gridAngleRad);

            var res = new List<Waypoint>();

            for (int k = 0; k < starts.Count; k++) {

                var profile = ProfileFactory.CreateWithFixedStep(starts[k], profileLength, siteStep, profileAngleRad);

                for (int i = 0; i < profile.Count; i++) {
                    profile[i].Name = k + "-" + i;
                }

                res.AddRange(profile);
            }

            return res;
        }

    }
}
