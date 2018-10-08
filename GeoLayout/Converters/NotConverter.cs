
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GeoLayout.Converters {
    public class NotConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return Negate(value);
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture) {
            return Negate(value);
        }

        private static object Negate(object value) {
            if (value is bool b)
                return !b;
            if (value is Visibility visibility)
                return visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            throw new ArgumentOutOfRangeException();
        }

    }
}
