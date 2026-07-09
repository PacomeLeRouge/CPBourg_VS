using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CPBourg.NextGenGui.Models;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>Error/warning detail overlay - see ErrorDetailDialog.xaml.</summary>
    public partial class ErrorDetailDialog : UserControl
    {
        /// <summary>Raised when Clear is clicked, with the record to remove from the list.</summary>
        public event EventHandler<ErrorRecord> Cleared;

        private ErrorRecord _current;

        public ErrorDetailDialog()
        {
            InitializeComponent();
        }

        public void Open(ErrorRecord record)
        {
            _current = record;

            TitleText.Text = "Error: " + record.Details;
            SourceText.Text = record.Source;
            RelatedModuleText.Text = record.RelatedModule;
            RelatedJobText.Text = record.RelatedJob;
            DescriptionText.Text = record.Description;
            SeverityBadgeText.Text = record.SeverityLabel;

            Brush fg, bg;
            switch (record.Severity)
            {
                case ErrorSeverity.Warning:
                    fg = (Brush)FindResource("WarningBrush");
                    bg = (Brush)FindResource("WarningBgBrush");
                    break;
                case ErrorSeverity.Info:
                    fg = (Brush)FindResource("StatusIdleBrush");
                    bg = (Brush)FindResource("StatusIdleBgBrush");
                    break;
                case ErrorSeverity.Resolved:
                    fg = (Brush)FindResource("StatusRunningBrush");
                    bg = (Brush)FindResource("StatusRunningBgBrush");
                    break;
                case ErrorSeverity.Critical:
                default:
                    fg = (Brush)FindResource("StatusErrorBrush");
                    bg = (Brush)FindResource("StatusErrorBgBrush");
                    break;
            }

            SeverityIconText.Foreground = fg;
            SeverityIconBg.Background = bg;
            SeverityBadge.Background = bg;
            SeverityBadgeText.Foreground = fg;

            Visibility = Visibility.Visible;
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            if (_current != null)
            {
                Cleared?.Invoke(this, _current);
            }
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }

        private void OnScrimMouseDown(object sender, MouseButtonEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }
    }
}
