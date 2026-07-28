using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>
    /// Reusable non-negative integer keypad for touchscreen number entry.
    /// The keypad caps values at nine digits to keep all counter operations
    /// safely inside a 32-bit integer.
    /// </summary>
    public partial class NumericInputDialog : UserControl
    {
        public const int MaximumValue = 999999999;

        private string _digits = "0";
        private bool _replaceOnNextDigit;
        private int _minimumValue;
        private int _maximumValue = MaximumValue;

        public event EventHandler<int> ValueConfirmed;

        public NumericInputDialog()
        {
            InitializeComponent();
        }

        public void Open(string title, string fieldLabel, string description,
            int initialValue, bool zeroMeansUnlimited)
        {
            Open(title, fieldLabel, description, initialValue, 0, MaximumValue,
                zeroMeansUnlimited ? "Enter 0 for unlimited production (\u221e)." : string.Empty);
        }

        public void Open(string title, string fieldLabel, string description,
            int initialValue, int minimumValue, int maximumValue, string hint)
        {
            Visibility = Visibility.Visible;
            LocalizationManager.Apply(this);
            _minimumValue = Math.Max(0, minimumValue);
            _maximumValue = Math.Min(MaximumValue, Math.Max(_minimumValue, maximumValue));
            TitleText.Text = title;
            FieldLabelText.Text = fieldLabel + ":";
            DescriptionText.Text = description;
            HintText.Text = LocalizationManager.Translate(hint ?? string.Empty);
            ValidationText.Text = string.Format(CultureInfo.CurrentCulture,
                LocalizationManager.Translate("Enter a value from {0} to {1}."),
                _minimumValue.ToString("N0", CultureInfo.CurrentCulture),
                _maximumValue.ToString("N0", CultureInfo.CurrentCulture));
            int boundedInitialValue = Math.Max(_minimumValue, Math.Min(_maximumValue, initialValue));
            _digits = boundedInitialValue.ToString(CultureInfo.InvariantCulture);
            _replaceOnNextDigit = true;
            ValidationText.Visibility = Visibility.Collapsed;
            RefreshValue();
        }

        private void OnDigitClick(object sender, RoutedEventArgs e)
        {
            string digit = (sender as FrameworkElement)?.Tag as string ?? string.Empty;
            string candidate = _replaceOnNextDigit || _digits == "0" ? digit : _digits + digit;
            _replaceOnNextDigit = false;

            int value;
            if (candidate.Length > 9 || !int.TryParse(candidate, NumberStyles.None,
                    CultureInfo.InvariantCulture, out value) || value > MaximumValue)
            {
                ValidationText.Visibility = Visibility.Visible;
                return;
            }

            _digits = candidate.Length == 0 ? "0" : candidate;
            ValidationText.Visibility = Visibility.Collapsed;
            RefreshValue();
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            _digits = "0";
            _replaceOnNextDigit = true;
            ValidationText.Visibility = Visibility.Collapsed;
            RefreshValue();
        }

        private void OnBackspaceClick(object sender, RoutedEventArgs e)
        {
            _replaceOnNextDigit = false;
            _digits = _digits.Length > 1 ? _digits.Substring(0, _digits.Length - 1) : "0";
            ValidationText.Visibility = Visibility.Collapsed;
            RefreshValue();
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            int value = int.Parse(_digits, CultureInfo.InvariantCulture);
            if (value < _minimumValue || value > _maximumValue)
            {
                ValidationText.Visibility = Visibility.Visible;
                return;
            }

            Close();
            ValueConfirmed?.Invoke(this, value);
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
            Visibility = Visibility.Collapsed;
        }

        private void RefreshValue()
        {
            int value = int.Parse(_digits, CultureInfo.InvariantCulture);
            ValueText.Text = value.ToString("N0", CultureInfo.CurrentCulture);
        }
    }
}
