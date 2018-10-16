
namespace GeoLayout.Domain.Data {
    public class LayoutTemplateElement {

        public LayoutTemplateElement(string name, GeoOffset offset) {
            Name = name;
            Offset = offset;
        }

        public string Name { get; }
        public GeoOffset Offset { get; }

    }
}
