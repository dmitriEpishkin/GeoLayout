using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using GeoLayout.Domain.Data;

namespace GeoLayout.Domain.IO {
    public class GpxExporter : IGeoExporter {

        public void ExportWaypoints(string fileName, List<Waypoint> points) {
            
            var root = new XElement(@"gpx");

            foreach (var p in points) {

                var wpt = new XElement(@"wpt",
                    new XAttribute(@"lat", p.Location.Latitude), new XAttribute(@"lon", p.Location.Longitude),
                    new XElement(@"name", p.Name));

                root.Add(wpt);

            }

            var doc = new XDocument(root);

            doc.Save(fileName);
        }

    }
}
