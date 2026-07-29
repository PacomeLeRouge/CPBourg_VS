using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>Remove a saved job (destructive, no undo) - see RemoveJobDialog.xaml.</summary>
    public partial class RemoveJobDialog : UserControl
    {
        /// <summary>Raised when Remove Job is clicked, with the job name to remove.</summary>
        public event EventHandler<string> Removed;

        public RemoveJobDialog()
        {
            InitializeComponent();
        }

        public void Open(string jobName, string format, string machineLine)
        {
            Visibility = Visibility.Visible;
            LocalizationManager.Apply(this);
            JobNameText.Text = jobName;
            SetupFormatText.Text = format;
            SetupMachineLineText.Text = machineLine;
        }

        private void OnRemoveClick(object sender, RoutedEventArgs e)
        {
            string jobName = JobNameText.Text;
            Visibility = Visibility.Collapsed;
            Removed?.Invoke(this, jobName);
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
