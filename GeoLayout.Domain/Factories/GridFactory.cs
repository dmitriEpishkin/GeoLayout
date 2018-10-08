using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoLayout.Domain.Data;
using GeoLayout.Domain.Helpers;

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

        public static List<Waypoint> CreateGridInsidePolygone(List<Waypoint> wpts, double profileStep, double siteStep, double profileAngleRad, double gridAngleRad) {

            var gridFrame = wpts.GetFrame();

            var corner = new Waypoint(gridFrame.Corner);

            var grid = CreateGrid(corner, profileStep, siteStep, gridFrame.GetProfileLengthMeters(), gridFrame.GetGridWidthMeters(), profileAngleRad, gridAngleRad);

            return grid.FindAll(wpt => wpt.IsInside(wpts));
        }
    }
}
