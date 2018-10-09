using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Waf.Applications.Services;
using GeoLayout.Domain.Data;

namespace GeoLayout.Domain.IO {
    public interface IGeoExporter {
        FileType GetFileType();
        void ExportWaypoints(string fileName, List<Waypoint> points);
    }
}
