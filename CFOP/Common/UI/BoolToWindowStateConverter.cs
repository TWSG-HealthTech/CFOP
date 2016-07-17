using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CFOP.Common.UI
{
    public class BoolToWindowStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return ((bool)value) ? WindowState.Maximized : WindowState.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return (WindowState) value == WindowState.Maximized;
        }
    }
}