using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace TaskamUI.UI.Converters
{
    public class DayToXConverter : IMultiValueConverter
    {
        public double CellWidth { get; set; } = 80;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is DateTime today &&
                values[1] is ObservableCollection<DateTime> dates)
            {
                int index = dates.IndexOf(today.Date);
                if (index >= 0)
                    return index * CellWidth;
            }
            return 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
