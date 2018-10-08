using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nordwest.Wpf.Controls.Layers {
    public interface IGMapElementsLayer {

        void ResetLayer();
        void UpdateClusters();

        int ElementsCount { get; }
        int LastElementIndex { get; set; }

        MapControl MapControl { get; set; }
    }
}
