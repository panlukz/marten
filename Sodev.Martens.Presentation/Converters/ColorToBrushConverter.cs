using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Sodev.Marten.Presentation.Converters
{
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stringColor = Enum.GetName(typeof(Common.Color), value);
            return (SolidColorBrush)new BrushConverter().ConvertFromString(stringColor);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
