using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>
    /// Hides an element when the bound string is null or empty. Used to hide
    /// the flag TextBlock for options that don't have one (e.g. keyboard
    /// layouts, mouse cursor) without needing a WPF-typed property on the
    /// plain <see cref="Models.ChangeOptionInfo"/> model (NFR-02).
    /// </summary>
    public sealed class NullToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
