using Mo3RegUI.Tasks;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Mo3RegUI
{
    public class MessageLevelToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!targetType.IsAssignableFrom(typeof(FontWeight))) { throw new NotImplementedException(); }
            if (value is not MessageLevel level) { throw new NotImplementedException(); }
            return level switch
            {
                MessageLevel.Debug => FontWeights.Normal,
                MessageLevel.Info => FontWeights.Normal,
                MessageLevel.Warning => FontWeights.Normal,
                MessageLevel.Error => FontWeights.Bold,
                MessageLevel.Critical => FontWeights.ExtraBold,
                _ => (object)FontWeights.Normal,
            };
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
