
using System;
using System.Linq;
using System.Waf.Foundation;
using GeoLayout.GeoLayoutTools;

namespace GeoLayout.Services {
    public class ConsoleInterpretatorService : Model {

        private readonly WaypointsService _waypointsService;
        private readonly GroupsService _groupsService;
        private readonly GeoLayoutService _geoLayoutBuilderService;

        public ConsoleInterpretatorService(WaypointsService waypointsService, GroupsService groupsService, GeoLayoutService geoLayoutBuilderService) {
            _waypointsService = waypointsService;
            _groupsService = groupsService;
            _geoLayoutBuilderService = geoLayoutBuilderService;
        }

        public string Process(string line) {
            var split = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            if (split[0] == "Профиль") {

                var tool = new ProfileBuilder(_waypointsService, _groupsService) {
                    StartPoint = _waypointsService.Waypoints.First(p => p.Name == split[1]),
                    LengthMeters = double.Parse(split[2]),
                    StepMeters = double.Parse(split[3]),
                    AngleDeg = double.Parse(split[4]) / 180.0 * Math.PI
                };

                tool.Apply();
                return "Создан новый профиль";
            }

            return "Неизвестная команда";

        } 


    }
}
