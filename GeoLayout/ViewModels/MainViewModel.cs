
using System.Collections.ObjectModel;
using System.Waf.Applications;
using System.Waf.Foundation;
using GeoLayout.Domain.Data;
using GeoLayout.Domain.IO;
using GeoLayout.Services;

namespace GeoLayout {
    public class MainViewModel : Model {

        private readonly WaypointsService _waypointsService;
        private readonly GroupsService _groupsService;
        private readonly GeoLayoutService _layoutBuilder;

        public MainViewModel(WaypointsService waypointsService, GroupsService groupsService, GeoLayoutService layoutService) {

            _waypointsService = waypointsService;
            _groupsService = groupsService;
            _layoutBuilder = layoutService;

            
            var inputFile = @"C:\Users\Епишкин Дмитрий\Desktop\ВЛУ\keyPoints2.gpx";
            var importer = new GpxImporter();
            var keyPoints = importer.ImportWaypoints(inputFile);

            var pp = importer.ImportWaypoints(@"C:\Users\Епишкин Дмитрий\Desktop\ВЛУ\ПП.gpx");
            var newPP = pp.FindAll(p => int.Parse(p.Name) > 54730000); //&& int.Parse(p.Name) % 2 == 0);
            newPP.Sort((p1, p2) => p1.Name.CompareTo(p2.Name));
            for (int i = 1; i < newPP.Count; i++) {
                if (newPP[i].Name == newPP[i - 1].Name) {
                    newPP.RemoveAt(i);
                    i--;
                }
            }
            new GpxExporter().ExportWaypoints(@"C:\Users\Епишкин Дмитрий\Desktop\ВЛУ\ПП-Север-5.gpx", newPP);

            //var group = new Group("Группа 1");

            foreach (var p in keyPoints) {
                _waypointsService.Waypoints.Add(p);
                //group.Waypoints.Add(new WaypointGroupWrapper(group, p));
            }

            //_groupsService.Groups.Add(group);

        }

        public DelegateCommand RemoveCommand { get; }

        public WaypointsService WaypointsService => _waypointsService;
        public GroupsService GroupsService => _groupsService;
        public GeoLayoutService BuilderService => _layoutBuilder;

    }
}
