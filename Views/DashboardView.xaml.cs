using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
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

        private readonly DispatcherTimer _productionTimer;
        private ProductionState _productionState = ProductionState.Ready;
        private int _activeErrorCount;
        private int _criticalAlertCount;
        private int _warningAlertCount;
        private int _infoAlertCount;
        private HashSet<string> _onlineModuleTypes = new HashSet<string>();
        private Func<string> _lastActionRenderer;

        private enum ProductionState
        {
            Ready,
            Running,
            Paused,
            Stopped,
            Completed,
        }

        /// <summary>Initializes sample modules, counter state, and the local production timer.</summary>
        public DashboardView()
        {
            InitializeComponent();

            CounterInputDialog.ValueConfirmed += OnCounterValueConfirmed;
            _productionTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _productionTimer.Tick += OnProductionTimerTick;

            // Default to only the Booklet Maker (STFO) online, matching the
            // default machine line. MainWindow re-syncs this from the real
            // line at startup and whenever the line changes.
            SetOnlineModules(new[] { "Booklet Maker" });

            UpdateCounterDisplay();
            UpdateConfirmedCounterDisplay();
            RefreshProductionButtons();
        }

        /// <summary>
        /// Rebuilds generated dashboard labels and status text after a language
        /// change without altering production or counter state.
        /// </summary>
        public void ApplyLanguage()
        {
            LocalizationManager.Apply(this);
            SetOnlineModules(_onlineModuleTypes);
            UpdateAlertsSummary(
                _criticalAlertCount, _warningAlertCount,
                _infoAlertCount, _activeErrorCount);
            UpdateCounterDisplay();
            UpdateConfirmedCounterDisplay();
            RefreshCurrentJobDisplay();
            RenderLastAction();
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
            _onlineModuleTypes = onLine;

            var tiles = new List<MachineTileInfo>();
            foreach (var tile in ModuleTiles)
            {
                var status = onLine.Contains(tile.ModuleType)
                    ? MachineStatus.Running
                    : MachineStatus.Offline;
                tiles.Add(new MachineTileInfo(
                    tile.ShortCode, status, T(status.ToString())));
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
        /// only when <paramref name="total"/> is zero; otherwise the severity
        /// strip supplies the detailed counts and the summary is coloured by
        /// the highest severity present (critical > warning > info).
        /// </summary>
        public void UpdateAlertsSummary(int critical, int warning, int info, int total)
        {
            _activeErrorCount = Math.Max(0, total);
            _criticalAlertCount = Math.Max(0, critical);
            _warningAlertCount = Math.Max(0, warning);
            _infoAlertCount = Math.Max(0, info);

            Brush fg, bg;
            string headline;
            string subtitle;
            string iconGlyph;

            if (total == 0)
            {
                fg = (Brush)FindResource("StatusRunningBrush");
                bg = (Brush)FindResource("StatusRunningBgBrush");
                iconGlyph = "\uE73E";
                headline = T("No active alerts");
                subtitle = T("All systems are operating normally.");
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

                headline = TF(total == 1 ? "{0} active alert" : "{0} active alerts", total);
                subtitle = string.Empty;
            }

            AlertsIconText.Foreground = fg;
            AlertsIconBg.Background = bg;
            AlertsIconText.Text = iconGlyph;
            AlertsHeadlineText.Text = headline;
            AlertsSubtitleText.Text = subtitle;
            AlertsSubtitleText.Visibility = total == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
            AlertsCriticalCountText.Text = critical.ToString("N0");
            AlertsWarningCountText.Text = warning.ToString("N0");
            AlertsInfoCountText.Text = info.ToString("N0");
            RefreshProductionButtons();
        }

        // ================= Counter / line controls =================
        //
        // These act on local mockup state only - there is no WFM connection in
        // this build, so instead of the old "not connected to the WFM" stubs
        // the controls now do real, visible work (adjust the counters, change
        // the job status). Swap these for WFM-backed commands once that link
        // exists (FR-03).

        private const int CounterStep = 1;

        private int _completedSets;
        private int _presetTarget;   // 0 == unlimited (shown as the infinity glyph)
        private int _confirmedCompletedSets;
        private int _confirmedPresetTarget;
        private bool _hasPendingCounterChanges;
        private CounterInputKind _pendingCounterInput;
        private JobRecord _currentJob;
        private MeasurementUnit _measurementUnit = MeasurementUnit.Millimeters;
        private string _jobStatusSource = "Idle";
        private string _jobStatusForegroundKey = "StatusOfflineBrush";
        private string _jobStatusBackgroundKey = "StatusOfflineBgBrush";

        private enum CounterInputKind
        {
            CompletedSets,
            PresetTarget,
        }

        private void UpdateCounterDisplay()
        {
            string completed = _completedSets.ToString("N0");
            CompletedSetsText.Text = completed;
            UpdatePresetDisplay();
        }

        private void UpdatePresetDisplay()
        {
            string target = _presetTarget == 0 ? "\u221E" : _presetTarget.ToString("N0");
            PresetValueText.Text = target;
        }

        private void UpdateConfirmedCounterDisplay()
        {
            string completed = _confirmedCompletedSets.ToString("N0");
            string target = _confirmedPresetTarget == 0
                ? "\u221E"
                : _confirmedPresetTarget.ToString("N0");

            JobCompletedText.Text = completed;
            JobQuantityText.Text = target;
        }

        /// <summary>
        /// Updates the shared job shown on Home and resets local production and
        /// counters for the newly selected job.
        /// </summary>
        public void SetCurrentJob(JobRecord job)
        {
            bool changedJob = !ReferenceEquals(_currentJob, job);
            _currentJob = job;
            if (changedJob)
            {
                ResetProductionState(clearPreset: true);
            }

            if (_currentJob == null)
            {
                _jobStatusSource = "Idle";
                _jobStatusForegroundKey = "StatusOfflineBrush";
                _jobStatusBackgroundKey = "StatusOfflineBgBrush";
                RefreshCurrentJobDisplay();
                RefreshProductionButtons();
                return;
            }

            _jobStatusSource = "Loaded";
            _jobStatusForegroundKey = "StatusRunningBrush";
            _jobStatusBackgroundKey = "StatusRunningBgBrush";
            RefreshCurrentJobDisplay();
            RefreshProductionButtons();
        }

        /// <summary>Reformats current-job measurements from canonical millimeters.</summary>
        public void SetMeasurementUnit(MeasurementUnit unit)
        {
            _measurementUnit = unit;
            RefreshCurrentJobMeasurements();
        }

        private void RefreshCurrentJobMeasurements()
        {
            if (_currentJob == null)
            {
                return;
            }

            JobFormatText.Text = T(_currentJob.Format) + " · " +
                MeasurementFormatter.FormatDimensions(
                    _currentJob.WidthMm, _currentJob.LengthMm, _measurementUnit);
        }

        private void RefreshCurrentJobDisplay()
        {
            if (_currentJob == null)
            {
                JobNameText.Text = T("No job loaded");
                JobFormatText.Text = "-";
                JobPagesText.Text = "-";
            }
            else
            {
                JobNameText.Text = _currentJob.Name;
                RefreshCurrentJobMeasurements();
                JobPagesText.Text = TF("{0} pages", _currentJob.Pages);
            }

            JobStatusText.Text = T(_jobStatusSource);
            JobStatusText.Foreground = (Brush)FindResource(_jobStatusForegroundKey);
            JobStatusPill.Background = (Brush)FindResource(_jobStatusBackgroundKey);
        }

        private void MarkCounterChangesPending(string source, params object[] values)
        {
            _hasPendingCounterChanges = true;
            RefreshProductionButtons();
            ShowAction(source, values);
        }

        private void SetJobStatus(string source, string foregroundKey, string backgroundKey)
        {
            _jobStatusSource = source;
            _jobStatusForegroundKey = foregroundKey;
            _jobStatusBackgroundKey = backgroundKey;
            RefreshCurrentJobDisplay();
        }

        private void ShowAction(string source, params object[] values)
        {
            object[] capturedValues = values ?? new object[0];
            _lastActionRenderer = () => TF(source, capturedValues);
            RenderLastAction();
        }

        private void RenderLastAction()
        {
            LastActionText.Text = _lastActionRenderer == null
                ? string.Empty
                : _lastActionRenderer();
        }

        private void OnCounterDecrementClick(object sender, RoutedEventArgs e)
        {
            _presetTarget = Math.Max(0, _presetTarget - CounterStep);
            UpdatePresetDisplay();
            if (_presetTarget == 0)
            {
                MarkCounterChangesPending(
                    "Preset pending: unlimited production. Select Confirm to apply the changes.");
            }
            else
            {
                MarkCounterChangesPending(
                    "Preset pending: {0:N0} sets. Select Confirm to apply the changes.",
                    _presetTarget);
            }
        }

        private void OnCounterIncrementClick(object sender, RoutedEventArgs e)
        {
            if (_presetTarget < NumericInputDialog.MaximumValue)
            {
                _presetTarget += CounterStep;
            }
            UpdatePresetDisplay();
            MarkCounterChangesPending(
                "Preset pending: {0:N0} sets. Select Confirm to apply the changes.",
                _presetTarget);
        }

        private void OnCompletedDecrementClick(object sender, RoutedEventArgs e)
        {
            _completedSets = Math.Max(0, _completedSets - CounterStep);
            UpdateCounterDisplay();
            MarkCounterChangesPending(
                "Completed sets pending: {0:N0}. Select Confirm to apply the changes.",
                _completedSets);
        }

        private void OnCompletedIncrementClick(object sender, RoutedEventArgs e)
        {
            if (_completedSets < NumericInputDialog.MaximumValue)
            {
                _completedSets += CounterStep;
            }
            UpdateCounterDisplay();
            MarkCounterChangesPending(
                "Completed sets pending: {0:N0}. Select Confirm to apply the changes.",
                _completedSets);
        }

        private void OnCompletedInputClick(object sender, RoutedEventArgs e)
        {
            _pendingCounterInput = CounterInputKind.CompletedSets;
            CounterInputDialog.Open(
                T("Set Completed Sets"),
                T("Completed sets"),
                T("Enter how many sets the machine has completed so far."),
                _completedSets,
                zeroMeansUnlimited: false);
        }

        private void OnPresetInputClick(object sender, RoutedEventArgs e)
        {
            _pendingCounterInput = CounterInputKind.PresetTarget;
            CounterInputDialog.Open(
                T("Set Production Preset"),
                T("Sets to make"),
                T("Enter the total number of sets the machine should make."),
                _presetTarget,
                zeroMeansUnlimited: true);
        }

        private void OnCounterValueConfirmed(object sender, int value)
        {
            if (_pendingCounterInput == CounterInputKind.CompletedSets)
            {
                _completedSets = value;
                UpdateCounterDisplay();
                MarkCounterChangesPending(
                    "Completed sets pending: {0:N0}. Select Confirm to apply the changes.",
                    _completedSets);
                return;
            }

            _presetTarget = value;
            UpdatePresetDisplay();
            if (_presetTarget == 0)
            {
                MarkCounterChangesPending(
                    "Preset pending: unlimited production. Select Confirm to apply the changes.");
            }
            else
            {
                MarkCounterChangesPending(
                    "Preset pending: {0:N0} sets. Select Confirm to apply the changes.",
                    _presetTarget);
            }
        }

        private void OnResetToZeroClick(object sender, RoutedEventArgs e)
        {
            _completedSets = 0;
            UpdateCounterDisplay();
            MarkCounterChangesPending(
                "Completed sets pending: {0:N0}. Select Confirm to apply the changes.", 0);
        }

        private void OnSetTargetClick(object sender, RoutedEventArgs e)
        {
            OnPresetInputClick(sender, e);
        }

        private void OnConfirmCounterChangesClick(object sender, RoutedEventArgs e)
        {
            if (_productionState == ProductionState.Running)
            {
                ShowAction("Pause production before applying counter changes.");
                return;
            }

            _confirmedCompletedSets = _completedSets;
            _confirmedPresetTarget = _presetTarget;
            UpdateConfirmedCounterDisplay();
            _hasPendingCounterChanges = false;
            RefreshProductionButtons();

            if (_confirmedPresetTarget == 0)
            {
                ShowAction(
                    "Counter changes confirmed: {0:N0} completed / unlimited preset.",
                    _confirmedCompletedSets);
            }
            else
            {
                ShowAction(
                    "Counter changes confirmed: {0:N0} completed / {1:N0} preset.",
                    _confirmedCompletedSets, _confirmedPresetTarget);
            }
        }

        private void OnNewJobClick(object sender, RoutedEventArgs e) => NavigateToJobsRequested?.Invoke(this, EventArgs.Empty);
        private void OnLoadJobClick(object sender, RoutedEventArgs e) => NavigateToJobsRequested?.Invoke(this, EventArgs.Empty);
        private void OnViewErrorsClick(object sender, RoutedEventArgs e) => NavigateToErrorsRequested?.Invoke(this, EventArgs.Empty);

        private void OnPurgeClick(object sender, RoutedEventArgs e)
        {
            ResetProductionState(clearPreset: true);
            SetJobStatus(_currentJob == null ? "Idle" : "Ready",
                _currentJob == null ? "StatusOfflineBrush" : "StatusRunningBrush",
                _currentJob == null ? "StatusOfflineBgBrush" : "StatusRunningBgBrush");
            ShowAction("Line purged. Production, completed sets, and preset were reset.");
        }

        private void OnStartClick(object sender, RoutedEventArgs e)
        {
            if (_currentJob == null)
            {
                ShowAction("Load a job before starting production.");
                return;
            }

            if (_activeErrorCount > 0)
            {
                ShowAction("Resolve all active errors before starting production.");
                return;
            }

            if (_hasPendingCounterChanges)
            {
                ShowAction("Confirm the pending counter changes before starting.");
                return;
            }

            if (_productionState != ProductionState.Ready &&
                _productionState != ProductionState.Paused &&
                _productionState != ProductionState.Stopped)
            {
                return;
            }

            if (_confirmedPresetTarget > 0 &&
                _confirmedCompletedSets >= _confirmedPresetTarget)
            {
                CompleteProduction();
                return;
            }

            bool wasPaused = _productionState == ProductionState.Paused;
            bool wasStopped = _productionState == ProductionState.Stopped;
            _productionState = ProductionState.Running;
            _productionTimer.Start();
            SetJobStatus("Running", "StatusRunningBrush", "StatusRunningBgBrush");
            ShowAction(wasPaused
                ? "Production resumed."
                : wasStopped
                    ? "Production restarted."
                    : "Production started.");
            RefreshProductionButtons();
        }

        private void OnPauseClick(object sender, RoutedEventArgs e)
        {
            if (_productionState != ProductionState.Running)
            {
                return;
            }

            _productionTimer.Stop();
            _productionState = ProductionState.Paused;
            SetJobStatus("Paused", "WarningBrush", "WarningBgBrush");
            ShowAction("Production paused. Select Start to resume.");
            RefreshProductionButtons();
        }

        private void OnStopClick(object sender, RoutedEventArgs e)
        {
            if (_productionState != ProductionState.Running &&
                _productionState != ProductionState.Paused)
            {
                return;
            }

            _productionTimer.Stop();
            _productionState = ProductionState.Stopped;
            SetJobStatus("Stopped", "StatusErrorBrush", "StatusErrorBgBrush");
            ShowAction("Production stopped. Select Start to restart or Purge to reset the line.");
            RefreshProductionButtons();
        }

        private void OnProductionTimerTick(object sender, EventArgs e)
        {
            if (_productionState != ProductionState.Running)
            {
                return;
            }

            if (_confirmedCompletedSets < NumericInputDialog.MaximumValue)
            {
                _confirmedCompletedSets++;
            }
            _completedSets = _confirmedCompletedSets;
            UpdateCounterDisplay();
            UpdateConfirmedCounterDisplay();

            if (_confirmedPresetTarget > 0 &&
                _confirmedCompletedSets >= _confirmedPresetTarget)
            {
                CompleteProduction();
            }
        }

        private void CompleteProduction()
        {
            _productionTimer.Stop();
            _productionState = ProductionState.Completed;
            SetJobStatus("Completed", "StatusRunningBrush", "StatusRunningBgBrush");
            ShowAction("Production completed the preset of {0:N0} sets.",
                _confirmedPresetTarget);
            RefreshProductionButtons();
        }

        private void ResetProductionState(bool clearPreset)
        {
            _productionTimer.Stop();
            _productionState = ProductionState.Ready;
            _completedSets = 0;
            _confirmedCompletedSets = 0;
            if (clearPreset)
            {
                _presetTarget = 0;
                _confirmedPresetTarget = 0;
            }
            else
            {
                _presetTarget = _confirmedPresetTarget;
            }

            _hasPendingCounterChanges = false;
            UpdateCounterDisplay();
            UpdateConfirmedCounterDisplay();
            RefreshProductionButtons();
        }

        private void RefreshProductionButtons()
        {
            bool hasJob = _currentJob != null;
            bool canEditCounters = _productionState != ProductionState.Running;
            bool canStartFromState =
                (_productionState == ProductionState.Ready ||
                 _productionState == ProductionState.Paused ||
                 _productionState == ProductionState.Stopped);

            StartButton.IsEnabled = hasJob &&
                                    _activeErrorCount == 0 &&
                                    !_hasPendingCounterChanges &&
                                    canStartFromState;
            PauseButton.IsEnabled = _productionState == ProductionState.Running;
            StopButton.IsEnabled = _productionState == ProductionState.Running ||
                                   _productionState == ProductionState.Paused;
            PurgeButton.IsEnabled = _productionState == ProductionState.Stopped ||
                                    _productionState == ProductionState.Completed;

            // Counter values form one transactional editing surface. Lock all
            // entry points while sheets are moving, then restore them when the
            // line is paused, stopped, completed, or ready.
            CompletedSetsMinusButton.IsEnabled = canEditCounters;
            CompletedSetsPlusButton.IsEnabled = canEditCounters;
            PresetMinusButton.IsEnabled = canEditCounters;
            PresetPlusButton.IsEnabled = canEditCounters;
            ResetToZeroButton.IsEnabled = canEditCounters;
            SetTargetButton.IsEnabled = canEditCounters;
            ConfirmCounterChangesButton.IsEnabled = canEditCounters;

            // Keep the live number displays fully legible while blocking
            // mouse, touch, and keyboard entry until production is paused or
            // stopped. They deliberately remain enabled visually.
            CompletedSetsValueButton.IsHitTestVisible = canEditCounters;
            CompletedSetsValueButton.Focusable = canEditCounters;
            CompletedSetsValueButton.Cursor = canEditCounters ? Cursors.Hand : Cursors.Arrow;
            PresetValueButton.IsHitTestVisible = canEditCounters;
            PresetValueButton.Focusable = canEditCounters;
            PresetValueButton.Cursor = canEditCounters ? Cursors.Hand : Cursors.Arrow;
        }

        private static string T(string source)
        {
            return LocalizationManager.Translate(source);
        }

        private static string TF(string source, params object[] values)
        {
            return string.Format(
                System.Globalization.CultureInfo.CurrentCulture,
                T(source), values);
        }
    }
}
