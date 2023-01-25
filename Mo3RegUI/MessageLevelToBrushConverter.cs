using Mo3RegUI.Tasks;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Mo3RegUI
{
    public class MessageLevelToBrushConverter : IValueConverter
    {
        // https://stackoverflow.com/a/56828144/7774607
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!targetType.IsAssignableFrom(typeof(Brush))) { throw new NotImplementedException(); }
            if (value is not MessageLevel level) { throw new NotImplementedException(); }
            return level switch
            {
                MessageLevel.Debug => Brushes.Gray,
                MessageLevel.Info => Brushes.Black,
                MessageLevel.Warning => Brushes.DarkRed,
                MessageLevel.Error => Brushes.DarkRed,
                MessageLevel.Critical => Brushes.DarkRed,
                _ => Brushes.Black,
            };
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
