using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CPBourg.NextGenGui.Models;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>
    /// Home dashboard. Populated with sample data for the prototype. The
    /// counter and line-control buttons (preset +/-, Reset to zero, Set
    /// target, Start / Pause / Stop, Purge) act on local mockup state so the
    /// interface is fully usable with no WFM connection; swap them for
    /// WFM-backed commands once that link exists (FR-03).
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

        /// <summary>Raised when the STFO machine tile is tapped - MainWindow
        /// opens the STFO individual-machine configuration wizard.</summary>
        public event EventHandler NavigateToStfoRequested;

        // Maps a configurable module type (the Machine Line Configuration
        // catalog) to its Home-dashboard tile short code, in display order.
        // A tile is shown online (Running) when its module is on the line and
        // offline (greyed out) otherwise - see SetOnlineModules. Codes match
        // the reference mock (BSF feeder, STFO booklet maker, BSE stacker,
        // TR trimmer).
        private static readonly (string ModuleType, string ShortCode)[] ModuleTiles =
        {
            ("Feeder", "BSF"),
            ("Booklet Maker", "STFO"),
            ("Stacker", "BSE"),
            ("Trimmer", "TR"),
        };

        public DashboardView()
        {
            InitializeComponent();

            // Default to only the Booklet Maker (STFO) online, matching the
            // default machine line. MainWindow re-syncs this from the real
            // line at startup and whenever the line changes.
            SetOnlineModules(new[] { "Booklet Maker" });

            UpdateCounterDisplay();
        }

        /// <summary>
        /// Shows a machine tile online (Running) when its module type is on
        /// the machine line, and offline (greyed out) otherwise. MainWindow
        /// wires this to <see cref="MachineLineConfigurationView.LineChanged"/>
        /// so adding a Feeder / Stacker / Trimmer on the configuration screen
        /// brings the matching tile online here, and removing it greys it out.
        /// </summary>
        public void SetOnlineModules(IEnumerable<string> moduleTypesOnLine)
        {
            var onLine = new HashSet<string>(moduleTypesOnLine ?? Enumerable.Empty<string>());

            var tiles = new List<MachineTileInfo>();
            foreach (var tile in ModuleTiles)
            {
                var status = onLine.Contains(tile.ModuleType)
                    ? MachineStatus.Running
                    : MachineStatus.Offline;
                tiles.Add(new MachineTileInfo(tile.ShortCode, status));
            }

            MachineTilesControl.ItemsSource = tiles;
        }

        // Tapping a machine tile opens that module's configuration. Only the
        // STFO screen exists in this prototype, so only the STFO tile
        // navigates; the other tiles are inert for now.
        private void OnMachineTileClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element &&
                element.DataContext is MachineTileInfo tile &&
                tile.ShortCode == "STFO")
            {
                NavigateToStfoRequested?.Invoke(this, EventArgs.Empty);
            }
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

        // ================= Counter / line controls =================
        //
        // These act on local mockup state only - there is no WFM connection in
        // this build, so instead of the old "not connected to the WFM" stubs
        // the controls now do real, visible work (adjust the counters, change
        // the job status). Swap these for WFM-backed commands once that link
        // exists (FR-03).

        private const int PresetStep = 500;

        private int _completedSets = 5234;
        private int _presetTarget;   // 0 == unlimited (shown as the infinity glyph)

        private void UpdateCounterDisplay()
        {
            string completed = _completedSets.ToString("N0");
            CompletedSetsText.Text = completed;
            MiniCompletedSetsText.Text = completed;
            UpdatePresetDisplay();
        }

        private void UpdatePresetDisplay()
        {
            string target = _presetTarget == 0 ? "\u221E" : _presetTarget.ToString("N0");
            PresetValueText.Text = "0 / " + target;
            MiniPresetText.Text = target;
        }

        private void SetJobStatus(string label, string foregroundKey, string backgroundKey)
        {
            JobStatusText.Text = label;
            JobStatusText.Foreground = (Brush)FindResource(foregroundKey);
            JobStatusPill.Background = (Brush)FindResource(backgroundKey);
            ShowAction("Line " + label.ToLowerInvariant() + ".");
        }

        private void ShowAction(string message)
        {
            LastActionText.Text = message;
        }

        private void OnCounterDecrementClick(object sender, RoutedEventArgs e)
        {
            _presetTarget = Math.Max(0, _presetTarget - PresetStep);
            UpdatePresetDisplay();
            ShowAction(_presetTarget == 0
                ? "Preset cleared (no limit)."
                : "Preset set to " + _presetTarget.ToString("N0") + " sets.");
        }

        private void OnCounterIncrementClick(object sender, RoutedEventArgs e)
        {
            _presetTarget += PresetStep;
            UpdatePresetDisplay();
            ShowAction("Preset set to " + _presetTarget.ToString("N0") + " sets.");
        }

        private void OnResetToZeroClick(object sender, RoutedEventArgs e)
        {
            _completedSets = 0;
            UpdateCounterDisplay();
            ShowAction("Completed sets reset to zero.");
        }

        private void OnSetTargetClick(object sender, RoutedEventArgs e)
        {
            // Set a production target just above the current count (next 1,000).
            _presetTarget = ((_completedSets / 1000) + 1) * 1000;
            UpdatePresetDisplay();
            ShowAction("Target set to " + _presetTarget.ToString("N0") + " sets.");
        }

        private void OnNewJobClick(object sender, RoutedEventArgs e) => NavigateToJobsRequested?.Invoke(this, EventArgs.Empty);
        private void OnLoadJobClick(object sender, RoutedEventArgs e) => NavigateToJobsRequested?.Invoke(this, EventArgs.Empty);
        private void OnViewErrorsClick(object sender, RoutedEventArgs e) => NavigateToErrorsRequested?.Invoke(this, EventArgs.Empty);

        private void OnPurgeClick(object sender, RoutedEventArgs e) => ShowAction("Line purged - cleared after jam.");
        private void OnStartClick(object sender, RoutedEventArgs e) => SetJobStatus("Running", "StatusRunningBrush", "StatusRunningBgBrush");
        private void OnPauseClick(object sender, RoutedEventArgs e) => SetJobStatus("Paused", "WarningBrush", "WarningBgBrush");
        private void OnStopClick(object sender, RoutedEventArgs e) => SetJobStatus("Stopped", "StatusErrorBrush", "StatusErrorBgBrush");
    }
}
