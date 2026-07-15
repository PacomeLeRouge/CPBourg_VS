using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CPBourg.NextGenGui.Models;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>
    /// Save the current machine setup as a new job - see
    /// SaveAsNewJobDialog.xaml for the two-state (input / conflict) design.
    /// </summary>
    public partial class SaveAsNewJobDialog : UserControl
    {
        /// <summary>Raised when Save Job is clicked in the normal input state,
        /// with the entered job definition. The caller should check whether that
        /// name already exists: if not, save and close; if it does, call
        /// <see cref="ShowConflict"/> instead of closing.</summary>
        public event EventHandler<SaveJobRequest> SaveRequested;

        /// <summary>Raised when Save Job is clicked in the conflict (overwrite)
        /// state, with the job definition to overwrite.</summary>
        public event EventHandler<SaveJobRequest> OverwriteConfirmed;

        private int _pages = 1;
        private double _widthMm = 210;
        private double _lengthMm = 297;
        private DimensionField _pendingDimensionField;
        private bool _updatingFormat;

        private enum DimensionField
        {
            Width,
            Length,
        }

        public SaveAsNewJobDialog()
        {
            InitializeComponent();
            PagesInputDialog.ValueConfirmed += OnPagesConfirmed;
            DimensionInputDialog.ValueConfirmed += OnDimensionConfirmed;

            foreach (var preset in BookFormatCatalog.Presets)
            {
                FormatPresetComboBox.Items.Add(preset.Name);
            }
            FormatPresetComboBox.Items.Add("Custom");
        }

        public void Open(string suggestedName, int pages, string format,
            double widthMm, double lengthMm, string machineLine)
        {
            SetupMachineLineText.Text = machineLine;
            JobNameTextBox.Text = suggestedName;
            _pages = Math.Max(1, pages);
            _widthMm = widthMm;
            _lengthMm = lengthMm;

            _updatingFormat = true;
            FormatPresetComboBox.SelectedItem = BookFormatCatalog.Find(format) == null
                ? "Custom"
                : format;
            _updatingFormat = false;
            RefreshJobDefinition();
            ShowInputState();
            Visibility = Visibility.Visible;
        }

        public void Close()
        {
            Visibility = Visibility.Collapsed;
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
            ValidationText.Visibility = Visibility.Collapsed;
        }

        private void OnFormatPresetChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_updatingFormat)
            {
                return;
            }

            var preset = BookFormatCatalog.Find(FormatPresetComboBox.SelectedItem as string);
            if (preset != null)
            {
                _widthMm = preset.WidthMm;
                _lengthMm = preset.LengthMm;
            }

            RefreshJobDefinition();
        }

        private void OnPagesInputPressed(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            PagesInputDialog.Open(
                "Set Number of Pages", "Pages",
                "Enter the total number of pages in the new job.",
                _pages, 1, 2000, "Enter a whole number from 1 to 2,000.");
        }

        private void OnPagesConfirmed(object sender, int pages)
        {
            _pages = pages;
            RefreshJobDefinition();
        }

        private void OnDimensionInputPressed(object sender, MouseButtonEventArgs e)
        {
            var textBox = sender as TextBox;
            _pendingDimensionField = (textBox?.Tag as string) == "Length"
                ? DimensionField.Length
                : DimensionField.Width;
            bool isWidth = _pendingDimensionField == DimensionField.Width;
            e.Handled = true;
            DimensionInputDialog.Open(
                isWidth ? "Set Book Width" : "Set Book Length",
                isWidth ? "Width (mm)" : "Length (mm)",
                "Enter the physical book format dimension in millimetres.",
                isWidth ? _widthMm : _lengthMm,
                false);
        }

        private void OnDimensionConfirmed(object sender, double value)
        {
            if (_pendingDimensionField == DimensionField.Width)
            {
                _widthMm = value;
            }
            else
            {
                _lengthMm = value;
            }

            RefreshJobDefinition();
        }

        private void RefreshJobDefinition()
        {
            PagesValueBox.Text = _pages.ToString();
            WidthValueBox.Text = _widthMm.ToString("0.0#");
            LengthValueBox.Text = _lengthMm.ToString("0.0#");
            string resolvedFormat = BookFormatCatalog.ResolveName(_widthMm, _lengthMm);
            SetupFormatText.Text = resolvedFormat;
            FormatClassificationText.Text = resolvedFormat == "Custom"
                ? "Dimensions do not match a standard preset; this job will be displayed as Custom."
                : "Dimensions match the " + resolvedFormat + " preset.";
        }

        private void OnSaveJobClick(object sender, RoutedEventArgs e)
        {
            string name = JobNameTextBox.Text?.Trim() ?? string.Empty;
            if (name.Length == 0 || _pages < 1 || _widthMm <= 0 || _lengthMm <= 0)
            {
                ValidationText.Text = name.Length == 0
                    ? "Enter a unique job name."
                    : "Pages and both dimensions must be greater than zero.";
                ValidationText.Visibility = Visibility.Visible;
                return;
            }

            var request = new SaveJobRequest(name, _pages, _widthMm, _lengthMm);
            bool isConflictState = ConflictBanner.Visibility == Visibility.Visible;

            if (isConflictState)
            {
                Close();
                OverwriteConfirmed?.Invoke(this, request);
            }
            else
            {
                SaveRequested?.Invoke(this, request);
            }
        }

        private void OnNewNameClick(object sender, RoutedEventArgs e)
        {
            ShowInputState();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnScrimMouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
