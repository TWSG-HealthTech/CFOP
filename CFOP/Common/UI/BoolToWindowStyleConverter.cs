using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CFOP.Common.UI
{
    public class BoolToWindowStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return ((bool)value) ? WindowStyle.ToolWindow : WindowStyle.SingleBorderWindow;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return (WindowStyle)value == WindowStyle.ToolWindow;
        }
    }
}
