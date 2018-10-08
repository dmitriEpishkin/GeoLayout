
using System;
using System.Windows;

namespace GeoLayout.InteractiveMap {
    public class MapViewport {

        private Point _modelCenterPoint;
        private double _scale = 1;

        private Rect _model = Rect.Empty;
        private Rect _client = Rect.Empty;
        
        private void UpdateModel() {
            if (_client == Rect.Empty)
                return;

            var width = _client.Width / Scale;
            var height = _client.Height / Scale;

            var x = _modelCenterPoint.X - width / 2;
            var y = _modelCenterPoint.Y - height / 2;
            
            _model = new Rect(x, y, width, height);

            OnModelChanged();
        }
        
        public Point GetModel(Point client) {
            return new Point(_model.X + client.X / Scale, _model.Y + client.Y / Scale);
        }

        public Point GetClient(Point model) {
            return new Point((model.X - _model.X) * Scale, (model.Y - _model.Y) * Scale);
        }

        /// <summary>
        /// Client / Model
        /// </summary>
        public double Scale {
            get => _scale;
            set {
                _scale = value;
                UpdateModel();
            }
        }

        public Point ModelCenterPoint {
            get => _modelCenterPoint;
            set {
                _modelCenterPoint = value;
                UpdateModel();
            }
        }

        public Rect Model => _model;

        public Rect Client {
            get => _client; 
            set {
                _client = value;
                UpdateModel();
            }
        }

        private void OnModelChanged() {
            ModelChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ModelChanged;

    }
}
