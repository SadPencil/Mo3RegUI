using System.Windows;

namespace Mo3RegUI
{
    // https://stackoverflow.com/a/5182660
    public sealed class BooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public BooleanToVisibilityConverter() :
            base(Visibility.Visible, Visibility.Collapsed)
        { }
    }
}
