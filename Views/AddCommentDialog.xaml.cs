using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>Add/update the comment on a job - see AddCommentDialog.xaml.</summary>
    public partial class AddCommentDialog : UserControl
    {
        /// <summary>Raised when Save Comment is clicked, with the new comment text.</summary>
        public event EventHandler<string> Saved;

        public AddCommentDialog()
        {
            InitializeComponent();
        }

        public void Open(string jobName, string existingComment)
        {
            Visibility = Visibility.Visible;
            LocalizationManager.Apply(this);
            JobNameText.Text = jobName;
            CommentTextBox.Text = existingComment == "-" ? string.Empty : existingComment;
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            Saved?.Invoke(this, CommentTextBox.Text);
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
