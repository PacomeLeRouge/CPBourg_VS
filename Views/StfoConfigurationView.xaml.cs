using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>
    /// Individual Machine Configuration screen for the STFO (booklet maker) -
    /// a five-step wizard (Menu / Stitching / Folding / Trimming / Conveyor)
    /// for a job's settings. Reached by tapping the STFO tile on the Home
    /// dashboard (see <see cref="DashboardView.NavigateToStfoRequested"/> ->
    /// MainWindow.NavigateToStfo).
    ///
    /// This implements the approved high-fidelity "Menu" step (step tab bar +
    /// Status/Machine/Job strip + Live Preview illustration). The per-step
    /// settings forms are future work, so every step currently shares that
    /// overview and differs only by the highlighted tab, the header title,
    /// the Next button label, and a step caption.
    ///
    /// Back / Reset / Save / Next drive the wizard locally - there is no WFM
    /// connection in this build.
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

        private readonly Button[] _stepTabs;
        private int _currentStep;

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

            StepCaptionText.Text = StepCaptions[_currentStep];

            bool isLast = _currentStep == StepNames.Length - 1;
            NextButtonText.Text = isLast ? "Finish" : "Next: " + StepNames[_currentStep + 1];

            FooterStatusText.Text = string.Empty;

            TitleChanged?.Invoke(this, "STFO - " + StepNames[_currentStep]);
        }

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
    }
}
