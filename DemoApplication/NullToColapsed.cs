using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DemoApplication
{
    public class NullToColapsed : IValueConverter
    {
        public static readonly NullToColapsed Instance = new NullToColapsed();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}