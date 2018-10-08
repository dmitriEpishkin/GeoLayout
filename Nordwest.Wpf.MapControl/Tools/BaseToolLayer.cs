using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using GMap.NET.WindowsPresentation;

namespace Nordwest.Wpf.Controls.Tools {
    public abstract class BaseToolLayer : Control, INotifyPropertyChanged {

        private bool _isActive;
        private bool _subscribed;

        protected MapControl _mapControl;
        protected GMapControl _gMap;

        protected abstract void SubscribeToMap();
        protected abstract void UnsubscribeFromMap();

        protected abstract void RefreshElements();
        protected abstract void Reset();

        private void SubscribeToMapCore() {
            if (_mapControl == null || _gMap == null || _subscribed || !IsActive)
                return;

            SubscribeToMap();

            _subscribed = true;
        }

        private void UnsubscribeFromMapCore() {
            if (_mapControl == null || _gMap == null || !_subscribed)
                return;

            UnsubscribeFromMap();

            _subscribed = false;
        }

        public MapControl MapControl {
            get => _mapControl;
            set {
                if (_mapControl != value) {
                    if (_mapControl != null)
                        _mapControl.GMapControlSet -= _mapControl_GMapControlSet;
                    UnsubscribeFromMapCore();

                    _mapControl = value;
                    _gMap = _mapControl.GMapControl;
                    _mapControl.Tools.Add(this);

                    if (_mapControl != null)
                        _mapControl.GMapControlSet += _mapControl_GMapControlSet;
                    SubscribeToMapCore();

                    OnPropertyChanged(nameof(MapControl));
                }
            }
        }

        private void _mapControl_GMapControlSet(object sender, EventArgs e) {
            _gMap = _mapControl.GMapControl;
            SubscribeToMapCore();
        }

        public bool IsActive {
            get => _isActive;
            set {
                if (_isActive != value) {
                    _isActive = value;

                    if (IsActive)
                        SubscribeToMapCore();
                    else {
                        Reset();
                        UnsubscribeFromMapCore();
                    }

                    RefreshElements();

                    OnPropertyChanged(nameof(IsActive));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
