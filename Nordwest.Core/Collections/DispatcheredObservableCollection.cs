
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace FieldSuite.MtDataManager.Tools {
    public class DispatcheredObservableCollection<T> : ObservableCollection<T> {
        public void AddInvoke(T item) {
            Application.Current.Dispatcher.Invoke(new Action(() => Add(item)));
        }

        public void RemoveInvoke(T item) {
            Application.Current.Dispatcher.Invoke(new Action(() => Remove(item)));
        }

        public void ClearInvoke() {
            Application.Current.Dispatcher.Invoke(new Action(Clear));
        }
    }
}
