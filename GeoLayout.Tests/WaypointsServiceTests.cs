
using GeoLayout.Domain.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeoLayout.Tests {
    [TestClass]
    public class WaypointsServiceTests {
       
        [TestMethod]
        public void AddAndRemoveToWaypointsTest() {

            var waypointsService = new WaypointsService();

            var selected1 = new Waypoint("", new GeoLocation(1, 1, 1)) {
                IsSelected = true
            };

            var selected2 = new Waypoint("", new GeoLocation(1, 1, 1)) {
                IsSelected = true
            };

            var visible1 = new Waypoint("", new GeoLocation(1, 1, 1)) {
                IsVisible = true
            };
            
            var visible2 = new Waypoint("", new GeoLocation(1, 1, 1)) {
                IsVisible = true
            };

            var wpt1 = new Waypoint("", new GeoLocation(1, 1, 1)) { IsVisible = false };
            var wpt2 = new Waypoint("", new GeoLocation(1, 1, 1)) { IsVisible = false };
            
            waypointsService.Waypoints.Add(selected1);

            Assert.AreEqual(1, waypointsService.Waypoints.Count);
            Assert.AreEqual(1, waypointsService.VisibleWaypoints.Count);
            Assert.AreEqual(1, waypointsService.SelectedWaypoints.Count);

            waypointsService.Waypoints.Add(visible1);

            Assert.AreEqual(2, waypointsService.Waypoints.Count);
            Assert.AreEqual(2, waypointsService.VisibleWaypoints.Count);
            Assert.AreEqual(1, waypointsService.SelectedWaypoints.Count);

            waypointsService.Waypoints.Add(wpt1);

            Assert.AreEqual(3, waypointsService.Waypoints.Count);
            Assert.AreEqual(2, waypointsService.VisibleWaypoints.Count);
            Assert.AreEqual(1, waypointsService.SelectedWaypoints.Count);

            waypointsService.Waypoints.Add(selected2);
            waypointsService.Waypoints.Add(visible2);
            waypointsService.Waypoints.Add(wpt2);

            Assert.AreEqual(6, waypointsService.Waypoints.Count);
            Assert.AreEqual(4, waypointsService.VisibleWaypoints.Count);
            Assert.AreEqual(2, waypointsService.SelectedWaypoints.Count);

            waypointsService.Waypoints.Remove(wpt1);

            Assert.AreEqual(5, waypointsService.Waypoints.Count);
            Assert.AreEqual(4, waypointsService.VisibleWaypoints.Count);
            Assert.AreEqual(2, waypointsService.SelectedWaypoints.Count);

            waypointsService.Waypoints.Remove(visible1);

            Assert.AreEqual(4, waypointsService.Waypoints.Count);
            Assert.AreEqual(3, waypointsService.VisibleWaypoints.Count);
            Assert.AreEqual(2, waypointsService.SelectedWaypoints.Count);

            waypointsService.Waypoints.Remove(selected1);

            Assert.AreEqual(3, waypointsService.Waypoints.Count);
            Assert.AreEqual(2, waypointsService.VisibleWaypoints.Count);
            Assert.AreEqual(1, waypointsService.SelectedWaypoints.Count);

            waypointsService.Waypoints.Clear();

            Assert.AreEqual(0, waypointsService.Waypoints.Count);
            Assert.AreEqual(0, waypointsService.VisibleWaypoints.Count);
            Assert.AreEqual(0, waypointsService.SelectedWaypoints.Count);

        }

        [TestMethod]
        public void RemoveCommandTest() {

            var waypointsService = new WaypointsService();

            var selected1 = new Waypoint("", new GeoLocation(1, 1, 1)) {
                IsSelected = true
            };

            var selected2 = new Waypoint("", new GeoLocation(1, 1, 1)) {
                IsSelected = true
            };

            var visible1 = new Waypoint("", new GeoLocation(1, 1, 1)) {
                IsVisible = true
            };

            var visible2 = new Waypoint("", new GeoLocation(1, 1, 1)) {
                IsVisible = true
            };

            var wpt1 = new Waypoint("", new GeoLocation(1, 1, 1)) { IsVisible = false };
            var wpt2 = new Waypoint("", new GeoLocation(1, 1, 1)) { IsVisible = false };

            waypointsService.Waypoints.Add(selected1);
            waypointsService.Waypoints.Add(selected2);
            waypointsService.Waypoints.Add(visible1);
            waypointsService.Waypoints.Add(visible2);
            waypointsService.Waypoints.Add(wpt1);
            waypointsService.Waypoints.Add(wpt2);

            waypointsService.RemoveCommand.Execute(wpt1);
            waypointsService.RemoveCommand.Execute(selected1);
            waypointsService.RemoveCommand.Execute(visible1);

            Assert.AreEqual(3, waypointsService.Waypoints.Count);
            Assert.AreEqual(2, waypointsService.VisibleWaypoints.Count);
            Assert.AreEqual(1, waypointsService.SelectedWaypoints.Count);

        }

    }
}
