using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CPBourg.NextGenGui.Models;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>
    /// Home dashboard. Populated with sample data for the prototype; most
    /// buttons here are intentionally stubs (FR-03 interaction is "Should"
    /// priority and depends on the WFM connection, not yet wired in).
    ///
    /// New Job / Load Job and View Errors are real navigation, not stubs:
    /// they raise <see cref="NavigateToJobsRequested"/> / <see
    /// cref="NavigateToErrorsRequested"/>, which MainWindow.xaml.cs wires to
    /// its shared NavigateTo(...) method - same pattern as the global menu
    /// and the header bell.
    ///
    /// The Active Alerts card reflects the REAL current state of the Errors
    /// & Information screen (see <see cref="UpdateAlertsSummary"/>), rather
    /// than always showing a static "No active alerts" - MainWindow.xaml.cs
    /// wires ErrorsView.MessagesChanged to call this whenever the message
    /// count changes, and calls it once at startup too.
    ///
    /// When real data is available, replace the hard-coded lists below with
    /// values sourced from the WFM/train configuration, keeping the same
    /// binding structure.
    /// </summary>
    public partial class DashboardView : UserControl
    {
        /// <summary>Raised when New Job or Load Job is clicked - MainWindow
        /// navigates to the Jobs / File Menu screen.</summary>
        public event EventHandler NavigateToJobsRequested;

        /// <summary>Raised when View Errors is clicked - MainWindow navigates
        /// to the Errors & Information screen.</summary>
        public event EventHandler NavigateToErrorsRequested;

        public DashboardView()
        {
            InitializeComponent();
            LoadSampleMachines();
        }

        private void LoadSampleMachines()
        {
            // Sample train matching PRD 3.1 (BSF feeder, BME/STFO booklet
            // maker, BSE square edge, BTR trimmer) - shown here with the
            // short codes used in the reference mock.
            var machines = new List<MachineTileInfo>
            {
                new MachineTileInfo("BSF", MachineStatus.Running),
                new MachineTileInfo("STFO", MachineStatus.Running),
                new MachineTileInfo("BSE", MachineStatus.Idle),
                new MachineTileInfo("TR", MachineStatus.Offline),
            };
            MachineTilesControl.ItemsSource = machines;
        }

        /// <summary>
        /// Reflects the real alert counts from the Errors & Information
        /// screen. Shows the plain "No active alerts" / all-clear message
        /// only when <paramref name="total"/> is zero; otherwise shows the
        /// actual counts and a status coloured by the highest severity
        /// present (critical > warning > info).
        /// </summary>
        public void UpdateAlertsSummary(int critical, int warning, int info, int total)
        {
            Brush fg, bg;
            string headline;
            string subtitle;
            string iconGlyph;

            if (total == 0)
            {
                fg = (Brush)FindResource("StatusRunningBrush");
                bg = (Brush)FindResource("StatusRunningBgBrush");
                iconGlyph = "\uE73E";
                headline = "No active alerts";
                subtitle = "All systems are operating normally.";
            }
            else
            {
                iconGlyph = "\uE7BA";
                if (critical > 0)
                {
                    fg = (Brush)FindResource("StatusErrorBrush");
                    bg = (Brush)FindResource("StatusErrorBgBrush");
                }
                else if (warning > 0)
                {
                    fg = (Brush)FindResource("WarningBrush");
                    bg = (Brush)FindResource("WarningBgBrush");
                }
                else
                {
                    fg = (Brush)FindResource("StatusIdleBrush");
                    bg = (Brush)FindResource("StatusIdleBgBrush");
                }

                headline = total + (total == 1 ? " active alert" : " active alerts");

                var parts = new List<string>();
                if (critical > 0) parts.Add(critical + " critical");
                if (warning > 0) parts.Add(warning + " warning" + (warning == 1 ? "" : "s"));
                if (info > 0) parts.Add(info + " info");
                subtitle = string.Join(" \u00b7 ", parts);
            }

            AlertsIconText.Foreground = fg;
            AlertsIconBg.Background = bg;
            AlertsIconText.Text = iconGlyph;
            AlertsHeadlineText.Text = headline;
            AlertsSubtitleText.Text = subtitle;
        }

        private void ShowStub(string actionName)
        {
            LastActionText.Text = "Last action: " + actionName +
                " (stub - not yet connected to the WFM)";
        }

        private void OnCounterDecrementClick(object sender, RoutedEventArgs e) => ShowStub("Decrease preset");
        private void OnCounterIncrementClick(object sender, RoutedEventArgs e) => ShowStub("Increase preset");
        private void OnResetToZeroClick(object sender, RoutedEventArgs e) => ShowStub("Reset to zero");
        private void OnSetTargetClick(object sender, RoutedEventArgs e) => ShowStub("Set target");
        private void OnNewJobClick(object sender, RoutedEventArgs e) => NavigateToJobsRequested?.Invoke(this, EventArgs.Empty);
        private void OnLoadJobClick(object sender, RoutedEventArgs e) => NavigateToJobsRequested?.Invoke(this, EventArgs.Empty);
        private void OnViewErrorsClick(object sender, RoutedEventArgs e) => NavigateToErrorsRequested?.Invoke(this, EventArgs.Empty);
        private void OnPurgeClick(object sender, RoutedEventArgs e) => ShowStub("Purge / clear after jam");
        private void OnStartClick(object sender, RoutedEventArgs e) => ShowStub("Start");
        private void OnPauseClick(object sender, RoutedEventArgs e) => ShowStub("Pause");
        private void OnStopClick(object sender, RoutedEventArgs e) => ShowStub("Stop");
    }
}
