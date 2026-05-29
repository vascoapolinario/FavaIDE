using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FavaStudio.Converters;

public class HexColorBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            var text = value?.ToString();
            if (string.IsNullOrWhiteSpace(text))
                return Brushes.Transparent;

            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(text.Trim()));
        }
        catch
        {
            return new SolidColorBrush(Color.FromRgb(255, 93, 93));
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        Binding.DoNothing;
}
