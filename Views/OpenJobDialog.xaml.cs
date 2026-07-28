using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>Open a saved job and load its settings - see OpenJobDialog.xaml.</summary>
    public partial class OpenJobDialog : UserControl
    {
        /// <summary>Raised when Open Job is clicked, with whether "load saved RUN adjustments" was checked.</summary>
        public event EventHandler<bool> Opened;

        public OpenJobDialog()
        {
            InitializeComponent();
        }

        public void Open(string jobName, string format)
        {
            Visibility = Visibility.Visible;
            LocalizationManager.Apply(this);
            JobNameText.Text = jobName;
            JobFormatText.Text = format;
            LoadRunAdjustmentsCheckBox.IsChecked = false;
        }

        private void OnOpenClick(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            Opened?.Invoke(this, LoadRunAdjustmentsCheckBox.IsChecked == true);
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
