using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DemoApplication
{
    public class CollapseIfSingleOverloadConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((int)value < 2) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}