using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;

namespace GeoLayout.Domain.Data {
    public class ProfileGroup : LayoutGroup {
        
        public ProfileGroup(string name) : base(name) {
            
        }
        
        protected override void UpdateLayout() {

            if (Children.Count > 1) {
                var shape = new List<GeoLocation> {
                    ((Waypoint) Children[0]).Location,
                    ((Waypoint) Children[Children.Count - 1]).Location
                };

                Shape = new ReadOnlyCollection<GeoLocation>(shape);
            }
            else {
                Shape = new ReadOnlyCollection<GeoLocation>(new List<GeoLocation>());
            }
            
        }
        
    }
}
