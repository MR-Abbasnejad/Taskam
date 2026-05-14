using System;
using System.Globalization;
using System.Windows.Data;

namespace TaskamUI.UI.Converters
{
    public class RowHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
                return count * 50 + 20; // 50px per row + 20px margin
            return 200;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}