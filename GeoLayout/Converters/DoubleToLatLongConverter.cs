using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using GMap.NET;

namespace GeoLayout {
    public class DoubleToPointLatLngConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values == null || values.Length != 2 || !(values[0] is double) || !(values[1] is double))
                return DependencyProperty.UnsetValue;

            return new PointLatLng((double)values[0], (double)values[1]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            if (!(value is PointLatLng))
                return new[] { DependencyProperty.UnsetValue, DependencyProperty.UnsetValue };

            var point = (PointLatLng)value;
            return new object[] { point.Lat, point.Lng };
        }
    }
}
