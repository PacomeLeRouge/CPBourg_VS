using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using CPBourg.NextGenGui.Models;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>
    /// Maps <see cref="MachineStatus"/> to the theme's status brushes.
    /// Kept in the Views layer (not Models) so the model types have no
    /// dependency on WPF/System.Windows.Media.
    /// </summary>
    public sealed class MachineStatusToBrushConverter : IValueConverter
    {
        /// <summary>If true, returns the pale background brush; otherwise the solid foreground/dot brush.</summary>
        public bool Background { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is MachineStatus status))
            {
                return Application.Current.Resources["TextMutedBrush"];
            }

            string key;
            switch (status)
            {
                case MachineStatus.Running:
                    key = Background ? "StatusRunningBgBrush" : "StatusRunningBrush";
                    break;
                case MachineStatus.Idle:
                    key = Background ? "StatusIdleBgBrush" : "StatusIdleBrush";
                    break;
                case MachineStatus.Offline:
                default:
                    key = Background ? "StatusOfflineBgBrush" : "StatusOfflineBrush";
                    break;
            }

            return (Brush)Application.Current.Resources[key];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
