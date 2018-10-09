
using System.Collections.Generic;
using System.Waf.Applications.Services;
using System.Xml.Linq;
using GeoLayout.Domain.Data;

namespace GeoLayout.Domain.IO {
    public class GpxExporter : IGeoExporter {

        private readonly FileType _fileType = new FileType("GPX файл", ".GPX");

        public FileType GetFileType() {
            return _fileType;
        }

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
