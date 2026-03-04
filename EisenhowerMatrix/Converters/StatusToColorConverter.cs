using System.Globalization;
using System.Windows.Data;
using AntDesign.WPF.Colors;
using EisenhowerMatrix.Models;

namespace EisenhowerMatrix.Converters;

public class StatusToColorConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TaskItemStatus status)
        {
            // When used with AntDesign Tag PresetColor
            if (targetType == typeof(PresetColor?) || parameter is string s && s == "PresetColor")
            {
                return status switch
                {
                    TaskItemStatus.NotStarted => PresetColor.Cyan,
                    TaskItemStatus.InProgress => PresetColor.Blue,
                    TaskItemStatus.Completed => PresetColor.Green,
                    TaskItemStatus.Blocked => PresetColor.Red,
                    _ => (PresetColor?)null
                };
            }

            // When used with SolidColorBrush (for other contexts)
            return status switch
            {
                TaskItemStatus.NotStarted => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x9C, 0xA3, 0xAF)),
                TaskItemStatus.InProgress => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x3B, 0x82, 0xF6)),
                TaskItemStatus.Completed => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x10, 0xB9, 0x81)),
                TaskItemStatus.Blocked => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xEF, 0x44, 0x44)),
                _ => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray)
            };
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
