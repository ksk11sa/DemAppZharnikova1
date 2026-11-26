using System;
using System.Globalization;
using System.Windows.Data;

namespace DemAppZharnikova
{
    public class DiscountToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int discount)
            {
                return discount > 15; 
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}