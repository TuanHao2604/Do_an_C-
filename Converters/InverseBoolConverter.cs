using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace TravelGuideApp.Converters
{
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool flag)
                return !flag;
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool flag)
                return !flag;
            return false;
        }
    }
}
