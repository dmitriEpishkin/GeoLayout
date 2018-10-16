
using System.ComponentModel;
using System.Runtime.CompilerServices;
using GeoLayout.Domain.Data;

namespace GeoLayout.Domain.LayoutTemplates {
    public class TdmLayoutTemplate : LayoutTemplate, INotifyPropertyChanged {

        private double _sourceSizeMeters = 200;
        private readonly GeoOffset _sourceOffset = new GeoOffset(0, 0);

        private double _receiverSizeMeters = 50;
        private readonly GeoOffset _receiverOffset = new GeoOffset(0, 0);

        public TdmLayoutTemplate() {

            UpdateLayoutTemplate();

            SourceOffset.PropertyChanged += (sender, args) => UpdateLayoutTemplate();
            ReceiverOffset.PropertyChanged += (sender, args) => UpdateLayoutTemplate();
        }

        protected override void UpdateLayoutTemplate() {

            var sourceHalf = _sourceSizeMeters / 2;

            Elements.Clear();
            Elements.Add(new LayoutTemplateElement("4", _sourceOffset.AddOffset(-sourceHalf, -sourceHalf)));
            Elements.Add(new LayoutTemplateElement("3", _sourceOffset.AddOffset(sourceHalf, -sourceHalf)));
            Elements.Add(new LayoutTemplateElement("2", _sourceOffset.AddOffset(sourceHalf, sourceHalf)));
            Elements.Add(new LayoutTemplateElement("1", _sourceOffset.AddOffset(-sourceHalf, sourceHalf)));

            if (SourceSizeMeters == ReceiverSizeMeters && SourceOffset.Equals(ReceiverOffset))
                return;

            var receiverHalf = _receiverSizeMeters / 2;

            Elements.Add(new LayoutTemplateElement("8", _receiverOffset.AddOffset(-receiverHalf, -receiverHalf)));
            Elements.Add(new LayoutTemplateElement("7", _receiverOffset.AddOffset(receiverHalf, -receiverHalf)));
            Elements.Add(new LayoutTemplateElement("6", _receiverOffset.AddOffset(receiverHalf, receiverHalf)));
            Elements.Add(new LayoutTemplateElement("5", _receiverOffset.AddOffset(-receiverHalf, receiverHalf)));

        }

        public double SourceSizeMeters {
            get => _sourceSizeMeters;
            set {
                if (_sourceSizeMeters != value && value > 0) {
                    _sourceSizeMeters = value;
                    OnPropertyChanged(nameof(SourceSizeMeters));
                    UpdateLayoutTemplate();
                }
            }
        }

        public GeoOffset SourceOffset => _sourceOffset;

        public double ReceiverSizeMeters {
            get => _receiverSizeMeters;
            set {
                if (_receiverSizeMeters != value && value > 0) {
                    _receiverSizeMeters = value;
                    OnPropertyChanged(nameof(ReceiverSizeMeters));
                    UpdateLayoutTemplate();
                }
            }
        }

        public GeoOffset ReceiverOffset => _receiverOffset;
    
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
