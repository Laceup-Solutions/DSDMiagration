using System;
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
    
    /// <summary>
    /// Converter that inverts a boolean value.
    /// Used to enable/disable buttons based on inverted boolean properties.
    /// </summary>
    public class InvertedBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return true;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false;
        }
    }
    
    /// <summary>
    /// Converter that returns true if the numeric value is not zero, false otherwise.
    /// Used to conditionally show weight and other numeric fields.
    /// </summary>
    public class IsNotZeroConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return false;
            
            if (value is double d)
                return Math.Abs(d) > 0.0001;
            
            if (value is float f)
                return Math.Abs(f) > 0.0001f;
            
            if (value is int i)
                return i != 0;
            
            if (value is decimal dec)
                return Math.Abs(dec) > 0.0001m;
            
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

