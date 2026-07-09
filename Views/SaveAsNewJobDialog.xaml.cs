using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>
    /// Save the current machine setup as a new job - see
    /// SaveAsNewJobDialog.xaml for the two-state (input / conflict) design.
    /// </summary>
    public partial class SaveAsNewJobDialog : UserControl
    {
        /// <summary>Raised when Save Job is clicked in the normal input state,
        /// with the entered job name. The caller should check whether that
        /// name already exists: if not, save and close; if it does, call
        /// <see cref="ShowConflict"/> instead of closing.</summary>
        public event EventHandler<string> SaveRequested;

        /// <summary>Raised when Save Job is clicked in the conflict (overwrite)
        /// state, with the job name to overwrite.</summary>
        public event EventHandler<string> OverwriteConfirmed;

        public SaveAsNewJobDialog()
        {
            InitializeComponent();
        }

        public void Open(string suggestedName, string format, string machineLine)
        {
            SetupFormatText.Text = format;
            SetupMachineLineText.Text = machineLine;
            JobNameTextBox.Text = suggestedName;
            ShowInputState();
            Visibility = Visibility.Visible;
        }

        /// <summary>Switches the dialog to the "Job Name Already Exists!" warning
        /// state, keeping the Current Setup box and typed name as they were.</summary>
        public void ShowConflict()
        {
            HeaderIconBg.Background = (Brush)FindResource("WarningBgBrush");
            HeaderIconText.Text = "\uE7BA";
            HeaderIconText.Foreground = (Brush)FindResource("WarningBrush");
            TitleText.Text = "Job Name Already Exists!";
            TitleText.Foreground = (Brush)FindResource("WarningBrush");
            SubtitleText.Text = "A job with the entered name already exists. " +
                "Do you want to replace the existing job with the current setup?";

            JobNameInputPanel.Visibility = Visibility.Collapsed;
            ConflictBanner.Visibility = Visibility.Visible;
            NewNameButton.Visibility = Visibility.Visible;
        }

        private void ShowInputState()
        {
            HeaderIconBg.Background = (Brush)FindResource("StatusIdleBgBrush");
            HeaderIconText.Text = "\uE78C";
            HeaderIconText.Foreground = (Brush)FindResource("JobsAccentBrush");
            TitleText.Text = "Save As New Job";
            TitleText.Foreground = (Brush)FindResource("TextPrimaryBrush");
            SubtitleText.Text = "Save the current machine setup as a new job.";

            JobNameInputPanel.Visibility = Visibility.Visible;
            ConflictBanner.Visibility = Visibility.Collapsed;
            NewNameButton.Visibility = Visibility.Collapsed;
        }

        private void OnSaveJobClick(object sender, RoutedEventArgs e)
        {
            string name = JobNameTextBox.Text?.Trim() ?? string.Empty;
            if (name.Length == 0)
            {
                return;
            }

            bool isConflictState = ConflictBanner.Visibility == Visibility.Visible;
            Visibility = Visibility.Collapsed;

            if (isConflictState)
            {
                OverwriteConfirmed?.Invoke(this, name);
            }
            else
            {
                SaveRequested?.Invoke(this, name);
            }
        }

        private void OnNewNameClick(object sender, RoutedEventArgs e)
        {
            ShowInputState();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }

        private void OnScrimMouseDown(object sender, MouseButtonEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }
    }
}
