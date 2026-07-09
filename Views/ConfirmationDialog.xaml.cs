using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>Shared success confirmation - see ConfirmationDialog.xaml.</summary>
    public partial class ConfirmationDialog : UserControl
    {
        /// <summary>Raised when OK is clicked (or the scrim is clicked to dismiss).</summary>
        public event EventHandler Closed;

        public ConfirmationDialog()
        {
            InitializeComponent();
        }

        public void Open(string title, string message)
        {
            TitleText.Text = title;
            MessageText.Text = message;
            Visibility = Visibility.Visible;
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            Closed?.Invoke(this, EventArgs.Empty);
        }

        private void OnScrimMouseDown(object sender, MouseButtonEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            Closed?.Invoke(this, EventArgs.Empty);
        }
    }
}
