using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using EisenhowerMatrix.Models;

namespace EisenhowerMatrix.Converters;

public class QuadrantToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is QuadrantType quadrant)
        {
            return quadrant switch
            {
                QuadrantType.Q1 => new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)), // Red
                QuadrantType.Q2 => new SolidColorBrush(Color.FromRgb(0xF9, 0x73, 0x16)), // Orange
                QuadrantType.Q3 => new SolidColorBrush(Color.FromRgb(0x3B, 0x82, 0xF6)), // Blue
                QuadrantType.Q4 => new SolidColorBrush(Color.FromRgb(0x9C, 0xA3, 0xAF)), // Gray
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
