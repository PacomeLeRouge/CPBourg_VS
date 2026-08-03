using System;
using System.Windows;
using System.Windows.Controls;
using CPBourg.NextGenGui.Models;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>
    /// Technician Interface high-fidelity prototype. Settings are loaded from
    /// and saved to the current Windows user's local application data.
    /// Machine actions remain safe UI-only demonstrations until a WFM command
    /// service is connected.
    /// </summary>
    public partial class TechnicianInterfaceView : UserControl
    {
        private readonly TechnicianSettingsStore _settingsStore;
        private TechnicianSettings _savedSettings;
        private bool _isApplyingSettings;
        private string _footerStatusSource;
        private object[] _footerStatusArguments;
        private bool _technicalAccessGranted;
        private const double CurrentSpeedMillimetersPerSecond = 1291;

        /// <summary>Raised after Back or a successful Confirm requests Home navigation.</summary>
        public event EventHandler CloseRequested;

        /// <summary>Loads the last durable technician configuration.</summary>
        public TechnicianInterfaceView()
        {
            InitializeComponent();

            _settingsStore = new TechnicianSettingsStore();
            _savedSettings = _settingsStore.Load();
            ApplySettingsToControls(_savedSettings);

            AccessDialog.AccessGranted += OnTechnicalAccessGranted;
        }

        /// <summary>Reformats the simulated current-speed readout.</summary>
        public void SetMeasurementUnit(MeasurementUnit unit)
        {
            CurrentSpeedValueText.Text = MeasurementFormatter.FormatValue(
                CurrentSpeedMillimetersPerSecond, unit, "0", "0.0");
            CurrentSpeedUnitText.Text = MeasurementFormatter.SpeedUnitSymbol(unit);
        }

        /// <summary>Refreshes labels, access state, feedback, and the code dialog.</summary>
        public void ApplyLanguage()
        {
            LocalizationManager.Apply(this);
            RenderFooterStatus();
            AccessStatusText.Text = T(_technicalAccessGranted ? "Granted" : "Protected");
            AccessDialog.ApplyLanguage();
        }

        private void ApplySettingsToControls(TechnicianSettings settings)
        {
            _isApplyingSettings = true;
            try
            {
                CalibrationAlwaysProcessRadio.IsChecked = settings.CalibrationSetOption == "Always Process";
                CalibrationAlwaysRejectRadio.IsChecked = settings.CalibrationSetOption != "Always Process";
                SaveAdjustmentsEnabledRadio.IsChecked = settings.SaveAdjustmentsEnabled;
                SaveAdjustmentsDisabledRadio.IsChecked = !settings.SaveAdjustmentsEnabled;
                LoopStitchRadio.IsChecked = settings.StitchHeadForm == "Loop Stitch";
                NormalStitchRadio.IsChecked = settings.StitchHeadForm != "Loop Stitch";
                DisableHead1CheckBox.IsChecked = settings.DisableStitchHead1;
                DisableHead2CheckBox.IsChecked = settings.DisableStitchHead2;
                SingleSheetEnabledRadio.IsChecked = settings.StitchSingleSheetEnabled;
                SingleSheetDisabledRadio.IsChecked = !settings.StitchSingleSheetEnabled;
                PurgeAlwaysProcessRadio.IsChecked = settings.PurgeOption == "Always Process";
                PurgeAlwaysPurgeRadio.IsChecked = settings.PurgeOption == "Always Purge";
                PurgeAskOperatorRadio.IsChecked = settings.PurgeOption != "Always Process" &&
                                                  settings.PurgeOption != "Always Purge";
                RestartAllowedRadio.IsChecked = settings.SheetInCompilerRestartAllowed;
                RestartForbiddenRadio.IsChecked = !settings.SheetInCompilerRestartAllowed;
            }
            finally
            {
                _isApplyingSettings = false;
            }
        }

        private TechnicianSettings ReadSettingsFromControls()
        {
            return new TechnicianSettings
            {
                CalibrationSetOption = CalibrationAlwaysProcessRadio.IsChecked == true
                    ? "Always Process" : "Always Reject",
                SaveAdjustmentsEnabled = SaveAdjustmentsEnabledRadio.IsChecked == true,
                StitchHeadForm = LoopStitchRadio.IsChecked == true ? "Loop Stitch" : "Normal Stitch",
                DisableStitchHead1 = DisableHead1CheckBox.IsChecked == true,
                DisableStitchHead2 = DisableHead2CheckBox.IsChecked == true,
                StitchSingleSheetEnabled = SingleSheetEnabledRadio.IsChecked == true,
                PurgeOption = PurgeAlwaysPurgeRadio.IsChecked == true
                    ? "Always Purge"
                    : PurgeAlwaysProcessRadio.IsChecked == true ? "Always Process" : "Ask Operator",
                SheetInCompilerRestartAllowed = RestartAllowedRadio.IsChecked == true,
            };
        }

        private void OnSettingChanged(object sender, RoutedEventArgs e)
        {
            if (!_isApplyingSettings)
            {
                SetFooterStatus("Unsaved changes", "WarningBrush");
            }
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            if (SaveSettings())
            {
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool SaveSettings()
        {
            TechnicianSettings settings = ReadSettingsFromControls();
            string errorMessage;
            if (!_settingsStore.TrySave(settings, out errorMessage))
            {
                _footerStatusSource = null;
                _footerStatusArguments = null;
                FooterStatusText.Text = errorMessage;
                FooterStatusText.Foreground = FindResource("StatusErrorBrush") as System.Windows.Media.Brush;
                return false;
            }

            _savedSettings = settings.Clone();
            SetFooterStatus("Technician settings saved", "StatusRunningBrush");
            return true;
        }

        private void OnResetClick(object sender, RoutedEventArgs e)
        {
            ApplySettingsToControls(TechnicianSettings.CreateDefaults());
            SetFooterStatus("Defaults restored — select Save to keep them", "WarningBrush");
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            // Back discards unsaved edits so returning to the screen always
            // reflects the last durable state.
            ApplySettingsToControls(_savedSettings);
            _footerStatusSource = null;
            _footerStatusArguments = null;
            FooterStatusText.Text = string.Empty;
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnActionClick(object sender, RoutedEventArgs e)
        {
            string action = (sender as FrameworkElement)?.Tag as string ?? "Action";
            SetFooterStatus("{0} command prepared (prototype only)", "TextSecondaryBrush", T(action));
        }

        private void OnTechnicalAccessClick(object sender, RoutedEventArgs e)
        {
            AccessDialog.Open();
        }

        private void OnTechnicalAccessGranted(object sender, string technicianCode)
        {
            // The code itself is never retained after validation.
            _technicalAccessGranted = true;
            AccessStatusText.Text = T("Granted");
            AccessStatusText.Foreground = FindResource("StatusRunningBrush") as System.Windows.Media.Brush;
            SetFooterStatus("Technical access granted", "StatusRunningBrush");
        }

        private void SetFooterStatus(string source, string brushResource, params object[] arguments)
        {
            _footerStatusSource = source;
            _footerStatusArguments = arguments;
            FooterStatusText.Foreground = FindResource(brushResource) as System.Windows.Media.Brush;
            RenderFooterStatus();
        }

        private void RenderFooterStatus()
        {
            if (string.IsNullOrEmpty(_footerStatusSource))
            {
                return;
            }

            FooterStatusText.Text = string.Format(
                System.Globalization.CultureInfo.CurrentCulture,
                T(_footerStatusSource), _footerStatusArguments ?? new object[0]);
        }

        private static string T(string source)
        {
            return LocalizationManager.Translate(source);
        }
    }
}
