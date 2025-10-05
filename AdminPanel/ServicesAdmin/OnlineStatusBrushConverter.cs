using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AdminPanelWPF.ServicesAdmin
{
    public class OnlineStatusBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Простая логика: true - зеленый, false - красный
            if (value is bool isOnline)
            {
                return isOnline ? Brushes.LimeGreen : Brushes.Red;
            }
            return Brushes.Gray; // По умолчанию серый
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}