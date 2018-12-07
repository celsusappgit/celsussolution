using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Celsus.Client.Wpf.Types.Converters
{
    public class VisibilityInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Visible;
            }
            var visibilityValue = (Visibility)value;
            switch (visibilityValue)
            {
                case Visibility.Visible:
                    return Visibility.Collapsed;
                case Visibility.Hidden:
                    return Visibility.Visible;
                case Visibility.Collapsed:
                    return Visibility.Visible;
                default:
                    break;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
