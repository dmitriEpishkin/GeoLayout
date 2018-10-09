
using System.Collections.Generic;
using GeoLayout.Domain.IO;

namespace GeoLayout.Services {
    public class ImportService {
        
        private readonly List<IGeoImporter> _importers;
        private readonly MultiFileDialogService _multiFileDialogService;
        private readonly WaypointsService _waypointsService;
        
        public ImportService(List<IGeoImporter> importers, MultiFileDialogService multiFileDialogService, WaypointsService waypointsService) {
            _importers = importers;
            _multiFileDialogService = multiFileDialogService;
            _waypointsService = waypointsService;
        }

        public void ImportWaypoints() {

            var fileTypes = _importers.ConvertAll(l => l.GetFileTypeInfo());

            var result = _multiFileDialogService.ShowOpenFileDialog(fileTypes);

            if (result != null && result.FileNames.Length > 0) {
                ImportWaypoints(result.FileNames, result.FileTypeInfo);
            }

        }

        private void ImportWaypoints(IEnumerable<string> fileNames, IFileTypeInfo fileTypeInfo) {

            var importer = _importers.Find(i => i.GetFileTypeInfo() == fileTypeInfo);  

            foreach (var fileName in fileNames) {
                var waypoints = importer.ImportWaypoints(fileName);
                foreach (var wpt in waypoints)
                    _waypointsService.Waypoints.Add(wpt);
            }

        }

    }
}
