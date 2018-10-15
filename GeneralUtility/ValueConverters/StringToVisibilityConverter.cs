using System;
using System.Globalization;
using System.Windows;

namespace GeneralUtility
{
    class StringToVisibilityConverter : BaseValueConverter<StringToVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;

            if ((string) value=="")
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
