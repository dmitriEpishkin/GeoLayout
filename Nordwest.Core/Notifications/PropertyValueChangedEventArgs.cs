using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Nordwest.Notifications {
    [ComVisible(false)]
    public class PropertyValueChangedEventArgs : PropertyChangedEventArgs {
        private readonly object _oldValue;
        private readonly object _newValue;

        public PropertyValueChangedEventArgs(string propertyName, object oldValue, object newValue)
            : base(propertyName) {

            _oldValue = oldValue;
            _newValue = newValue;
        }

        public object OldValue { get { return _oldValue; } }
        public object NewValue { get { return _newValue; } }
    }
}
