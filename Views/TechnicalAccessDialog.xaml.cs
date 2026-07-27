using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>
    /// Touch-friendly technician code prompt. It deliberately follows the
    /// Add Module confirmation keypad; live deployments can replace the
    /// non-empty check with the machine's credential validator.
    /// </summary>
    public partial class TechnicalAccessDialog : UserControl
    {
        private string _code = string.Empty;

        public event EventHandler<string> AccessGranted;

        public TechnicalAccessDialog()
        {
            InitializeComponent();
        }

        public void Open()
        {
            Open(
                "Technical Access",
                "Enter your technician code to unlock protected actions.",
                "Unlock");
        }

        public void Open(string title, string description, string submitLabel)
        {
            _code = string.Empty;
            DialogTitleText.Text = title;
            DialogDescriptionText.Text = description;
            SubmitButtonText.Text = submitLabel;
            ValidationText.Visibility = Visibility.Collapsed;
            RefreshMaskedCode();
            Visibility = Visibility.Visible;
        }

        private void OnDigitClick(object sender, RoutedEventArgs e)
        {
            if (_code.Length >= 12)
            {
                return;
            }

            _code += (sender as FrameworkElement)?.Tag as string ?? string.Empty;
            ValidationText.Visibility = Visibility.Collapsed;
            RefreshMaskedCode();
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            _code = string.Empty;
            RefreshMaskedCode();
        }

        private void OnBackspaceClick(object sender, RoutedEventArgs e)
        {
            if (_code.Length > 0)
            {
                _code = _code.Substring(0, _code.Length - 1);
                RefreshMaskedCode();
            }
        }

        private void OnUnlockClick(object sender, RoutedEventArgs e)
        {
            if (_code.Length == 0)
            {
                ValidationText.Visibility = Visibility.Visible;
                return;
            }

            string submittedCode = _code;
            Close();
            AccessGranted?.Invoke(this, submittedCode);
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnScrimMouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void Close()
        {
            _code = string.Empty;
            RefreshMaskedCode();
            Visibility = Visibility.Collapsed;
        }

        private void RefreshMaskedCode()
        {
            MaskedCodeText.Text = new string('\u25CF', _code.Length);
        }
    }
}
