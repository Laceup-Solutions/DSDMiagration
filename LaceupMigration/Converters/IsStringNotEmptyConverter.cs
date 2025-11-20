using System.Globalization;
using Microsoft.Maui.Controls;

namespace LaceupMigration.Converters
{
    /// <summary>
    /// Converter that returns true if the string is not null or empty, false otherwise.
    /// Used to hide group headers when CompanyName is empty (matches Xamarin behavior).
    /// </summary>
    public class IsStringNotEmptyConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return !string.IsNullOrEmpty(str);
            }
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

