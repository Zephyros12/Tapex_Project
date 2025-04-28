using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Tapex_Project.Converters
{
    /// <summary>
    /// NumericUpDown(Value: decimal?) ↔ ViewModel(double) 변환용
    /// </summary>
    public sealed class DoubleToDecimalConverter : IValueConverter
    {
        public object? Convert(object? value,
                               Type targetType,
                               object? parameter,
                               CultureInfo culture)
        {
            return value is double d ? (decimal?)d : null;
        }

        public object? ConvertBack(object? value,
                                   Type targetType,
                                   object? parameter,
                                   CultureInfo culture)
        {
            return value is decimal dec ? (double)dec : 0d;
        }
    }
}
