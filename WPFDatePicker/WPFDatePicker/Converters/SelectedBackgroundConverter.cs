using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace WPFDatePicker.Converters
{
    public class SelectedBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == DependencyProperty.UnsetValue)
            {
                return DependencyProperty.UnsetValue;
            }

            if (value is bool boolValue)
            {
                if (boolValue == true)
                {
                    return new SolidColorBrush(Colors.DeepSkyBlue);
                }
            }

            return SystemColors.ControlLightBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
