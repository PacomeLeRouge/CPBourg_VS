using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using CPBourg.NextGenGui.Models;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>
    /// Maps <see cref="ErrorSeverity"/> to the theme's status brushes,
    /// reusing the same red/amber/blue/green tokens already used elsewhere
    /// (Stop button, Settings' Unsaved banner, Idle machine tiles, Preferences
    /// Saved banner) rather than introducing new colours for this screen.
    /// Kept in Views (not Models) so the model stays WPF-free (NFR-02) - same
    /// reasoning as MachineStatusToBrushConverter.
    /// </summary>
    public sealed class ErrorSeverityToBrushConverter : IValueConverter
    {
        /// <summary>If true, returns the pale background brush; otherwise the solid foreground/icon brush.</summary>
        public bool Background { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ErrorSeverity severity))
            {
                return Application.Current.Resources["TextMutedBrush"];
            }

            string key;
            switch (severity)
            {
                case ErrorSeverity.Critical:
                    key = Background ? "StatusErrorBgBrush" : "StatusErrorBrush";
                    break;
                case ErrorSeverity.Warning:
                    key = Background ? "WarningBgBrush" : "WarningBrush";
                    break;
                case ErrorSeverity.Info:
                    key = Background ? "StatusIdleBgBrush" : "StatusIdleBrush";
                    break;
                case ErrorSeverity.Resolved:
                default:
                    key = Background ? "StatusRunningBgBrush" : "StatusRunningBrush";
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
