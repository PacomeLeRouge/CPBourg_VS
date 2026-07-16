using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CPBourg.NextGenGui.Models;
using Microsoft.Win32;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>Previews a selected job's event log and exports it as CSV.</summary>
    public partial class JobLogDialog : UserControl
    {
        private JobRecord _job;
        private MeasurementUnit _measurementUnit = MeasurementUnit.Millimeters;

        public event EventHandler<string> Exported;

        public JobLogDialog()
        {
            InitializeComponent();
        }

        public void Open(JobRecord job)
        {
            _job = job;
            JobNameText.Text = job?.Name ?? "-";
            LogGrid.ItemsSource = job?.LogEntries;
            ExportStatusText.Text = string.Empty;
            Visibility = Visibility.Visible;
        }

        public void SetMeasurementUnit(MeasurementUnit unit)
        {
            _measurementUnit = unit;
        }

        private void OnExportClick(object sender, RoutedEventArgs e)
        {
            if (_job == null)
            {
                return;
            }

            var dialog = new SaveFileDialog
            {
                Title = "Export Job Log",
                FileName = MakeSafeFileName(_job.Name) + "-log.csv",
                DefaultExt = ".csv",
                Filter = "CSV log (*.csv)|*.csv|Text log (*.txt)|*.txt|All files (*.*)|*.*",
                AddExtension = true,
                OverwritePrompt = true,
            };

            if (dialog.ShowDialog(Window.GetWindow(this)) != true)
            {
                return;
            }

            try
            {
                File.WriteAllText(dialog.FileName, BuildCsv(_job, _measurementUnit), new UTF8Encoding(true));
                ExportStatusText.Foreground = (System.Windows.Media.Brush)FindResource("StatusRunningBrush");
                ExportStatusText.Text = "Log saved to " + dialog.FileName;
                Exported?.Invoke(this, dialog.FileName);
            }
            catch (Exception ex)
            {
                ExportStatusText.Foreground = (System.Windows.Media.Brush)FindResource("StatusErrorBrush");
                ExportStatusText.Text = "The log could not be saved: " + ex.Message;
            }
        }

        internal static string BuildCsv(JobRecord job, MeasurementUnit unit)
        {
            var lines = new List<string>
            {
                "Job Name," + Csv(job.Name),
                "Barcode ID," + Csv(job.BarcodeId),
                "Format," + Csv(job.Format),
                "Dimensions (" + MeasurementFormatter.UnitSymbol(unit) + ")," +
                    Csv(MeasurementFormatter.FormatDimensions(job.WidthMm, job.LengthMm, unit)),
                "Pages," + job.Pages.ToString(CultureInfo.InvariantCulture),
                string.Empty,
                "Timestamp,Action,Details",
            };
            lines.AddRange(job.LogEntries.Select(entry =>
                Csv(entry.TimestampLabel) + "," + Csv(entry.Action) + "," + Csv(entry.Details)));
            return string.Join(Environment.NewLine, lines);
        }

        private static string Csv(string value)
        {
            return "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
        }

        private static string MakeSafeFileName(string value)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return new string((value ?? "job").Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
        }

        private void OnCloseClick(object sender, RoutedEventArgs e) => Visibility = Visibility.Collapsed;
        private void OnScrimMouseDown(object sender, MouseButtonEventArgs e) => Visibility = Visibility.Collapsed;
    }
}
