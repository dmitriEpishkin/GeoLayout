
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using GeoLayout.Domain.Data;

namespace GeoLayout.Domain.IO {
    public class GpxImporter : IGeoImporter {

        private readonly IFileTypeInfo _fileTypeInfo = new FileTypeInfo("GPX файл", ".GPX");

        public IFileTypeInfo GetFileTypeInfo() {
            return _fileTypeInfo;
        }

        public List<Waypoint> ImportWaypoints(string fileName) {
            var doc = XDocument.Load(File.OpenRead(fileName));
            var root = doc.Root;
            var el = root.Elements().ToList();

            var points = new List<Waypoint>();

            foreach (var e in el.FindAll(x => x.Name.LocalName == @"wpt")) {

                var p = new Waypoint(e.Elements().First(x => x.Name.LocalName == @"name").Value,
                    new GeoLocation((double)e.Attribute(@"lat"), (double)e.Attribute(@"lon"), 0.0));

                points.Add(p);
            }

            return points;
        }
    }
}
