using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using CPBourg.NextGenGui.Models;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>
    /// Individual Machine Configuration screen for the STFO (booklet maker) -
    /// a five-step wizard (Overview / Stitching / Folding / Trimming / Conveyor)
    /// for a job's settings. Reached by tapping the STFO tile on the Home
    /// dashboard (see <see cref="DashboardView.NavigateToStfoRequested"/>).
    ///
    /// Step 1 shows the machine-line overview. Steps 2-5 provide interactive
    /// Stitching, Folding, Trimming, and Conveyor forms with live previews.
    /// Each form keeps a saved snapshot: Reset loads defaults as pending edits,
    /// Save commits the current step, and leaving discards unsaved changes.
    /// All local; no WFM in this build.
    /// </summary>
    public partial class StfoConfigurationView : UserControl
    {
        private static readonly string[] StepNames =
        {
            "Overview", "Stitching", "Folding", "Trimming", "Conveyor",
        };

        private const int StitchingStep = 1;
        private const int FoldingStep = 2;
        private const int TrimmingStep = 3;
        private const int ConveyorStep = 4;

        private const double DefaultPaperWidth = 210;
        private const double DefaultPaperLength = 297;
        private const double DefaultStitchSpacing = 10;
        private const double DefaultHorizontalOffset = 0;
        private const double DefaultVerticalOffset = 0;
        private const string DefaultStitchMode = "Saddle";

        private const bool DefaultFoldEnabled = true;
        private const double DefaultFoldPosition = 10;
        private const string DefaultPressureMode = "Manual";
        private const double DefaultPressureLevel = 0.45;

        private const bool DefaultTrimEnabled = true;
        private const double DefaultFinalLength = 205;
        private const double DefaultClampPressure = 50;
        private const bool DefaultChipBlower = true;

        private const int DefaultBookletSpacing = 8;
        private const int DefaultBookletOffset = 10;
        private const bool DefaultFullDetection = true;

        private readonly Button[] _stepTabs;
        private int _currentStep;
        private JobRecord _currentJob;
        private MeasurementUnit _measurementUnit = MeasurementUnit.Millimeters;

        // Stitching parameters (defaults match the field text set in XAML).
        private double _paperW = DefaultPaperWidth, _paperL = DefaultPaperLength;
        private double _spacing = DefaultStitchSpacing, _hOffset = DefaultHorizontalOffset;
        private double _vOffset = DefaultVerticalOffset;
        private string _stitchMode = DefaultStitchMode;
        private StitchNumericField _pendingStitchNumericField;

        private enum StitchNumericField
        {
            PaperWidth,
            PaperLength,
            Spacing,
            HorizontalOffset,
            VerticalOffset,
            FoldPosition,
            FinalBookletLength,
        }

        private ConveyorNumericField _pendingConveyorNumericField;

        private enum ConveyorNumericField
        {
            Spacing,
            Offset,
        }

        private Button[] _modeButtons;
        private TextBlock[] _modeLabels;

        // Folding parameters (defaults match the controls set in XAML).
        private bool _foldEnabled = DefaultFoldEnabled;
        private double _foldPosition = DefaultFoldPosition;
        private string _pressureMode = DefaultPressureMode;

        // Trimming parameters (defaults match the controls set in XAML).
        private bool _trimEnabled = DefaultTrimEnabled;
        private double _finalLength = DefaultFinalLength;
        private double _clampPressure = DefaultClampPressure;
        private bool _chipBlower = DefaultChipBlower;

        // Conveyor parameters (defaults match the controls set in XAML).
        private int _bookletSpacing = DefaultBookletSpacing;
        private int _bookletOffset = DefaultBookletOffset;
        private bool _fullDetection = DefaultFullDetection;

        private StitchingConfiguration _savedStitching;
        private FoldingConfiguration _savedFolding;
        private TrimmingConfiguration _savedTrimming;
        private ConveyorConfiguration _savedConveyor;

        private sealed class StitchingConfiguration
        {
            public double PaperWidth;
            public double PaperLength;
            public double Spacing;
            public double HorizontalOffset;
            public double VerticalOffset;
            public string Mode;
        }

        private sealed class FoldingConfiguration
        {
            public bool Enabled;
            public double Position;
            public string PressureMode;
            public double PressureLevel;
        }

        private sealed class TrimmingConfiguration
        {
            public bool Enabled;
            public double FinalLength;
            public double ClampPressure;
            public bool ChipBlower;
        }

        private sealed class ConveyorConfiguration
        {
            public int Spacing;
            public int Offset;
            public bool FullDetection;
        }

        // Suppresses the TextChanged handlers that fire while InitializeComponent
        // sets each field's initial text, before the rest of the tree exists.
        private bool _loaded;

        /// <summary>Raised when the step changes so the shell header title can
        /// follow it (e.g. "STFO - Stitching").</summary>
        public event EventHandler<string> TitleChanged;

        /// <summary>Raised to return to the dashboard - from Back on the first
        /// step, or Confirm on the last.</summary>
        public event EventHandler CloseRequested;

        public StfoConfigurationView()
        {
            InitializeComponent();

            StitchNumericDialog.ValueConfirmed += OnStitchNumericValueConfirmed;
            ConveyorNumericDialog.ValueConfirmed += OnConveyorNumericValueConfirmed;

            _stepTabs = new[] { StepTab0, StepTab1, StepTab2, StepTab3, StepTab4 };
            _modeButtons = new[] { ModeSaddle, ModeTop, ModeRightCorner, ModeLeftCorner, ModeNone };
            _modeLabels = new[] { ModeSaddleLabel, ModeTopLabel, ModeRightCornerLabel, ModeLeftCornerLabel, ModeNoneLabel };

            _loaded = true;
            RefreshStitchModeButtons();
            UpdateStitchSummary();
            RedrawStitchPreview();

            _foldPosition = FoldPositionSlider.Value;
            FoldPositionValueText.Text = Fmt(_foldPosition, "0.0");
            RefreshFoldChoiceButtons();
            UpdateFoldSummary();
            RedrawFoldPreview();

            _clampPressure = ClampPressureSlider.Value;
            ClampPressureValueText.Text = _clampPressure.ToString("0", CultureInfo.InvariantCulture) + "%";
            RefreshTrimChoiceButtons();
            UpdateTrimSummary();
            RedrawTrimPreview();

            _bookletSpacing = (int)Math.Round(SpacingSlider.Value);
            _bookletOffset = (int)Math.Round(OffsetSlider.Value);
            SpacingValueText.Text = _bookletSpacing.ToString(CultureInfo.InvariantCulture);
            OffsetValueText.Text = _bookletOffset.ToString(CultureInfo.InvariantCulture);
            RefreshConveyorChoiceButtons();
            UpdateConveyorSummary();
            RedrawConveyorPreview();

            SaveAllCurrentConfigurations();

            RefreshUi();
        }

        /// <summary>Loads the current job's independent STFO configuration.</summary>
        public void LoadJob(JobRecord job)
        {
            RestoreSavedConfiguration(_currentStep);
            _currentJob = job;
            CurrentJobNameRun.Text = job == null
                ? T("No job loaded")
                : job.Name + " (" + job.Format + ", " + TF("{0} pages", job.Pages) + ")";

            if (job == null)
            {
                ApplyStitchingConfiguration(CreateDefaultStitchingConfiguration());
                ApplyFoldingConfiguration(CreateDefaultFoldingConfiguration());
                ApplyTrimmingConfiguration(CreateDefaultTrimmingConfiguration());
                ApplyConveyorConfiguration(CreateDefaultConveyorConfiguration());
            }
            else
            {
                ApplyJobSettings(job.StfoSettings);
            }

            SaveAllCurrentConfigurations();
        }

        /// <summary>Re-applies fixed and generated copy after the operator
        /// changes language without discarding the current configuration.</summary>
        public void ApplyLanguage()
        {
            LocalizationManager.Apply(this);
            RefreshUi();
            UpdateStitchSummary();
            RedrawStitchPreview();
            UpdateFoldSummary();
            RedrawFoldPreview();
            UpdateTrimSummary();
            RedrawTrimPreview();
            UpdateConveyorSummary();
            RedrawConveyorPreview();

            if (_currentJob == null)
            {
                CurrentJobNameRun.Text = T("No job loaded");
            }
            else
            {
                CurrentJobNameRun.Text = _currentJob.Name + " (" + _currentJob.Format + ", " +
                    TF("{0} pages", _currentJob.Pages) + ")";
            }
        }

        public void SetMeasurementUnit(MeasurementUnit unit)
        {
            _measurementUnit = unit;
            RefreshMeasurementDisplay();
        }

        private void RefreshMeasurementDisplay()
        {
            bool wasLoaded = _loaded;
            _loaded = false;
            PaperWidthBox.Text = DisplayValue(_paperW);
            PaperLengthBox.Text = DisplayValue(_paperL);
            SpacingBox.Text = DisplayValue(_spacing);
            HOffsetBox.Text = DisplayValue(_hOffset);
            VOffsetBox.Text = DisplayValue(_vOffset);
            FoldPositionValueText.Text = DisplayValue(_foldPosition, "0.0", "0.000");
            FinalLengthBox.Text = DisplayValue(_finalLength, "0.0", "0.000");

            string symbol = MeasurementFormatter.UnitSymbol(_measurementUnit);
            PaperWidthUnitText.Text = symbol;
            PaperLengthUnitText.Text = symbol;
            StitchSpacingUnitText.Text = symbol;
            HorizontalOffsetUnitText.Text = symbol;
            VerticalOffsetUnitText.Text = symbol;
            FoldPositionUnitText.Text = symbol;
            FinalLengthUnitText.Text = symbol;
            _loaded = wasLoaded;

            UpdateStitchSummary();
            RedrawStitchPreview();
            UpdateFoldSummary();
            RedrawFoldPreview();
            UpdateTrimSummary();
            RedrawTrimPreview();
        }

        /// <summary>Resets the wizard to the first (Overview) step. Called when
        /// the dashboard navigates in, so entry always lands on Overview.</summary>
        public void ResetToStart()
        {
            RestoreSavedConfiguration(_currentStep);
            _currentStep = 0;
            RefreshUi();
        }

        private void GoToStep(int step)
        {
            int targetStep = Math.Max(0, Math.Min(StepNames.Length - 1, step));
            if (targetStep == _currentStep)
            {
                return;
            }

            // Each step is an edit transaction. Leaving without Save discards
            // its working values and restores the most recently saved snapshot.
            RestoreSavedConfiguration(_currentStep);
            _currentStep = targetStep;
            RefreshUi();
        }

        private void RefreshUi()
        {
            var activeBg = (Brush)FindResource("HeaderBackgroundBrush");
            var activeFg = (Brush)FindResource("HeaderTextPrimaryBrush");
            var inactiveFg = (Brush)FindResource("TextPrimaryBrush");

            for (int i = 0; i < _stepTabs.Length; i++)
            {
                bool isActive = i == _currentStep;
                _stepTabs[i].Background = isActive ? activeBg : Brushes.Transparent;
                _stepTabs[i].Foreground = isActive ? activeFg : inactiveFg;
                _stepTabs[i].FontWeight = isActive ? FontWeights.SemiBold : FontWeights.Normal;
            }

            bool isStitching = _currentStep == StitchingStep;
            bool isFolding = _currentStep == FoldingStep;
            bool isTrimming = _currentStep == TrimmingStep;
            bool isConveyor = _currentStep == ConveyorStep;
            StitchingContent.Visibility = isStitching ? Visibility.Visible : Visibility.Collapsed;
            FoldingContent.Visibility = isFolding ? Visibility.Visible : Visibility.Collapsed;
            TrimmingContent.Visibility = isTrimming ? Visibility.Visible : Visibility.Collapsed;
            ConveyorContent.Visibility = isConveyor ? Visibility.Visible : Visibility.Collapsed;
            OverviewContent.Visibility = (!isStitching && !isFolding && !isTrimming && !isConveyor) ? Visibility.Visible : Visibility.Collapsed;

            BackButtonText.Text = _currentStep == 0
                ? T("Back")
                : TF("Back: {0}", T(StepNames[_currentStep - 1]));

            // Last step commits the whole configuration (Confirm) rather than
            // advancing.
            bool isLast = _currentStep == StepNames.Length - 1;
            NextButtonText.Text = isLast
                ? T("Confirm")
                : TF("Next: {0}", T(StepNames[_currentStep + 1]));
            ResetStepButton.Visibility = _currentStep == 0 ? Visibility.Collapsed : Visibility.Visible;
            SaveStepButton.Visibility = _currentStep == 0 ? Visibility.Collapsed : Visibility.Visible;

            FooterStatusText.Text = string.Empty;

            LocalizationManager.Apply(this);
            if (Visibility == Visibility.Visible)
            {
                TitleChanged?.Invoke(this, "STFO - " + T(StepNames[_currentStep]));
            }
        }

        // ================= Wizard navigation =================

        private void OnStepTabClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tag && int.TryParse(tag, out int step))
            {
                GoToStep(step);
            }
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            if (_currentStep == 0)
            {
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                GoToStep(_currentStep - 1);
            }
        }

        private void OnNextClick(object sender, RoutedEventArgs e)
        {
            if (_currentStep == StepNames.Length - 1)
            {
                RestoreSavedConfiguration(_currentStep);
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                GoToStep(_currentStep + 1);
            }
        }

        private void OnResetClick(object sender, RoutedEventArgs e)
        {
            if (_currentStep == 0)
            {
                FooterStatusText.Text = T("Select a configuration step to reset its settings.");
                return;
            }

            ApplyDefaultConfiguration(_currentStep);
            FooterStatusText.Text = TF(
                "{0} settings reset to defaults. Select Save to keep them.",
                T(StepNames[_currentStep]));
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            if (_currentStep == 0)
            {
                FooterStatusText.Text = T("Select a configuration step to save its settings.");
                return;
            }

            SaveCurrentConfiguration(_currentStep);
            SaveConfigurationsToCurrentJob();
            FooterStatusText.Text = TF("{0} configuration saved.", T(StepNames[_currentStep]));
        }

        private void ApplyJobSettings(StfoJobSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            ApplyStitchingConfiguration(new StitchingConfiguration
            {
                PaperWidth = settings.PaperWidth,
                PaperLength = settings.PaperLength,
                Spacing = settings.StitchSpacing,
                HorizontalOffset = settings.HorizontalOffset,
                VerticalOffset = settings.VerticalOffset,
                Mode = settings.StitchMode,
            });
            ApplyFoldingConfiguration(new FoldingConfiguration
            {
                Enabled = settings.FoldEnabled,
                Position = settings.FoldPosition,
                PressureMode = settings.PressureMode,
                PressureLevel = settings.PressureLevel,
            });
            ApplyTrimmingConfiguration(new TrimmingConfiguration
            {
                Enabled = settings.TrimEnabled,
                FinalLength = settings.FinalBookletLength,
                ClampPressure = settings.ClampPressure,
                ChipBlower = settings.ChipBlower,
            });
            ApplyConveyorConfiguration(new ConveyorConfiguration
            {
                Spacing = settings.BookletSpacing,
                Offset = settings.BookletOffset,
                FullDetection = settings.FullDetection,
            });
        }

        private void SaveConfigurationsToCurrentJob()
        {
            if (_currentJob == null)
            {
                return;
            }

            _currentJob.StfoSettings = new StfoJobSettings
            {
                PaperWidth = _savedStitching.PaperWidth,
                PaperLength = _savedStitching.PaperLength,
                StitchSpacing = _savedStitching.Spacing,
                HorizontalOffset = _savedStitching.HorizontalOffset,
                VerticalOffset = _savedStitching.VerticalOffset,
                StitchMode = _savedStitching.Mode,

                FoldEnabled = _savedFolding.Enabled,
                FoldPosition = _savedFolding.Position,
                PressureMode = _savedFolding.PressureMode,
                PressureLevel = _savedFolding.PressureLevel,

                TrimEnabled = _savedTrimming.Enabled,
                FinalBookletLength = _savedTrimming.FinalLength,
                ClampPressure = _savedTrimming.ClampPressure,
                ChipBlower = _savedTrimming.ChipBlower,

                BookletSpacing = _savedConveyor.Spacing,
                BookletOffset = _savedConveyor.Offset,
                FullDetection = _savedConveyor.FullDetection,
            };
        }

        private void SaveAllCurrentConfigurations()
        {
            _savedStitching = CaptureStitchingConfiguration();
            _savedFolding = CaptureFoldingConfiguration();
            _savedTrimming = CaptureTrimmingConfiguration();
            _savedConveyor = CaptureConveyorConfiguration();
        }

        private void SaveCurrentConfiguration(int step)
        {
            switch (step)
            {
                case StitchingStep:
                    _savedStitching = CaptureStitchingConfiguration();
                    break;
                case FoldingStep:
                    _savedFolding = CaptureFoldingConfiguration();
                    break;
                case TrimmingStep:
                    _savedTrimming = CaptureTrimmingConfiguration();
                    break;
                case ConveyorStep:
                    _savedConveyor = CaptureConveyorConfiguration();
                    break;
            }
        }

        private void RestoreSavedConfiguration(int step)
        {
            switch (step)
            {
                case StitchingStep:
                    ApplyStitchingConfiguration(_savedStitching);
                    break;
                case FoldingStep:
                    ApplyFoldingConfiguration(_savedFolding);
                    break;
                case TrimmingStep:
                    ApplyTrimmingConfiguration(_savedTrimming);
                    break;
                case ConveyorStep:
                    ApplyConveyorConfiguration(_savedConveyor);
                    break;
            }
        }

        private void ApplyDefaultConfiguration(int step)
        {
            switch (step)
            {
                case StitchingStep:
                    ApplyStitchingConfiguration(CreateDefaultStitchingConfiguration());
                    break;
                case FoldingStep:
                    ApplyFoldingConfiguration(CreateDefaultFoldingConfiguration());
                    break;
                case TrimmingStep:
                    ApplyTrimmingConfiguration(CreateDefaultTrimmingConfiguration());
                    break;
                case ConveyorStep:
                    ApplyConveyorConfiguration(CreateDefaultConveyorConfiguration());
                    break;
            }
        }

        private StitchingConfiguration CaptureStitchingConfiguration()
        {
            return new StitchingConfiguration
            {
                PaperWidth = _paperW,
                PaperLength = _paperL,
                Spacing = _spacing,
                HorizontalOffset = _hOffset,
                VerticalOffset = _vOffset,
                Mode = _stitchMode,
            };
        }

        private FoldingConfiguration CaptureFoldingConfiguration()
        {
            return new FoldingConfiguration
            {
                Enabled = _foldEnabled,
                Position = _foldPosition,
                PressureMode = _pressureMode,
                PressureLevel = PressureSlider.Value,
            };
        }

        private TrimmingConfiguration CaptureTrimmingConfiguration()
        {
            return new TrimmingConfiguration
            {
                Enabled = _trimEnabled,
                FinalLength = _finalLength,
                ClampPressure = _clampPressure,
                ChipBlower = _chipBlower,
            };
        }

        private ConveyorConfiguration CaptureConveyorConfiguration()
        {
            return new ConveyorConfiguration
            {
                Spacing = _bookletSpacing,
                Offset = _bookletOffset,
                FullDetection = _fullDetection,
            };
        }

        private static StitchingConfiguration CreateDefaultStitchingConfiguration()
        {
            return new StitchingConfiguration
            {
                PaperWidth = DefaultPaperWidth,
                PaperLength = DefaultPaperLength,
                Spacing = DefaultStitchSpacing,
                HorizontalOffset = DefaultHorizontalOffset,
                VerticalOffset = DefaultVerticalOffset,
                Mode = DefaultStitchMode,
            };
        }

        private static FoldingConfiguration CreateDefaultFoldingConfiguration()
        {
            return new FoldingConfiguration
            {
                Enabled = DefaultFoldEnabled,
                Position = DefaultFoldPosition,
                PressureMode = DefaultPressureMode,
                PressureLevel = DefaultPressureLevel,
            };
        }

        private static TrimmingConfiguration CreateDefaultTrimmingConfiguration()
        {
            return new TrimmingConfiguration
            {
                Enabled = DefaultTrimEnabled,
                FinalLength = DefaultFinalLength,
                ClampPressure = DefaultClampPressure,
                ChipBlower = DefaultChipBlower,
            };
        }

        private static ConveyorConfiguration CreateDefaultConveyorConfiguration()
        {
            return new ConveyorConfiguration
            {
                Spacing = DefaultBookletSpacing,
                Offset = DefaultBookletOffset,
                FullDetection = DefaultFullDetection,
            };
        }

        private void ApplyStitchingConfiguration(StitchingConfiguration configuration)
        {
            if (configuration == null)
            {
                return;
            }

            bool wasLoaded = _loaded;
            _loaded = false;
            _paperW = configuration.PaperWidth;
            _paperL = configuration.PaperLength;
            _spacing = configuration.Spacing;
            _hOffset = configuration.HorizontalOffset;
            _vOffset = configuration.VerticalOffset;
            _stitchMode = configuration.Mode;
            PaperWidthBox.Text = DisplayValue(_paperW);
            PaperLengthBox.Text = DisplayValue(_paperL);
            SpacingBox.Text = DisplayValue(_spacing);
            HOffsetBox.Text = DisplayValue(_hOffset);
            VOffsetBox.Text = DisplayValue(_vOffset);
            _loaded = wasLoaded;
            RefreshStitchModeButtons();
            UpdateStitchSummary();
            RedrawStitchPreview();
        }

        private void ApplyFoldingConfiguration(FoldingConfiguration configuration)
        {
            if (configuration == null)
            {
                return;
            }

            bool wasLoaded = _loaded;
            _loaded = false;
            _foldEnabled = configuration.Enabled;
            _foldPosition = configuration.Position;
            _pressureMode = configuration.PressureMode == "Manual" ? "Manual" : "Auto";
            FoldPositionSlider.Value = _foldPosition;
            FoldPositionValueText.Text = DisplayValue(_foldPosition, "0.0", "0.000");
            PressureSlider.Value = configuration.PressureLevel;
            _loaded = wasLoaded;
            RefreshFoldChoiceButtons();
            UpdateFoldSummary();
            RedrawFoldPreview();
        }

        private void ApplyTrimmingConfiguration(TrimmingConfiguration configuration)
        {
            if (configuration == null)
            {
                return;
            }

            bool wasLoaded = _loaded;
            _loaded = false;
            _trimEnabled = configuration.Enabled;
            _finalLength = configuration.FinalLength;
            _clampPressure = Clamp(configuration.ClampPressure, 0, 100);
            _chipBlower = configuration.ChipBlower;
            FinalLengthBox.Text = DisplayValue(_finalLength, "0.0", "0.000");
            ClampPressureSlider.Value = _clampPressure;
            ClampPressureValueText.Text = _clampPressure.ToString("0", CultureInfo.InvariantCulture) + "%";
            _loaded = wasLoaded;
            RefreshTrimChoiceButtons();
            UpdateTrimSummary();
            RedrawTrimPreview();
        }

        private void ApplyConveyorConfiguration(ConveyorConfiguration configuration)
        {
            if (configuration == null)
            {
                return;
            }

            bool wasLoaded = _loaded;
            _loaded = false;
            _bookletSpacing = configuration.Spacing;
            _bookletOffset = configuration.Offset;
            _fullDetection = configuration.FullDetection;
            SpacingSlider.Value = _bookletSpacing;
            OffsetSlider.Value = _bookletOffset;
            SpacingValueText.Text = _bookletSpacing.ToString(CultureInfo.InvariantCulture);
            OffsetValueText.Text = _bookletOffset.ToString(CultureInfo.InvariantCulture);
            _loaded = wasLoaded;
            RefreshConveyorChoiceButtons();
            UpdateConveyorSummary();
            RedrawConveyorPreview();
        }

        // ================= Stitching form =================

        private void OnStitchNumericFieldPressed(object sender, MouseButtonEventArgs e)
        {
            var textBox = sender as TextBox;
            string field = textBox?.Tag as string ?? string.Empty;
            string title;
            string label;
            string description;
            double value;
            bool allowNegative;
            string unit = MeasurementFormatter.UnitSymbol(_measurementUnit);

            switch (field)
            {
                case "PaperWidth":
                    _pendingStitchNumericField = StitchNumericField.PaperWidth;
                    title = T("Set Paper Width");
                    label = TF("Paper width ({0})", unit);
                    description = T("Enter the sheet width used by the stitching job.");
                    value = MeasurementFormatter.ToDisplay(_paperW, _measurementUnit);
                    allowNegative = false;
                    break;
                case "PaperLength":
                    _pendingStitchNumericField = StitchNumericField.PaperLength;
                    title = T("Set Paper Length");
                    label = TF("Paper length ({0})", unit);
                    description = T("Enter the sheet length used by the stitching job.");
                    value = MeasurementFormatter.ToDisplay(_paperL, _measurementUnit);
                    allowNegative = false;
                    break;
                case "Spacing":
                    _pendingStitchNumericField = StitchNumericField.Spacing;
                    title = T("Set Stitch Spacing");
                    label = TF("Spacing between stitches ({0})", unit);
                    description = T("Enter the distance between stitch positions.");
                    value = MeasurementFormatter.ToDisplay(_spacing, _measurementUnit);
                    allowNegative = false;
                    break;
                case "HorizontalOffset":
                    _pendingStitchNumericField = StitchNumericField.HorizontalOffset;
                    title = T("Set Horizontal Offset");
                    label = TF("Horizontal offset ({0})", unit);
                    description = T("Use a negative value for left or a positive value for right.");
                    value = MeasurementFormatter.ToDisplay(_hOffset, _measurementUnit);
                    allowNegative = true;
                    break;
                case "VerticalOffset":
                    _pendingStitchNumericField = StitchNumericField.VerticalOffset;
                    title = T("Set Vertical Offset");
                    label = TF("Vertical offset ({0})", unit);
                    description = T("Use a negative value for front or a positive value for rear.");
                    value = MeasurementFormatter.ToDisplay(_vOffset, _measurementUnit);
                    allowNegative = true;
                    break;
                default:
                    return;
            }

            e.Handled = true;
            StitchNumericDialog.Open(title, label, description, value, allowNegative);
        }

        private void OnStitchNumericValueConfirmed(object sender, double value)
        {
            double millimeters = MeasurementFormatter.ToMillimeters(value, _measurementUnit);
            string formatted = DisplayValue(millimeters);
            string fieldLabel;

            switch (_pendingStitchNumericField)
            {
                case StitchNumericField.PaperWidth:
                    _paperW = millimeters;
                    PaperWidthBox.Text = formatted;
                    fieldLabel = "Paper width";
                    break;
                case StitchNumericField.PaperLength:
                    _paperL = millimeters;
                    PaperLengthBox.Text = formatted;
                    fieldLabel = "Paper length";
                    break;
                case StitchNumericField.Spacing:
                    _spacing = millimeters;
                    SpacingBox.Text = formatted;
                    fieldLabel = "Stitch spacing";
                    break;
                case StitchNumericField.HorizontalOffset:
                    _hOffset = millimeters;
                    HOffsetBox.Text = formatted;
                    fieldLabel = "Horizontal offset";
                    break;
                case StitchNumericField.VerticalOffset:
                    _vOffset = millimeters;
                    VOffsetBox.Text = formatted;
                    fieldLabel = "Vertical offset";
                    break;
                case StitchNumericField.FoldPosition:
                    millimeters = Clamp(millimeters, FoldPositionSlider.Minimum, FoldPositionSlider.Maximum);
                    formatted = DisplayValue(millimeters);
                    FoldPositionSlider.Value = millimeters;
                    fieldLabel = "Fold position";
                    break;
                case StitchNumericField.FinalBookletLength:
                    millimeters = Clamp(millimeters, 50, 350);
                    formatted = DisplayValue(millimeters);
                    SetFinalLength(millimeters);
                    fieldLabel = "Final booklet length";
                    break;
                default:
                    return;
            }

            FooterStatusText.Text = TF("{0} updated to {1} {2}.", T(fieldLabel), formatted,
                MeasurementFormatter.UnitSymbol(_measurementUnit));
        }

        private void OnParamChanged(object sender, TextChangedEventArgs e)
        {
            if (!_loaded)
            {
                return;
            }

            _paperW = ParseLengthOr(PaperWidthBox.Text, _paperW);
            _paperL = ParseLengthOr(PaperLengthBox.Text, _paperL);
            _spacing = ParseLengthOr(SpacingBox.Text, _spacing);
            _hOffset = ParseLengthOr(HOffsetBox.Text, _hOffset);
            _vOffset = ParseLengthOr(VOffsetBox.Text, _vOffset);

            UpdateStitchSummary();
            RedrawStitchPreview();
        }

        private void OnStitchModeClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string mode)
            {
                _stitchMode = mode;
                RefreshStitchModeButtons();
                UpdateStitchSummary();
                RedrawStitchPreview();
            }
        }

        private static double ParseOr(string text, double fallback)
        {
            return double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double value)
                ? value
                : fallback;
        }

        private double ParseLengthOr(string text, double fallbackMillimeters)
        {
            double displayValue;
            return double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out displayValue)
                ? MeasurementFormatter.ToMillimeters(displayValue, _measurementUnit)
                : fallbackMillimeters;
        }

        private string DisplayValue(double millimeters, string metricFormat = "0.0##",
            string inchFormat = "0.000")
        {
            return MeasurementFormatter.FormatValue(
                millimeters, _measurementUnit, metricFormat, inchFormat);
        }

        private string DisplayLength(double millimeters, string metricFormat = "0.0##",
            string inchFormat = "0.000")
        {
            return MeasurementFormatter.FormatLength(
                millimeters, _measurementUnit, metricFormat, inchFormat);
        }

        private double MeasurementStepMillimeters()
        {
            // Physical steppers move by 1 mm in metric or 0.1 in in imperial.
            return _measurementUnit == MeasurementUnit.Inches
                ? MeasurementFormatter.MillimetersPerInch / 10.0
                : 1.0;
        }

        private void RefreshStitchModeButtons()
        {
            var selBorder = (Brush)FindResource("JobsAccentBrush");
            var selBg = (Brush)FindResource("StatusIdleBgBrush");
            var selText = (Brush)FindResource("JobsAccentBrush");
            var normBorder = (Brush)FindResource("CardBorderBrush");
            var normBg = (Brush)FindResource("CardBackgroundBrush");
            var normText = (Brush)FindResource("TextSecondaryBrush");

            for (int i = 0; i < _modeButtons.Length; i++)
            {
                bool isSelected = (string)_modeButtons[i].Tag == _stitchMode;
                _modeButtons[i].BorderBrush = isSelected ? selBorder : normBorder;
                _modeButtons[i].BorderThickness = new Thickness(isSelected ? 2 : 1);
                _modeButtons[i].Background = isSelected ? selBg : normBg;
                _modeLabels[i].Foreground = isSelected ? selText : normText;
                _modeLabels[i].FontWeight = isSelected ? FontWeights.SemiBold : FontWeights.Normal;
            }
        }

        private void UpdateStitchSummary()
        {
            SummaryPaperSize.Text = MeasurementFormatter.FormatDimensions(
                _paperW, _paperL, _measurementUnit);
            SummaryStitchMode.Text = T(_stitchMode);
            SummarySpacing.Text = DisplayLength(_spacing, "0.0", "0.000");
            SummaryHOffset.Text = DisplayLength(_hOffset, "0.0", "0.000");
            SummaryVOffset.Text = DisplayLength(_vOffset, "0.0", "0.000");
        }

        private static string Fmt(double value, string format)
        {
            return value.ToString(format, CultureInfo.InvariantCulture);
        }

        private static string T(string source)
        {
            return LocalizationManager.Translate(source);
        }

        private static string TF(string source, params object[] values)
        {
            return string.Format(CultureInfo.CurrentCulture, T(source), values);
        }

        // ---- Live preview drawing ----
        //
        // Draws the sheet to the current W x L aspect with width/length
        // dimension lines, then places stitch marks for the selected mode
        // (spacing / offsets in mm are scaled to the drawn sheet). Redrawn on
        // every parameter or mode change.

        private void RedrawStitchPreview()
        {
            var canvas = StitchPreviewCanvas;
            canvas.Children.Clear();

            if (_paperW <= 0 || _paperL <= 0)
            {
                return;
            }

            var sheetStroke = (Brush)FindResource("TextSecondaryBrush");
            var sheetFill = (Brush)FindResource("CardBackgroundBrush");
            var dimBrush = (Brush)FindResource("TextMutedBrush");
            var labelBrush = (Brush)FindResource("TextSecondaryBrush");
            var stitchBrush = (Brush)FindResource("TextSecondaryBrush");
            var foldBrush = (Brush)FindResource("TextMutedBrush");
            var pathBrush = (Brush)FindResource("JobsAccentBrush");

            const double left = 90, top = 46, right = 20, bottom = 58;
            double areaW = canvas.Width - left - right;
            double areaH = canvas.Height - top - bottom;

            double scale = Math.Min(areaW / _paperW, areaH / _paperL);
            double sheetW = _paperW * scale;
            double sheetH = _paperL * scale;
            double x0 = left + (areaW - sheetW) / 2;
            double y0 = top + (areaH - sheetH) / 2;

            // Sheet
            canvas.Children.Add(Positioned(new Rectangle
            {
                Width = sheetW,
                Height = sheetH,
                RadiusX = 3,
                RadiusY = 3,
                Fill = sheetFill,
                Stroke = sheetStroke,
                StrokeThickness = 1.5,
            }, x0, y0));

            // Width dimension line (above the sheet)
            double wy = y0 - 16;
            AddLine(canvas, x0, wy, x0 + sheetW, wy, dimBrush, 1);
            AddLine(canvas, x0, wy - 4, x0, wy + 4, dimBrush, 1);
            AddLine(canvas, x0 + sheetW, wy - 4, x0 + sheetW, wy + 4, dimBrush, 1);
            AddLabel(canvas, DisplayLength(_paperW, "0.#", "0.000"), x0, y0 - 40, sheetW, labelBrush, TextAlignment.Center);

            // Length dimension line (left of the sheet)
            double lx = x0 - 16;
            AddLine(canvas, lx, y0, lx, y0 + sheetH, dimBrush, 1);
            AddLine(canvas, lx - 4, y0, lx + 4, y0, dimBrush, 1);
            AddLine(canvas, lx - 4, y0 + sheetH, lx + 4, y0 + sheetH, dimBrush, 1);
            AddLabel(canvas, DisplayLength(_paperL, "0.#", "0.000"), 2, y0 + sheetH / 2 - 9, left - 22, labelBrush, TextAlignment.Right);

            double pad = 16;
            double pxPerMm = scale;
            double cx = Clamp(x0 + sheetW / 2 + _hOffset * pxPerMm, x0 + pad, x0 + sheetW - pad);
            double cy = Clamp(y0 + sheetH / 2 + _vOffset * pxPerMm, y0 + pad, y0 + sheetH - pad);
            // Saddle/Top place a pair of stitches; "spacing" is the gap between
            // the two (half either side of the centre).
            double halfGap = Math.Max(4, _spacing * pxPerMm / 2);

            switch (_stitchMode)
            {
                case "Saddle":
                    // Centre fold (the spine) + a pair of stitches on it.
                    canvas.Children.Add(new Line { X1 = cx, Y1 = y0, X2 = cx, Y2 = y0 + sheetH, Stroke = foldBrush, StrokeThickness = 1, StrokeDashArray = new DoubleCollection { 3, 3 } });
                    AddStitch(canvas, cx, Clamp(cy - halfGap, y0 + pad, y0 + sheetH - pad), 3, 14, -18, stitchBrush);
                    AddStitch(canvas, cx, Clamp(cy + halfGap, y0 + pad, y0 + sheetH - pad), 3, 14, -18, stitchBrush);
                    break;

                case "Top":
                    double ty = Clamp(y0 + 18 + _vOffset * pxPerMm, y0 + pad, y0 + sheetH - pad);
                    AddStitch(canvas, Clamp(cx - halfGap, x0 + pad, x0 + sheetW - pad), ty, 3, 14, -18, stitchBrush);
                    AddStitch(canvas, Clamp(cx + halfGap, x0 + pad, x0 + sheetW - pad), ty, 3, 14, -18, stitchBrush);
                    break;

                case "Right Corner":
                    AddStitch(canvas, x0 + sheetW - 20, y0 + 20, 3, 18, -38, stitchBrush);
                    break;

                case "Left Corner":
                    AddStitch(canvas, x0 + 20, y0 + 20, 3, 18, 38, stitchBrush);
                    break;

                case "None":
                    AddLabel(canvas, T("No stitching"), x0, y0 + sheetH / 2 - 10, sheetW, dimBrush, TextAlignment.Center);
                    break;
            }

            double pathY = canvas.Height - 18;
            AddHArrow(canvas, 52, 288, pathY, pathBrush, false);
            var pathLabel = new TextBlock
            {
                Text = T("INFEED  \u2192  PAPER PATH  \u2192  OUTPUT"),
                Width = 236,
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = pathBrush,
                TextAlignment = TextAlignment.Center,
            };
            canvas.Children.Add(Positioned(pathLabel, 52, pathY - 26));
        }

        private static double Clamp(double value, double min, double max)
        {
            if (max < min) return (min + max) / 2;
            return Math.Max(min, Math.Min(max, value));
        }

        private static UIElement Positioned(UIElement element, double x, double y)
        {
            Canvas.SetLeft(element, x);
            Canvas.SetTop(element, y);
            return element;
        }

        private static void AddLine(Canvas canvas, double x1, double y1, double x2, double y2, Brush brush, double thickness)
        {
            canvas.Children.Add(new Line { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2, Stroke = brush, StrokeThickness = thickness });
        }

        private void AddStitch(Canvas canvas, double cx, double cy, double w, double h, double angle, Brush brush)
        {
            var rect = new Rectangle { Width = w, Height = h, RadiusX = 1, RadiusY = 1, Fill = brush };
            if (angle != 0)
            {
                rect.RenderTransformOrigin = new Point(0.5, 0.5);
                rect.RenderTransform = new RotateTransform(angle);
            }
            canvas.Children.Add(Positioned(rect, cx - w / 2, cy - h / 2));
        }

        private void AddLabel(Canvas canvas, string text, double x, double y, double width, Brush brush, TextAlignment alignment)
        {
            var block = new TextBlock
            {
                Text = text,
                Width = width,
                FontSize = 13,
                Foreground = brush,
                TextAlignment = alignment,
            };
            canvas.Children.Add(Positioned(block, x, y));
        }

        // ================= Folding form =================

        private void OnFoldFunctionClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tag)
            {
                _foldEnabled = tag == "Enabled";
                RefreshFoldChoiceButtons();
                UpdateFoldSummary();
                RedrawFoldPreview();
            }
        }

        private void OnPressureModeClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string mode)
            {
                _pressureMode = mode;
                RefreshFoldChoiceButtons();
                UpdateFoldSummary();
            }
        }

        private void OnFoldMinusClick(object sender, RoutedEventArgs e)
        {
            FoldPositionSlider.Value = Math.Max(
                FoldPositionSlider.Minimum, FoldPositionSlider.Value - MeasurementStepMillimeters());
        }

        private void OnFoldPlusClick(object sender, RoutedEventArgs e)
        {
            FoldPositionSlider.Value = Math.Min(
                FoldPositionSlider.Maximum, FoldPositionSlider.Value + MeasurementStepMillimeters());
        }

        private void OnFoldPositionInputPressed(object sender, MouseButtonEventArgs e)
        {
            if (!FoldPositionSlider.IsEnabled)
            {
                return;
            }

            e.Handled = true;
            _pendingStitchNumericField = StitchNumericField.FoldPosition;
            string unit = MeasurementFormatter.UnitSymbol(_measurementUnit);
            StitchNumericDialog.Open(
                T("Set Fold Position"),
                TF("Fold position ({0})", unit),
                TF("Enter a value from {0} (backward) to {1} (forward).",
                    DisplayLength(-50), DisplayLength(50)),
                MeasurementFormatter.ToDisplay(_foldPosition, _measurementUnit),
                true);
        }

        private void OnFoldPositionChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_loaded)
            {
                return;
            }

            _foldPosition = FoldPositionSlider.Value;
            FoldPositionValueText.Text = DisplayValue(_foldPosition, "0.0", "0.000");
            UpdateFoldSummary();
            RedrawFoldPreview();
        }

        private void OnPressureChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Pressure level is internal state only in this prototype; the mode
            // (Auto / Manual) is what the summary reflects.
        }

        private void RefreshFoldChoiceButtons()
        {
            SetChoiceSelected(FoldEnabledButton, _foldEnabled);
            SetChoiceSelected(FoldDisabledButton, !_foldEnabled);
            SetChoiceSelected(PressureAutoButton, _pressureMode == "Auto");
            SetChoiceSelected(PressureManualButton, _pressureMode == "Manual");

            // Fold position only matters when folding is on; pressure is only
            // hand-set in Manual mode (Auto drives it automatically).
            FoldMinusButton.IsEnabled = _foldEnabled;
            FoldPlusButton.IsEnabled = _foldEnabled;
            FoldPositionSlider.IsEnabled = _foldEnabled;
            PressureAutoButton.IsEnabled = _foldEnabled;
            PressureManualButton.IsEnabled = _foldEnabled;
            PressureSlider.IsEnabled = _foldEnabled && _pressureMode == "Manual";
        }

        private void SetChoiceSelected(Button button, bool selected)
        {
            button.Background = (Brush)FindResource(selected ? "JobsAccentBrush" : "CardBackgroundBrush");
            button.Foreground = (Brush)FindResource(selected ? "JobsAccentForegroundBrush" : "TextPrimaryBrush");
            button.BorderBrush = (Brush)FindResource(selected ? "JobsAccentBrush" : "OutlineButtonBorderBrush");
        }

        private void UpdateFoldSummary()
        {
            FoldSummaryFolding.Text = T(_foldEnabled ? "Enabled" : "Disabled");
            FoldSummaryPosition.Text = _foldEnabled
                ? DisplayLength(_foldPosition, "0.00", "0.000")
                : T("Bypass to top tray");
            FoldSummaryPressure.Text = T(_foldEnabled ? _pressureMode : "Not used");
        }

        // ---- Folding live preview ----
        //
        // Shows the sheet moving left-to-right through the fold rollers. When
        // folding is disabled the alternate path to the top tray is explicit.

        private void RedrawFoldPreview()
        {
            var canvas = FoldPreviewCanvas;
            canvas.Children.Clear();

            var pageFill = (Brush)FindResource("CardBackgroundBrush");
            var stroke = (Brush)FindResource("TextSecondaryBrush");
            var grey = (Brush)FindResource("CardBorderBrush");
            var muted = (Brush)FindResource("TextMutedBrush");
            var navy = (Brush)FindResource("HeaderBackgroundBrush");
            var labelBrush = (Brush)FindResource("TextSecondaryBrush");

            AddLabel(canvas, T("Infeed"), 12, 14, 70, labelBrush, TextAlignment.Left);
            AddLabel(canvas, T("BBM output"), 247, 14, 82, labelBrush, TextAlignment.Right);
            AddHArrow(canvas, 48, 294, 46, navy, false);

            // Incoming sheet and its adjustable fold line.
            AddPolygon(canvas, new double[,] { { 32, 112 }, { 126, 94 }, { 126, 220 }, { 32, 238 } }, pageFill, stroke);
            AddLine(canvas, 48, 142, 107, 132, grey, 4);
            AddLine(canvas, 48, 158, 107, 148, grey, 4);

            if (!_foldEnabled)
            {
                // Disabled sheets are diverted upward to the top tray.
                AddArrow(canvas, 126, 150, 225, 82, navy);
                AddLine(canvas, 220, 78, 302, 78, stroke, 3);
                AddLine(canvas, 224, 85, 306, 85, grey, 3);
                AddLine(canvas, 228, 92, 310, 92, grey, 3);
                AddLabel(canvas, T("Top tray"), 224, 100, 86, labelBrush, TextAlignment.Center);
                AddLabel(canvas, T("Folding disabled: sheet bypasses fold rollers"),
                    5, 276, 330, muted, TextAlignment.Center);
                return;
            }

            const double centerX = 79;
            const double pxPerMm = 0.9;
            double foldX = Clamp(centerX + _foldPosition * pxPerMm, 48, 110);
            canvas.Children.Add(new Line
            {
                X1 = foldX,
                Y1 = 102,
                X2 = foldX,
                Y2 = 229,
                Stroke = muted,
                StrokeThickness = 1.5,
                StrokeDashArray = new DoubleCollection { 4, 3 },
            });

            // Fold rollers and paper movement through them.
            canvas.Children.Add(Positioned(new Ellipse
            {
                Width = 34, Height = 34, Fill = grey, Stroke = stroke, StrokeThickness = 1.5,
            }, 146, 120));
            canvas.Children.Add(Positioned(new Ellipse
            {
                Width = 34, Height = 34, Fill = grey, Stroke = stroke, StrokeThickness = 1.5,
            }, 146, 170));
            AddArrow(canvas, 124, 163, 193, 163, navy);
            AddLabel(canvas, T("Fold rollers"), 126, 212, 76, muted, TextAlignment.Center);

            // A clearly folded, closed booklet leaving the rollers.
            AddPolygon(canvas, new double[,] { { 208, 126 }, { 292, 143 }, { 292, 218 }, { 208, 202 } }, pageFill, stroke);
            AddPolygon(canvas, new double[,] { { 208, 126 }, { 218, 119 }, { 302, 136 }, { 292, 143 } }, grey, stroke);
            AddPolygon(canvas, new double[,] { { 292, 143 }, { 302, 136 }, { 302, 211 }, { 292, 218 } }, grey, stroke);
            AddLine(canvas, 224, 153, 276, 164, grey, 4);
            AddLine(canvas, 224, 169, 276, 180, grey, 4);
            AddLabel(canvas, T("Folded booklet"), 214, 232, 92, labelBrush, TextAlignment.Center);

            double half = Clamp(Math.Abs(_foldPosition) * 0.9, 18, 54);
            AddHArrow(canvas, centerX - half, centerX + half, 264, navy, true);
            AddLabel(canvas, T("Offset from centre"), 18, 276, 126, labelBrush, TextAlignment.Center);
        }

        private void AddPolygon(Canvas canvas, double[,] points, Brush fill, Brush stroke)
        {
            var polygon = new Polygon { Fill = fill, Stroke = stroke, StrokeThickness = 1.5 };
            var collection = new PointCollection();
            for (int i = 0; i < points.GetLength(0); i++)
            {
                collection.Add(new Point(points[i, 0], points[i, 1]));
            }
            polygon.Points = collection;
            canvas.Children.Add(polygon);
        }

        private void AddRect(Canvas canvas, double x, double y, double w, double h, Brush fill)
        {
            canvas.Children.Add(Positioned(new Rectangle { Width = w, Height = h, Fill = fill }, x, y));
        }

        private void AddHArrow(Canvas canvas, double x1, double x2, double y, Brush brush, bool doubleHead)
        {
            AddLine(canvas, x1, y, x2, y, brush, 2);
            int dir = x2 >= x1 ? 1 : -1;
            AddArrowHead(canvas, x2, y, dir, brush);
            if (doubleHead)
            {
                AddArrowHead(canvas, x1, y, -dir, brush);
            }
        }

        private void AddArrow(Canvas canvas, double x1, double y1, double x2, double y2, Brush brush)
        {
            AddLine(canvas, x1, y1, x2, y2, brush, 2);
            double dx = x2 - x1;
            double dy = y2 - y1;
            double length = Math.Sqrt(dx * dx + dy * dy);
            if (length < 0.001)
            {
                return;
            }

            double ux = dx / length;
            double uy = dy / length;
            double baseX = x2 - ux * 11;
            double baseY = y2 - uy * 11;
            double px = -uy * 6;
            double py = ux * 6;
            var head = new Polygon { Fill = brush };
            head.Points = new PointCollection
            {
                new Point(x2, y2),
                new Point(baseX + px, baseY + py),
                new Point(baseX - px, baseY - py),
            };
            canvas.Children.Add(head);
        }

        private void AddArrowHead(Canvas canvas, double tipX, double y, int dir, Brush brush)
        {
            var head = new Polygon { Fill = brush };
            head.Points = new PointCollection
            {
                new Point(tipX, y),
                new Point(tipX - dir * 10, y - 6),
                new Point(tipX - dir * 10, y + 6),
            };
            canvas.Children.Add(head);
        }

        // ================= Trimming form =================

        private void OnTrimFunctionClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tag)
            {
                _trimEnabled = tag == "Enabled";
                RefreshTrimChoiceButtons();
                UpdateTrimSummary();
                RedrawTrimPreview();
            }
        }

        private void OnChipBlowerClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tag)
            {
                _chipBlower = tag == "On";
                RefreshTrimChoiceButtons();
                UpdateTrimSummary();
            }
        }

        private void OnClampPressureChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_loaded)
            {
                return;
            }

            _clampPressure = Math.Round(ClampPressureSlider.Value / 10.0) * 10.0;
            ClampPressureValueText.Text = _clampPressure.ToString("0", CultureInfo.InvariantCulture) + "%";
            UpdateTrimSummary();
            RedrawTrimPreview();
        }

        private void OnFinalLengthChanged(object sender, TextChangedEventArgs e)
        {
            if (!_loaded)
            {
                return;
            }

            _finalLength = ParseLengthOr(FinalLengthBox.Text, _finalLength);
            UpdateTrimSummary();
            RedrawTrimPreview();
        }

        private void OnFinalLengthMinus(object sender, RoutedEventArgs e)
        {
            SetFinalLength(_finalLength - MeasurementStepMillimeters());
        }

        private void OnFinalLengthPlus(object sender, RoutedEventArgs e)
        {
            SetFinalLength(_finalLength + MeasurementStepMillimeters());
        }

        private void OnFinalLengthInputPressed(object sender, MouseButtonEventArgs e)
        {
            if (!FinalLengthBox.IsEnabled)
            {
                return;
            }

            e.Handled = true;
            _pendingStitchNumericField = StitchNumericField.FinalBookletLength;
            string unit = MeasurementFormatter.UnitSymbol(_measurementUnit);
            StitchNumericDialog.Open(
                T("Set Final Booklet Length"),
                TF("Final booklet length ({0})", unit),
                TF("Enter the desired finished length from {0} to {1}.",
                    DisplayLength(50), DisplayLength(350)),
                MeasurementFormatter.ToDisplay(_finalLength, _measurementUnit),
                false);
        }

        private void SetFinalLength(double value)
        {
            // Setting the box text raises OnFinalLengthChanged, which updates
            // the state, summary and preview.
            FinalLengthBox.Text = DisplayValue(Clamp(value, 50, 350), "0.0", "0.000");
        }

        private void RefreshTrimChoiceButtons()
        {
            SetChoiceSelected(TrimEnabledButton, _trimEnabled);
            SetChoiceSelected(TrimDisabledButton, !_trimEnabled);
            SetChoiceSelected(ChipOnButton, _chipBlower);
            SetChoiceSelected(ChipOffButton, !_chipBlower);
            // Length and clamp pressure only matter while trimming is on.
            FinalLengthMinus.IsEnabled = _trimEnabled;
            FinalLengthPlus.IsEnabled = _trimEnabled;
            FinalLengthBox.IsEnabled = _trimEnabled;
            ClampPressureSlider.IsEnabled = _trimEnabled;
        }

        private void UpdateTrimSummary()
        {
            TrimSummaryTrimming.Text = T(_trimEnabled ? "Enabled" : "Disabled");
            TrimSummaryLength.Text = DisplayLength(_finalLength, "0.0", "0.000");
            TrimSummaryTotal.Text = DisplayLength(TotalBookletLength, "0.0", "0.000");
            TrimSummaryStrip.Text = DisplayLength(TrimmedStripLength, "0.0", "0.000");
            TotalBookletLengthText.Text = DisplayLength(TotalBookletLength, "0.0", "0.000");
            TrimmedStripLengthText.Text = DisplayLength(TrimmedStripLength, "0.0", "0.000");
            TrimSummaryClamp.Text = _clampPressure.ToString("0", CultureInfo.InvariantCulture) + "%";
            TrimSummaryChip.Text = T(_chipBlower ? "On" : "Off");
        }

        private double TotalBookletLength => Math.Max(_paperW, _finalLength);

        private double TrimmedStripLength => _trimEnabled
            ? Math.Max(0, TotalBookletLength - _finalLength)
            : 0;

        // ---- Trimming live preview ----
        //
        // Top: the booklet drawn front-on, its width scaled to the final
        // booklet length (with dimension lines for finished, total and strip).
        // Bottom: a side view of the clamp whose marker follows the graduated
        // pressure setting.

        private void RedrawTrimPreview()
        {
            var canvas = TrimPreviewCanvas;
            canvas.Children.Clear();

            var stroke = (Brush)FindResource("TextSecondaryBrush");
            var fill = (Brush)FindResource("CardBackgroundBrush");
            var grey = (Brush)FindResource("CardBorderBrush");
            var muted = (Brush)FindResource("TextMutedBrush");
            var navy = (Brush)FindResource("HeaderBackgroundBrush");
            var label = (Brush)FindResource("TextSecondaryBrush");

            // Top booklet - total width is shown, with the removable strip
            // separated from the finished booklet at the trim line.
            const double pxPerMm = 0.72;
            double totalW = Clamp(TotalBookletLength * pxPerMm, 100, 215);
            double finishedRatio = TotalBookletLength <= 0 ? 1 : _finalLength / TotalBookletLength;
            double finishedW = Clamp(totalW * finishedRatio, 72, totalW);
            double stripW = Math.Max(0, totalW - finishedW);
            double h = 118;
            double bx = 160 - totalW / 2;
            double by = 72;

            canvas.Children.Add(Positioned(new Rectangle
            {
                Width = finishedW,
                Height = h,
                RadiusX = 5,
                RadiusY = 5,
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = 1.5,
            }, bx, by));
            AddLine(canvas, bx + 6, by + 6, bx + 6, by + h - 6, muted, 1);
            AddLine(canvas, bx + 18, by + 34, bx + finishedW - 14, by + 34, grey, 4);
            AddLine(canvas, bx + 18, by + 50, bx + finishedW - 14, by + 50, grey, 4);
            AddLine(canvas, bx + 18, by + 66, bx + finishedW - 30, by + 66, grey, 4);

            AddHArrow(canvas, bx, bx + finishedW, by - 16, navy, true);
            AddLabel(canvas, TF("Finished {0}", DisplayLength(_finalLength, "0.0", "0.000")),
                bx - 30, by - 40, finishedW + 60, label, TextAlignment.Center);

            if (_trimEnabled && stripW > 0.1)
            {
                double tx = bx + finishedW;
                canvas.Children.Add(new Line
                {
                    X1 = tx,
                    Y1 = by - 4,
                    X2 = tx,
                    Y2 = by + h + 4,
                    Stroke = muted,
                    StrokeThickness = 1.5,
                    StrokeDashArray = new DoubleCollection { 4, 3 },
                });
                canvas.Children.Add(Positioned(new Rectangle
                {
                    Width = Math.Max(8, stripW),
                    Height = h,
                    Fill = grey,
                    Stroke = muted,
                    StrokeThickness = 1,
                }, tx + 4, by));
                AddLabel(canvas, TF("Trim strip {0}", DisplayLength(TrimmedStripLength, "0.0", "0.000")),
                    tx - 18, by + h + 7, Math.Max(80, stripW + 36), label, TextAlignment.Center);
            }

            AddHArrow(canvas, bx, bx + totalW, by + h + 38, muted, true);
            AddLabel(canvas, TF("Total before trim {0}", DisplayLength(TotalBookletLength, "0.0", "0.000")),
                bx - 30, by + h + 48, totalW + 60, label, TextAlignment.Center);

            // Bottom side view of the clamp conveyor.
            const double baseY = 326;
            canvas.Children.Add(Positioned(new Rectangle
            {
                Width = 120,
                Height = 16,
                RadiusX = 2,
                RadiusY = 2,
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = 1.2,
            }, 80, 270));
            AddLine(canvas, 86, 276, 200, 276, grey, 1.5);
            AddLine(canvas, 86, 281, 200, 281, grey, 1.5);
            AddPolygon(canvas, new double[,] { { 70, 326 }, { 220, 326 }, { 220, 288 } }, grey, stroke);
            if (_trimEnabled)
            {
                AddRect(canvas, 232, 276, 8, 50, grey);
            }

            double heightMarker = 22 + _clampPressure / 100.0 * 48;
            AddVArrow(canvas, 256, baseY - heightMarker, baseY, navy);
            AddLabel(canvas, TF("Clamp {0}%", _clampPressure.ToString("0", CultureInfo.InvariantCulture)),
                268, baseY - heightMarker / 2 - 9, 68, label, TextAlignment.Left);
        }

        private void AddVArrow(Canvas canvas, double x, double y1, double y2, Brush brush)
        {
            AddLine(canvas, x, y1, x, y2, brush, 2);
            AddVHead(canvas, x, y2, y2 >= y1 ? 1 : -1, brush);
            AddVHead(canvas, x, y1, y2 >= y1 ? -1 : 1, brush);
        }

        private void AddVHead(Canvas canvas, double x, double tipY, int dir, Brush brush)
        {
            var head = new Polygon { Fill = brush };
            head.Points = new PointCollection
            {
                new Point(x, tipY),
                new Point(x - 6, tipY - dir * 10),
                new Point(x + 6, tipY - dir * 10),
            };
            canvas.Children.Add(head);
        }

        // ================= Conveyor form =================

        private void OnSpacingSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_loaded)
            {
                return;
            }

            _bookletSpacing = (int)Math.Round(SpacingSlider.Value);
            SpacingValueText.Text = _bookletSpacing.ToString(CultureInfo.InvariantCulture);
            UpdateConveyorSummary();
            RedrawConveyorPreview();
        }

        private void OnSpacingMinus(object sender, RoutedEventArgs e)
        {
            SpacingSlider.Value = Math.Max(SpacingSlider.Minimum, Math.Round(SpacingSlider.Value) - 1);
        }

        private void OnSpacingPlus(object sender, RoutedEventArgs e)
        {
            SpacingSlider.Value = Math.Min(SpacingSlider.Maximum, Math.Round(SpacingSlider.Value) + 1);
        }

        private void OnOffsetSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_loaded)
            {
                return;
            }

            _bookletOffset = (int)Math.Round(OffsetSlider.Value);
            OffsetValueText.Text = _bookletOffset.ToString(CultureInfo.InvariantCulture);
            UpdateConveyorSummary();
            RedrawConveyorPreview();
        }

        private void OnOffsetMinus(object sender, RoutedEventArgs e)
        {
            OffsetSlider.Value = Math.Max(OffsetSlider.Minimum, Math.Round(OffsetSlider.Value) - 1);
        }

        private void OnOffsetPlus(object sender, RoutedEventArgs e)
        {
            OffsetSlider.Value = Math.Min(OffsetSlider.Maximum, Math.Round(OffsetSlider.Value) + 1);
        }

        private void OnConveyorNumericInputPressed(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            string field = border?.Tag as string ?? string.Empty;
            string title;
            string label;
            string description;
            int value;

            if (field == "Spacing")
            {
                _pendingConveyorNumericField = ConveyorNumericField.Spacing;
                title = T("Set Booklet Spacing");
                label = T("Booklet spacing");
                description = T("Enter the output conveyor advance from 1 to 30.");
                value = _bookletSpacing;
            }
            else if (field == "Offset")
            {
                _pendingConveyorNumericField = ConveyorNumericField.Offset;
                title = T("Set Booklet Offset");
                label = T("Booklet offset");
                description = T("Enter how often a booklet should be offset, from 1 to 30.");
                value = _bookletOffset;
            }
            else
            {
                return;
            }

            e.Handled = true;
            ConveyorNumericDialog.Open(title, label, description, value, 1, 30,
                T("Enter a whole number from 1 to 30."));
        }

        private void OnConveyorNumericValueConfirmed(object sender, int value)
        {
            string fieldLabel;

            if (_pendingConveyorNumericField == ConveyorNumericField.Spacing)
            {
                SpacingSlider.Value = value;
                fieldLabel = "Booklet spacing";
            }
            else
            {
                OffsetSlider.Value = value;
                fieldLabel = "Booklet offset";
            }

            FooterStatusText.Text = TF("{0} updated to {1}.", T(fieldLabel),
                value.ToString(CultureInfo.InvariantCulture));
        }

        private void OnFullDetectionClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tag)
            {
                _fullDetection = tag == "Enabled";
                RefreshConveyorChoiceButtons();
                UpdateConveyorSummary();
            }
        }

        private void RefreshConveyorChoiceButtons()
        {
            SetChoiceSelected(FullDetectionEnabledButton, _fullDetection);
            SetChoiceSelected(FullDetectionDisabledButton, !_fullDetection);
        }

        private void UpdateConveyorSummary()
        {
            ConvSummarySpacing.Text = _bookletSpacing.ToString(CultureInfo.InvariantCulture);
            ConvSummaryOffset.Text = _bookletOffset.ToString(CultureInfo.InvariantCulture);
            ConvSummaryDetection.Text = T(_fullDetection ? "Enabled" : "Disabled");
        }

        // ---- Conveyor live preview ----
        //
        // Two conveyor illustrations. Top: booklets standing on the belt with
        // the gap between them scaled to the Booklet Spacing value (1 = tight,
        // 30 = wide), dimensioned with the value. Bottom: booklets with every
        // Nth one nudged forward to show the Booklet Offset grouping.

        private void RedrawConveyorPreview()
        {
            DrawSpacingPreview();
            DrawOffsetPreview();
        }

        private void DrawSpacingPreview()
        {
            var canvas = SpacingPreviewCanvas;
            canvas.Children.Clear();

            var stroke = (Brush)FindResource("TextSecondaryBrush");
            var white = (Brush)FindResource("CardBackgroundBrush");
            var grey = (Brush)FindResource("CardBorderBrush");
            var green = (Brush)FindResource("StatusRunningBrush");
            var navy = (Brush)FindResource("HeaderBackgroundBrush");

            double topY = canvas.Height - 22;
            AddRect(canvas, 12, topY, 276, 14, grey);

            const double bw = 16, bh = 34;
            double gap = 4 + (_bookletSpacing - 1) / 29.0 * 30; // 4..34 px
            double x = 26, first = -1, second = -1;
            int guard = 0;
            while (x + bw <= 284 && guard < 40)
            {
                DrawStandingBooklet(canvas, x, topY - bh, bw, bh, white, stroke, green, grey);
                if (first < 0) first = x;
                else if (second < 0) second = x;
                x += bw + gap;
                guard++;
            }

            if (second >= 0)
            {
                double leftEdge = first + bw;
                double rightEdge = second;
                double midX = (leftEdge + rightEdge) / 2;
                double ay = 18;
                AddNumberBox(canvas, midX, 8, _bookletSpacing.ToString(CultureInfo.InvariantCulture));
                if (rightEdge - leftEdge > 26)
                {
                    AddHArrow(canvas, leftEdge, midX - 16, ay, navy, false);
                    AddHArrow(canvas, rightEdge, midX + 16, ay, navy, false);
                }
            }
        }

        private void DrawOffsetPreview()
        {
            var canvas = OffsetPreviewCanvas;
            canvas.Children.Clear();

            var stroke = (Brush)FindResource("TextSecondaryBrush");
            var white = (Brush)FindResource("CardBackgroundBrush");
            var grey = (Brush)FindResource("CardBorderBrush");
            var green = (Brush)FindResource("StatusRunningBrush");
            var muted = (Brush)FindResource("TextMutedBrush");
            var navy = (Brush)FindResource("HeaderBackgroundBrush");

            double topY = canvas.Height - 22;
            AddRect(canvas, 12, topY, 276, 14, grey);

            const double bw = 16, bh = 34, gap = 8;
            double x = 26;
            int i = 0, guard = 0;
            while (x + bw <= 284 && guard < 40)
            {
                bool offset = _bookletOffset > 0 && (i + 1) % _bookletOffset == 0;
                double by = offset ? topY - bh + 8 : topY - bh;
                if (offset)
                {
                    canvas.Children.Add(new Line
                    {
                        X1 = x - 6,
                        Y1 = topY - bh - 4,
                        X2 = x - 6,
                        Y2 = topY + 12,
                        Stroke = muted,
                        StrokeThickness = 1,
                        StrokeDashArray = new DoubleCollection { 3, 3 },
                    });
                }
                DrawStandingBooklet(canvas, x, by, bw, bh, white, stroke, green, grey);
                x += bw + gap + (offset ? 10 : 0);
                i++;
                guard++;
            }

            AddNumberBox(canvas, 150, 8, _bookletOffset.ToString(CultureInfo.InvariantCulture));
            AddHArrow(canvas, 150 - 16, 40, 18, navy, false);
            AddHArrow(canvas, 150 + 16, 262, 18, navy, false);
        }

        private void DrawStandingBooklet(Canvas canvas, double x, double y, double bw, double bh, Brush fill, Brush stroke, Brush green, Brush grey)
        {
            canvas.Children.Add(Positioned(new Rectangle
            {
                Width = bw,
                Height = bh,
                RadiusX = 2,
                RadiusY = 2,
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = 1.2,
            }, x, y));
            AddRect(canvas, x, y + bh - 4, bw, 4, green);
            AddLine(canvas, x + 3, y + 5, x + bw - 3, y + 5, grey, 1);
        }

        private void AddNumberBox(Canvas canvas, double cx, double top, string text)
        {
            var white = (Brush)FindResource("CardBackgroundBrush");
            var stroke = (Brush)FindResource("OutlineButtonBorderBrush");
            var textBrush = (Brush)FindResource("TextPrimaryBrush");

            const double w = 28, h = 20;
            canvas.Children.Add(Positioned(new Rectangle
            {
                Width = w,
                Height = h,
                RadiusX = 3,
                RadiusY = 3,
                Fill = white,
                Stroke = stroke,
                StrokeThickness = 1,
            }, cx - w / 2, top));
            canvas.Children.Add(Positioned(new TextBlock
            {
                Text = text,
                Width = w,
                FontSize = 13,
                FontWeight = FontWeights.Medium,
                Foreground = textBrush,
                TextAlignment = TextAlignment.Center,
            }, cx - w / 2, top + 2));
        }
    }
}
