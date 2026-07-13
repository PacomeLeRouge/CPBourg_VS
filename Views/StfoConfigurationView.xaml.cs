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

        private readonly Button[] _stepTabs;
        private int _currentStep;

        // Stitching parameters (defaults match the field text set in XAML).
        private double _paperW = 210, _paperL = 297, _spacing = 10, _hOffset, _vOffset;
        private string _stitchMode = "Saddle";

        private Button[] _modeButtons;
        private TextBlock[] _modeLabels;

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
            StitchingContent.Visibility = isStitching ? Visibility.Visible : Visibility.Collapsed;
            OverviewContent.Visibility = isStitching ? Visibility.Collapsed : Visibility.Visible;

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
    }
}
