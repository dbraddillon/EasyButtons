using System.Globalization;

namespace EasyButtons.Helpers;

/// <summary>
/// Converts a hex color string (e.g. "#E53935") to a MAUI Color for use in bindings.
/// </summary>
public class HexToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string hex && Color.TryParse(hex, out var color))
            return color;
        return Color.FromArgb("#E53935");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is Color c ? c.ToArgbHex() : "#E53935";
}
