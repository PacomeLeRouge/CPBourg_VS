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
            Visibility = Visibility.Visible;
            _job = job;
            JobNameText.Text = job?.Name ?? "-";
            ExportStatusText.Text = string.Empty;
            ApplyLanguage();
        }

        public void ApplyLanguage()
        {
            LocalizationManager.Apply(this);
            DateTimeColumn.Header = LocalizationManager.Translate("Date & Time");
            ActionColumn.Header = LocalizationManager.Translate("Action");
            DetailsColumn.Header = LocalizationManager.Translate("Details");
            LogGrid.ItemsSource = _job?.LogEntries.Select(entry => new
            {
                entry.TimestampLabel,
                Action = LocalizationManager.Translate(entry.Action),
                Details = LocalizeDetails(_job, entry),
            }).ToList();
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
                Title = LocalizationManager.Translate("Export Job Log"),
                FileName = MakeSafeFileName(_job.Name) + "-log.csv",
                DefaultExt = ".csv",
                Filter = LocalizationManager.Translate("CSV log (*.csv)") + "|*.csv|" +
                    LocalizationManager.Translate("Text log (*.txt)") + "|*.txt|" +
                    LocalizationManager.Translate("All files (*.*)") + "|*.*",
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
                ExportStatusText.Text = string.Format(CultureInfo.CurrentCulture,
                    LocalizationManager.Translate("Log saved to {0}"), dialog.FileName);
                Exported?.Invoke(this, dialog.FileName);
            }
            catch (Exception ex)
            {
                ExportStatusText.Foreground = (System.Windows.Media.Brush)FindResource("StatusErrorBrush");
                ExportStatusText.Text = string.Format(CultureInfo.CurrentCulture,
                    LocalizationManager.Translate("The log could not be saved: {0}"), ex.Message);
            }
        }

        internal static string BuildCsv(JobRecord job, MeasurementUnit unit)
        {
            var lines = new List<string>
            {
                LocalizationManager.Translate("Job Name") + "," + Csv(job.Name),
                LocalizationManager.Translate("Barcode ID") + "," + Csv(job.BarcodeId),
                LocalizationManager.Translate("Format") + "," + Csv(job.Format),
                LocalizationManager.Translate("Dimensions") + " (" + MeasurementFormatter.UnitSymbol(unit) + ")," +
                    Csv(MeasurementFormatter.FormatDimensions(job.WidthMm, job.LengthMm, unit)),
                LocalizationManager.Translate("Pages") + "," + job.Pages.ToString(CultureInfo.InvariantCulture),
                string.Empty,
                LocalizationManager.Translate("Timestamp") + "," +
                    LocalizationManager.Translate("Action") + "," +
                    LocalizationManager.Translate("Details"),
            };
            lines.AddRange(job.LogEntries.Select(entry =>
                Csv(entry.TimestampLabel) + "," +
                Csv(LocalizationManager.Translate(entry.Action)) + "," +
                Csv(LocalizeDetails(job, entry))));
            return string.Join(Environment.NewLine, lines);
        }

        private static string LocalizeDetails(JobRecord job, JobLogEntry entry)
        {
            if (entry.Action == "Job saved")
            {
                return string.Format(CultureInfo.CurrentCulture,
                    LocalizationManager.Translate("{0}, {1} pages"),
                    LocalizationManager.Translate(job.Format), job.Pages);
            }

            if (entry.Action == "Job loaded")
            {
                return LocalizationManager.Translate("Loaded as the current production job.");
            }

            if (entry.Action == "Barcode scanned")
            {
                return string.Format(CultureInfo.CurrentCulture,
                    LocalizationManager.Translate("Barcode {0} matched this saved job."),
                    job.BarcodeId);
            }

            // Comment text is operator-authored data and must not be translated.
            return entry.Action == "Comment updated"
                ? entry.Details
                : LocalizationManager.Translate(entry.Details);
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
