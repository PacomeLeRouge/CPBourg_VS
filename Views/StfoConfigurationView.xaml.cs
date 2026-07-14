using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>
    /// Individual Machine Configuration screen for the STFO (booklet maker) -
    /// a five-step wizard (Menu / Stitching / Folding / Trimming / Conveyor)
    /// for a job's settings. Reached by tapping the STFO tile on the Home
    /// dashboard (see <see cref="DashboardView.NavigateToStfoRequested"/>).
    ///
    /// Step 1 (Menu) and the not-yet-built steps 3-5 show the machine-line
    /// overview. Step 2 (Stitching) is the interactive step: the parameters
    /// form on the right (paper size, stitch mode, spacing, offsets) drives a
    /// live preview on the left - a sheet drawn to the W x L aspect with
    /// dimension annotations and stitch marks placed for the selected mode -
    /// via <see cref="RedrawStitchPreview"/>. All local; no WFM in this build.
    /// </summary>
    public partial class StfoConfigurationView : UserControl
    {
        private static readonly string[] StepNames =
        {
            "Menu", "Stitching", "Folding", "Trimming", "Conveyor",
        };

        private static readonly string[] StepCaptions =
        {
            "Overview and live machine preview.",
            "Configure stitching for this job.",
            "Configure folding for this job.",
            "Configure trimming for this job.",
            "Configure the conveyor / output for this job.",
        };

        private const int StitchingStep = 1;
        private const int FoldingStep = 2;
        private const int TrimmingStep = 3;

        private readonly Button[] _stepTabs;
        private int _currentStep;

        // Stitching parameters (defaults match the field text set in XAML).
        private double _paperW = 210, _paperL = 297, _spacing = 10, _hOffset, _vOffset;
        private string _stitchMode = "Saddle";

        private Button[] _modeButtons;
        private TextBlock[] _modeLabels;

        // Folding parameters (defaults match the controls set in XAML).
        private bool _foldEnabled = true;
        private double _foldPosition = 10;
        private string _pressureMode = "Manual";

        // Trimming parameters (defaults match the controls set in XAML).
        private bool _trimEnabled = true;
        private double _finalLength = 205;
        private string _clampHeight = "Auto";
        private bool _chipBlower = true;

        private Button[] _clampButtons;
        private TextBlock[] _clampLabels;

        // Suppresses the TextChanged handlers that fire while InitializeComponent
        // sets each field's initial text, before the rest of the tree exists.
        private bool _loaded;

        /// <summary>Raised when the step changes so the shell header title can
        /// follow it (e.g. "STFO - Stitching").</summary>
        public event EventHandler<string> TitleChanged;

        /// <summary>Raised to return to the dashboard - from Back on the first
        /// step, or Finish on the last.</summary>
        public event EventHandler CloseRequested;

        public StfoConfigurationView()
        {
            InitializeComponent();

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

            _clampButtons = new[] { ClampAutoButton, ClampMaximumButton, ClampMinimumButton };
            _clampLabels = new[] { ClampAutoLabel, ClampMaximumLabel, ClampMinimumLabel };
            RefreshTrimChoiceButtons();
            UpdateTrimSummary();
            RedrawTrimPreview();

            RefreshUi();
        }

        /// <summary>Resets the wizard to the first (Menu) step. Called when the
        /// dashboard navigates in, so entry always lands on Menu.</summary>
        public void ResetToStart()
        {
            _currentStep = 0;
            RefreshUi();
        }

        private void GoToStep(int step)
        {
            _currentStep = Math.Max(0, Math.Min(StepNames.Length - 1, step));
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
            StitchingContent.Visibility = isStitching ? Visibility.Visible : Visibility.Collapsed;
            FoldingContent.Visibility = isFolding ? Visibility.Visible : Visibility.Collapsed;
            TrimmingContent.Visibility = isTrimming ? Visibility.Visible : Visibility.Collapsed;
            OverviewContent.Visibility = (!isStitching && !isFolding && !isTrimming) ? Visibility.Visible : Visibility.Collapsed;

            StepCaptionText.Text = StepCaptions[_currentStep];

            BackButtonText.Text = _currentStep == 0 ? "Back" : "Back: " + StepNames[_currentStep - 1];

            bool isLast = _currentStep == StepNames.Length - 1;
            NextButtonText.Text = isLast ? "Finish" : "Next: " + StepNames[_currentStep + 1];

            FooterStatusText.Text = string.Empty;

            TitleChanged?.Invoke(this, "STFO - " + StepNames[_currentStep]);
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
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                GoToStep(_currentStep + 1);
            }
        }

        private void OnResetClick(object sender, RoutedEventArgs e)
        {
            FooterStatusText.Text = StepNames[_currentStep] + " settings reset to defaults.";
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            FooterStatusText.Text = "Configuration saved.";
        }

        // ================= Stitching form =================

        private void OnParamChanged(object sender, TextChangedEventArgs e)
        {
            if (!_loaded)
            {
                return;
            }

            _paperW = ParseOr(PaperWidthBox.Text, _paperW);
            _paperL = ParseOr(PaperLengthBox.Text, _paperL);
            _spacing = ParseOr(SpacingBox.Text, _spacing);
            _hOffset = ParseOr(HOffsetBox.Text, _hOffset);
            _vOffset = ParseOr(VOffsetBox.Text, _vOffset);

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
            SummaryPaperSize.Text = Fmt(_paperW, "0.#") + " x " + Fmt(_paperL, "0.#") + " mm";
            SummaryStitchMode.Text = _stitchMode;
            SummarySpacing.Text = Fmt(_spacing, "0.0") + " mm";
            SummaryHOffset.Text = Fmt(_hOffset, "0.0") + " mm";
            SummaryVOffset.Text = Fmt(_vOffset, "0.0") + " mm";
        }

        private static string Fmt(double value, string format)
        {
            return value.ToString(format, CultureInfo.InvariantCulture);
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

            const double left = 60, top = 46, right = 20, bottom = 20;
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
            AddLabel(canvas, Fmt(_paperW, "0.#") + " mm", x0, y0 - 40, sheetW, labelBrush, TextAlignment.Center);

            // Length dimension line (left of the sheet)
            double lx = x0 - 16;
            AddLine(canvas, lx, y0, lx, y0 + sheetH, dimBrush, 1);
            AddLine(canvas, lx - 4, y0, lx + 4, y0, dimBrush, 1);
            AddLine(canvas, lx - 4, y0 + sheetH, lx + 4, y0 + sheetH, dimBrush, 1);
            AddLabel(canvas, Fmt(_paperL, "0.#") + " mm", 2, y0 + sheetH / 2 - 9, left - 22, labelBrush, TextAlignment.Right);

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
                    AddStitch(canvas, cx, Clamp(cy - halfGap, y0 + pad, y0 + sheetH - pad), 4, 14, 0, stitchBrush);
                    AddStitch(canvas, cx, Clamp(cy + halfGap, y0 + pad, y0 + sheetH - pad), 4, 14, 0, stitchBrush);
                    break;

                case "Top":
                    double ty = Clamp(y0 + 18 + _vOffset * pxPerMm, y0 + pad, y0 + sheetH - pad);
                    AddStitch(canvas, Clamp(cx - halfGap, x0 + pad, x0 + sheetW - pad), ty, 14, 4, 0, stitchBrush);
                    AddStitch(canvas, Clamp(cx + halfGap, x0 + pad, x0 + sheetW - pad), ty, 14, 4, 0, stitchBrush);
                    break;

                case "Right Corner":
                    AddStitch(canvas, x0 + sheetW - 20, y0 + 20, 18, 4, 45, stitchBrush);
                    break;

                case "Left Corner":
                    AddStitch(canvas, x0 + 20, y0 + 20, 18, 4, -45, stitchBrush);
                    break;

                case "None":
                    AddLabel(canvas, "No stitching", x0, y0 + sheetH / 2 - 10, sheetW, dimBrush, TextAlignment.Center);
                    break;
            }
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
            FoldPositionSlider.Value = Math.Max(FoldPositionSlider.Minimum, FoldPositionSlider.Value - 1);
        }

        private void OnFoldPlusClick(object sender, RoutedEventArgs e)
        {
            FoldPositionSlider.Value = Math.Min(FoldPositionSlider.Maximum, FoldPositionSlider.Value + 1);
        }

        private void OnFoldPositionChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_loaded)
            {
                return;
            }

            _foldPosition = FoldPositionSlider.Value;
            FoldPositionValueText.Text = Fmt(_foldPosition, "0.0");
            UpdateFoldSummary();
            RedrawFoldPreview();
        }

        private void OnPressureChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Pressure level is internal state only in this prototype; the mode
            // (Auto / Default / Manual) is what the summary reflects.
        }

        private void RefreshFoldChoiceButtons()
        {
            SetChoiceSelected(FoldEnabledButton, _foldEnabled);
            SetChoiceSelected(FoldDisabledButton, !_foldEnabled);
            SetChoiceSelected(PressureAutoButton, _pressureMode == "Auto");
            SetChoiceSelected(PressureDefaultButton, _pressureMode == "Default");
            SetChoiceSelected(PressureManualButton, _pressureMode == "Manual");

            // Fold position only matters when folding is on; pressure is only
            // hand-set in Manual mode (Auto/Default drive it automatically).
            FoldMinusButton.IsEnabled = _foldEnabled;
            FoldPlusButton.IsEnabled = _foldEnabled;
            FoldPositionSlider.IsEnabled = _foldEnabled;
            PressureSlider.IsEnabled = _pressureMode == "Manual";
        }

        private void SetChoiceSelected(Button button, bool selected)
        {
            button.Background = (Brush)FindResource(selected ? "JobsAccentBrush" : "CardBackgroundBrush");
            button.Foreground = (Brush)FindResource(selected ? "JobsAccentForegroundBrush" : "TextPrimaryBrush");
            button.BorderBrush = (Brush)FindResource(selected ? "JobsAccentBrush" : "OutlineButtonBorderBrush");
        }

        private void UpdateFoldSummary()
        {
            FoldSummaryFolding.Text = _foldEnabled ? "Enabled" : "Disabled";
            FoldSummaryPosition.Text = Fmt(_foldPosition, "0.00") + " mm";
            FoldSummaryPressure.Text = _pressureMode;
        }

        // ---- Folding live preview ----
        //
        // An open booklet with a movable dashed fold line: the line (and the
        // "Fold direction" arrow) shift to whichever side is selected via the
        // fold position, and the offset arrow beneath shows the distance from
        // centre. Positive = Forward (fold shifts right); negative = Backward
        // (left).

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

            const double cx = 170;

            // Open booklet - left page recedes, right page faces the viewer.
            AddPolygon(canvas, new double[,] { { 170, 102 }, { 96, 74 }, { 96, 210 }, { 170, 250 } }, pageFill, stroke);
            AddPolygon(canvas, new double[,] { { 170, 102 }, { 250, 120 }, { 250, 252 }, { 170, 250 } }, pageFill, stroke);

            // Sample content on the front page.
            AddLine(canvas, 192, 152, 236, 152, grey, 5);
            AddLine(canvas, 192, 167, 236, 167, grey, 5);
            AddLine(canvas, 192, 182, 230, 182, grey, 5);
            AddRect(canvas, 196, 200, 26, 34, grey);

            if (_foldEnabled)
            {
                const double pxPerMm = 1.4;
                double foldX = Clamp(cx + _foldPosition * pxPerMm, 104, 246);

                canvas.Children.Add(new Line
                {
                    X1 = foldX,
                    Y1 = 86,
                    X2 = foldX,
                    Y2 = 256,
                    Stroke = muted,
                    StrokeThickness = 1.5,
                    StrokeDashArray = new DoubleCollection { 4, 3 },
                });

                double half = Clamp(Math.Abs(_foldPosition) * pxPerMm, 26, 120);
                AddHArrow(canvas, cx - half, cx + half, 284, navy, true);
                AddLabel(canvas, "Fold offset (from center)", cx - 120, 296, 240, labelBrush, TextAlignment.Center);

                if (Math.Abs(_foldPosition) > 0.001)
                {
                    AddLabel(canvas, "Fold direction", 246, 126, 92, labelBrush, TextAlignment.Center);
                    if (_foldPosition > 0)
                    {
                        AddHArrow(canvas, 262, 300, 158, navy, false);
                    }
                    else
                    {
                        AddHArrow(canvas, 300, 262, 158, navy, false);
                    }
                }
            }
            else
            {
                AddLabel(canvas, "Folding disabled", cx - 120, 286, 240, muted, TextAlignment.Center);
            }
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

        private void OnClampHeightClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string mode)
            {
                _clampHeight = mode;
                RefreshClampButtons();
                UpdateTrimSummary();
                RedrawTrimPreview();
            }
        }

        private void OnFinalLengthChanged(object sender, TextChangedEventArgs e)
        {
            if (!_loaded)
            {
                return;
            }

            _finalLength = ParseOr(FinalLengthBox.Text, _finalLength);
            UpdateTrimSummary();
            RedrawTrimPreview();
        }

        private void OnFinalLengthMinus(object sender, RoutedEventArgs e)
        {
            SetFinalLength(ParseOr(FinalLengthBox.Text, _finalLength) - 1);
        }

        private void OnFinalLengthPlus(object sender, RoutedEventArgs e)
        {
            SetFinalLength(ParseOr(FinalLengthBox.Text, _finalLength) + 1);
        }

        private void SetFinalLength(double value)
        {
            // Setting the box text raises OnFinalLengthChanged, which updates
            // the state, summary and preview.
            FinalLengthBox.Text = Fmt(Clamp(value, 50, 350), "0.0");
        }

        private void RefreshTrimChoiceButtons()
        {
            SetChoiceSelected(TrimEnabledButton, _trimEnabled);
            SetChoiceSelected(TrimDisabledButton, !_trimEnabled);
            SetChoiceSelected(ChipOnButton, _chipBlower);
            SetChoiceSelected(ChipOffButton, !_chipBlower);
            RefreshClampButtons();

            // Length and clamp height only matter while trimming is on.
            FinalLengthMinus.IsEnabled = _trimEnabled;
            FinalLengthPlus.IsEnabled = _trimEnabled;
            FinalLengthBox.IsEnabled = _trimEnabled;
            foreach (var button in _clampButtons)
            {
                button.IsEnabled = _trimEnabled;
            }
        }

        private void RefreshClampButtons()
        {
            var selBorder = (Brush)FindResource("JobsAccentBrush");
            var selBg = (Brush)FindResource("StatusIdleBgBrush");
            var selText = (Brush)FindResource("JobsAccentBrush");
            var normBorder = (Brush)FindResource("CardBorderBrush");
            var normBg = (Brush)FindResource("CardBackgroundBrush");
            var normText = (Brush)FindResource("TextSecondaryBrush");

            for (int i = 0; i < _clampButtons.Length; i++)
            {
                bool isSelected = (string)_clampButtons[i].Tag == _clampHeight;
                _clampButtons[i].BorderBrush = isSelected ? selBorder : normBorder;
                _clampButtons[i].BorderThickness = new Thickness(isSelected ? 2 : 1);
                _clampButtons[i].Background = isSelected ? selBg : normBg;
                _clampLabels[i].Foreground = isSelected ? selText : normText;
                _clampLabels[i].FontWeight = isSelected ? FontWeights.SemiBold : FontWeights.Normal;
            }
        }

        private void UpdateTrimSummary()
        {
            TrimSummaryTrimming.Text = _trimEnabled ? "Enabled" : "Disabled";
            TrimSummaryLength.Text = Fmt(_finalLength, "0.0") + " mm";
            TrimSummaryClamp.Text = _clampHeight == "Auto" ? "Automatic" : _clampHeight;
            TrimSummaryChip.Text = _chipBlower ? "On" : "Off";
        }

        // ---- Trimming live preview ----
        //
        // Top: the booklet drawn front-on, its width scaled to the final
        // booklet length (with a dimension line and, when trimming is on, the
        // trimmed fore-edge strip). Bottom: a side view of the clamp conveyor
        // whose height marker reflects the selected clamp mode.

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

            // Top booklet - width tracks the final length.
            const double pxPerMm = 0.73;
            double w = Clamp(_finalLength * pxPerMm, 80, 190);
            double h = 118;
            double bx = 150 - w / 2;
            double by = 64;

            canvas.Children.Add(Positioned(new Rectangle
            {
                Width = w,
                Height = h,
                RadiusX = 5,
                RadiusY = 5,
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = 1.5,
            }, bx, by));
            AddLine(canvas, bx + 6, by + 6, bx + 6, by + h - 6, muted, 1);
            AddLine(canvas, bx + 18, by + 34, bx + w - 14, by + 34, grey, 4);
            AddLine(canvas, bx + 18, by + 50, bx + w - 14, by + 50, grey, 4);
            AddLine(canvas, bx + 18, by + 66, bx + w - 30, by + 66, grey, 4);

            AddHArrow(canvas, bx, bx + w, by - 16, navy, true);
            AddLabel(canvas, Fmt(_finalLength, "0.0") + " mm", bx - 30, by - 38, w + 60, label, TextAlignment.Center);

            if (_trimEnabled)
            {
                double tx = bx + w + 10;
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
                AddRect(canvas, tx + 6, by, 8, h, grey);
            }

            // Bottom side view of the clamp conveyor.
            const double baseY = 300;
            canvas.Children.Add(Positioned(new Rectangle
            {
                Width = 120,
                Height = 16,
                RadiusX = 2,
                RadiusY = 2,
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = 1.2,
            }, 80, 244));
            AddLine(canvas, 86, 250, 200, 250, grey, 1.5);
            AddLine(canvas, 86, 255, 200, 255, grey, 1.5);
            AddPolygon(canvas, new double[,] { { 70, 300 }, { 220, 300 }, { 220, 262 } }, grey, stroke);
            if (_trimEnabled)
            {
                AddRect(canvas, 232, 250, 8, 50, grey);
            }

            double heightMarker = _clampHeight == "Maximum" ? 66 : _clampHeight == "Minimum" ? 26 : 46;
            AddVArrow(canvas, 256, baseY - heightMarker, baseY, navy);
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
    }
}
