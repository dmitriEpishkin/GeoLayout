using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoLayout.Domain.Data {
    public class GeoLocationXY {

        private int _zone;

        private int _x;
        private int _y;
        private int _elevation;

        public GeoLocationXY(int zone, int x, int y, int elevation) {
            _zone = zone;
            _x = x;
            _y = y;
            _elevation = elevation;
        }

        public int Zone => _zone;

        public int X => _x;
        public int Y => _y;
        public int Elevation => _elevation;

    }
}
