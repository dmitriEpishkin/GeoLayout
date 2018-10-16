
using System.Collections.ObjectModel;
using System.Linq;

namespace GeoLayout.Domain.Data {
    public class TdmGroup : LayoutGroup {

        public TdmGroup(string name) : base(name) {

        }

        protected override void UpdateLayout() {
            var shape = Children.OfType<Waypoint>().Select(child => child.Location).ToList();
            Shape = new ReadOnlyCollection<GeoLocation>(shape);
        }

    }
}
