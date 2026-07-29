using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>Captures keyboard-wedge barcode scanner input and submits it for lookup.</summary>
    public partial class BarcodeScanDialog : UserControl
    {
        public event EventHandler<string> ScanRequested;

        public BarcodeScanDialog()
        {
            InitializeComponent();
        }

        public void Open()
        {
            Visibility = Visibility.Visible;
            LocalizationManager.Apply(this);
            BarcodeTextBox.Text = string.Empty;
            ValidationText.Visibility = Visibility.Collapsed;
            Dispatcher.BeginInvoke(new Action(() => BarcodeTextBox.Focus()), DispatcherPriority.Input);
        }

        public void Close()
        {
            Visibility = Visibility.Collapsed;
        }

        public void ShowError(string message)
        {
            ValidationText.Text = message;
            ValidationText.Visibility = Visibility.Visible;
            BarcodeTextBox.SelectAll();
            BarcodeTextBox.Focus();
        }

        private void Submit()
        {
            string barcode = BarcodeTextBox.Text.Trim();
            if (string.IsNullOrEmpty(barcode))
            {
                ShowError(LocalizationManager.Translate("Scan or enter a barcode ID first."));
                return;
            }

            ScanRequested?.Invoke(this, barcode);
        }

        private void OnFindJobClick(object sender, RoutedEventArgs e) => Submit();

        private void OnBarcodeKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                e.Handled = true;
                Submit();
            }
        }

        private void OnBarcodeTextChanged(object sender, TextChangedEventArgs e)
        {
            ValidationText.Visibility = Visibility.Collapsed;
        }

        private void OnCancelClick(object sender, RoutedEventArgs e) => Close();
        private void OnScrimMouseDown(object sender, MouseButtonEventArgs e) => Close();
    }
}
