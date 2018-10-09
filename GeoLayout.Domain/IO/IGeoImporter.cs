using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoLayout.Domain.Data;

namespace GeoLayout.Domain.IO {
    public interface IGeoImporter {
        IFileTypeInfo GetFileTypeInfo();
        List<Waypoint> ImportWaypoints(string fileName);
    }
}
