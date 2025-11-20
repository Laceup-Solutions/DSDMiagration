using System.Globalization;
using Microsoft.Maui.Controls;

namespace LaceupMigration.Converters
{
    public class CreditTypeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool damaged)
            {
                return damaged ? "Dump" : "Return";
            }
            return value?.ToString() ?? string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return str.ToLowerInvariant() == "dump";
            }
            return false;
        }
    }
}

