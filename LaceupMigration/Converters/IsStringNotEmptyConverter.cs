using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

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
    
    /// <summary>
    /// Converter that returns TextDecorations.Underline if value is true, otherwise TextDecorations.None.
    /// </summary>
    public class BoolToTextDecorationsConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
            {
                return TextDecorations.Underline;
            }
            return TextDecorations.None;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    /// <summary>
    /// Converter that returns FontAttributes.Bold if value is true, otherwise FontAttributes.None.
    /// Used to keep Bold when price is not clickable, or add underline when clickable.
    /// </summary>
    public class BoolToUnderlineConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Always return Bold, underline is handled by TextDecorations
            return FontAttributes.Bold;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    /// <summary>
    /// Converter that returns a color based on boolean value.
    /// Parameter format: "TrueColor|FalseColor" (e.g., "#F5F5F5|Transparent")
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter is string paramStr && !string.IsNullOrEmpty(paramStr))
            {
                var colors = paramStr.Split('|');
                if (colors.Length == 2)
                {
                    bool boolValue = false;
                    if (value is bool b)
                        boolValue = b;
                    else if (value is OrderDetail detail && detail != null)
                        boolValue = detail.IsFreeItem;
                    
                    var colorStr = boolValue ? colors[0].Trim() : colors[1].Trim();
                    if (colorStr.Equals("Transparent", StringComparison.OrdinalIgnoreCase))
                        return Microsoft.Maui.Graphics.Colors.Transparent;
                    return Microsoft.Maui.Graphics.Color.FromArgb(colorStr);
                }
            }
            return Microsoft.Maui.Graphics.Colors.Transparent;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    /// <summary>
    /// Converter that checks if an OrderDetail is a free item.
    /// Returns true if Detail is not null and IsFreeItem is true.
    /// If parameter is "Inverted", returns the inverted value.
    /// </summary>
    public class IsNotFreeItemConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isFreeItem = false;
            if (value is OrderDetail detail && detail != null)
            {
                isFreeItem = detail.IsFreeItem;
            }
            
            bool result = !isFreeItem;
            if (parameter is string paramStr && paramStr == "Inverted")
                result = !result;
            
            return result;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    /// <summary>
    /// Converter that returns a RoundRectangle with appropriate corner radius based on HasHeader boolean.
    /// When true (has header), returns rounded bottom corners only (0,0,8,8).
    /// When false (standalone), returns all rounded corners (8).
    /// </summary>
    public class HasHeaderToCornerRadiusConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool hasHeader)
            {
                if (hasHeader)
                {
                    // Has header: rounded bottom corners only (top-left, top-right, bottom-right, bottom-left)
                    return new RoundRectangle { CornerRadius = new CornerRadius(0, 0, 8, 8) };
                }
                else
                {
                    // Standalone: all rounded corners
                    return new RoundRectangle { CornerRadius = new CornerRadius(8) };
                }
            }
            // Default: all rounded corners
            return new RoundRectangle { CornerRadius = new CornerRadius(8) };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    /// <summary>
    /// Converter that returns a Thickness margin based on whether header is empty.
    /// When header is empty, returns no top margin (0,0,0,0).
    /// When header has value, returns top margin (0,8,0,0) to match Xamarin spacing.
    /// </summary>
    public class HeaderToMarginConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string header && !string.IsNullOrEmpty(header))
            {
                // Header has value: add top margin
                return new Thickness(0, 8, 0, 0);
            }
            // Header is empty: no margin
            return new Thickness(0, 0, 0, 0);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    /// <summary>
    /// Converter that returns 0 height when string is empty, otherwise returns -1 (auto).
    /// Used to collapse empty group headers completely.
    /// </summary>
    public class StringToHeightConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string str && !string.IsNullOrEmpty(str))
            {
                return -1.0; // Auto height
            }
            return 0.0; // Zero height for empty strings
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

