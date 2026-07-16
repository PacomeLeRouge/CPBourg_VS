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
        private const double CurrentSpeedMillimetersPerSecond = 1291;

        public event EventHandler CloseRequested;

        public TechnicianInterfaceView()
        {
            InitializeComponent();

            _settingsStore = new TechnicianSettingsStore();
            _savedSettings = _settingsStore.Load();
            ApplySettingsToControls(_savedSettings);

            AccessDialog.AccessGranted += OnTechnicalAccessGranted;
        }

        public void SetMeasurementUnit(MeasurementUnit unit)
        {
            CurrentSpeedValueText.Text = MeasurementFormatter.FormatValue(
                CurrentSpeedMillimetersPerSecond, unit, "0", "0.0");
            CurrentSpeedUnitText.Text = MeasurementFormatter.SpeedUnitSymbol(unit);
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
                FooterStatusText.Text = "Unsaved changes";
                FooterStatusText.Foreground = FindResource("WarningBrush") as System.Windows.Media.Brush;
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
                FooterStatusText.Text = errorMessage;
                FooterStatusText.Foreground = FindResource("StatusErrorBrush") as System.Windows.Media.Brush;
                return false;
            }

            _savedSettings = settings.Clone();
            FooterStatusText.Text = "Technician settings saved";
            FooterStatusText.Foreground = FindResource("StatusRunningBrush") as System.Windows.Media.Brush;
            return true;
        }

        private void OnResetClick(object sender, RoutedEventArgs e)
        {
            ApplySettingsToControls(TechnicianSettings.CreateDefaults());
            FooterStatusText.Text = "Defaults restored — select Save to keep them";
            FooterStatusText.Foreground = FindResource("WarningBrush") as System.Windows.Media.Brush;
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            // Back discards unsaved edits so returning to the screen always
            // reflects the last durable state.
            ApplySettingsToControls(_savedSettings);
            FooterStatusText.Text = string.Empty;
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnActionClick(object sender, RoutedEventArgs e)
        {
            string action = (sender as FrameworkElement)?.Tag as string ?? "Action";
            FooterStatusText.Text = action + " command prepared (prototype only)";
            FooterStatusText.Foreground = FindResource("TextSecondaryBrush") as System.Windows.Media.Brush;
        }

        private void OnTechnicalAccessClick(object sender, RoutedEventArgs e)
        {
            AccessDialog.Open();
        }

        private void OnTechnicalAccessGranted(object sender, string technicianCode)
        {
            // The code itself is never retained after validation.
            AccessStatusText.Text = "Granted";
            AccessStatusText.Foreground = FindResource("StatusRunningBrush") as System.Windows.Media.Brush;
            FooterStatusText.Text = "Technical access granted";
            FooterStatusText.Foreground = FindResource("StatusRunningBrush") as System.Windows.Media.Brush;
        }
    }
}
