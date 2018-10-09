
using System.Collections.Generic;
using System.Linq;
using GeoLayout.Domain.IO;

namespace GeoLayout.Services {
    public class ExportService {

        private readonly List<IGeoExporter> _exporters;
        private readonly SaveFileDialogService _saveFileDialogService;
        private readonly WaypointsService _waypointsService;

        public ExportService(List<IGeoExporter> exporters, SaveFileDialogService saveFileDialogService, WaypointsService waypointsService) {
            _exporters = exporters;
            _saveFileDialogService = saveFileDialogService;
            _waypointsService = waypointsService;
        }

        public void ExportWaypoints() {
            var fileTypes = _exporters.ConvertAll(l => l.GetFileType());

            var result = _saveFileDialogService.SaveFileDialog(fileTypes, fileTypes[0], "waypoints");

            if (!string.IsNullOrWhiteSpace(result.FileName))
                ExportWaypoints(result);
        }

        private void ExportWaypoints(SaveFileDialogResult result) {
            var exporter = _exporters.Find(e => e.GetFileType() == result.FileType);
            exporter.ExportWaypoints(result.FileName, _waypointsService.Waypoints.ToList());
        }

    }
}
