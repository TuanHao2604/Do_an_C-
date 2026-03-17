using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace TravelGuideApp.Converters
{
    public class FavoriteTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool flag && flag)
                return "Bỏ yêu thích";
            return "Yêu thích";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }
}
