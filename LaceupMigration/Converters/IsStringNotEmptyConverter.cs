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
    /// Converter that returns expand/collapse icon character based on boolean value.
    /// Returns "▼" when true (expanded), "▶" when false (collapsed).
    /// </summary>
    public class ExpandIconConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "▼" : "▶";
            }
            return "▶";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
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
    
    /// <summary>
    /// Converter that returns ColumnDefinitionCollection based on boolean value.
    /// When true (showing routes), returns 4 columns: Auto,Auto,*,Auto (badge, icon, content, balance).
    /// When false (showing all), returns 2 columns: *,Auto (content, balance).
    /// </summary>
    public class HasLeftContentToColumnDefinitionsConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var columnDefinitions = new ColumnDefinitionCollection();
            
            if (value is bool hasLeftContent && hasLeftContent)
            {
                // Showing routes: badge, icon, content, balance
                columnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                columnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                columnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                columnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            }
            else
            {
                // Showing all: content, balance
                columnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                columnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            }
            
            return columnDefinitions;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    /// <summary>
    /// Converter that returns the column index for the content (name/address) grid.
    /// When true (showing routes), returns 2 (after badge and icon).
    /// When false (showing all), returns 0 (first column).
    /// </summary>
    public class HasLeftContentToContentColumnConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool hasLeftContent && hasLeftContent)
                return 2;
            return 0;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    /// <summary>
    /// Converter that returns the column index for the balance label.
    /// When true (showing routes), returns 3 (last column).
    /// When false (showing all), returns 1 (second column).
    /// </summary>
    public class HasLeftContentToBalanceColumnConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool hasLeftContent && hasLeftContent)
                return 3;
            return 1;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    /// <summary>
    /// Converter that returns the column span for the separator BoxView.
    /// When true (showing routes), returns 4 (all columns).
    /// When false (showing all), returns 2 (both columns).
    /// </summary>
    public class HasLeftContentToColumnSpanConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool hasLeftContent && hasLeftContent)
                return 4;
            return 2;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

