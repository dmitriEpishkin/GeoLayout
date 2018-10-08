using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using GeoLayout.Domain.Data;

namespace GeoLayout.InteractiveMap {
    public class InteractivePoint : INotifyPropertyChanged {

        private Point _point;
        private Control _control;

        public InteractivePoint(Point point, Control control) {
            _point = point;
            _control = control;
        }

        public Point Point {
            get => _point;
            set {
                if (_point != value) {
                    _point = value;
                    OnPropertyChanged();
                }
            }
        }

        public Control Control => _control;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
