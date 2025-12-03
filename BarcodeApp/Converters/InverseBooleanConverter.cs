using System;
using System.Globalization;
using System.Windows.Data;

namespace BarcodeApp.Converters;

public class InverseBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        return value is bool boolValue ? !boolValue : true;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        return value is bool boolValue ? !boolValue : true;
    }
}

