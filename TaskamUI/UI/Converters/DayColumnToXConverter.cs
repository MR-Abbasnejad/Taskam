using System;
using System.Globalization;
using System.Windows.Data;

namespace TaskamUI.UI.Converters
{
    public class DayColumnToXConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int index = (int)value;

            if (index < 0)
                return -1000;

            return index * 120;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
