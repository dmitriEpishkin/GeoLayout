using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoLayout.Domain.Data;
using GeoLayout.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeoLayout.Tests {
    [TestClass]
    public class GroupsServiceTests {

        [TestMethod]
        public void AddWaypointToDifferentGroupsTest() {

            var waypointsService = new WaypointsService();
            var groupsService = new GroupsService(waypointsService);

            var wpt = new Waypoint("", GeoLocation.Empty);
            var group1 = new Group("");
            var group2 = new Group("");

            Assert.AreEqual(0, groupsService.Groups.Count);

            groupsService.Groups.Add(group1);
            groupsService.Groups.Add(group2);
            
            Assert.AreEqual(2, groupsService.Groups.Count);

            waypointsService.Waypoints.Add(wpt);

            Assert.IsTrue(groupsService.Groups.Contains(groupsService.DefaultGroup));
            Assert.IsTrue(groupsService.DefaultGroup.Children.Contains(wpt));
            
            group1.Children.Add(wpt);
            //groupsService.DefaultGroup.Children.Remove(wpt);

            Assert.IsFalse(groupsService.Groups.Contains(groupsService.DefaultGroup));
            Assert.IsTrue(waypointsService.Waypoints.Contains(wpt));

            group1.Children.Remove(wpt);

            Assert.IsFalse(waypointsService.Waypoints.Contains(wpt));

        } 

    }
}
