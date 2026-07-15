using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>
    /// Touchscreen keypad for decimal machine parameters. It accepts invariant
    /// decimal input and optionally allows a negative sign for directional
    /// offsets.
    /// </summary>
    public partial class DecimalInputDialog : UserControl
    {
        private const double MaximumAbsoluteValue = 999999.999;

        private string _text = "0.0";
        private bool _replaceOnNextInput;
        private bool _allowNegative;

        public event EventHandler<double> ValueConfirmed;

        public DecimalInputDialog()
        {
            InitializeComponent();
        }

        public void Open(string title, string fieldLabel, string description,
            double initialValue, bool allowNegative)
        {
            TitleText.Text = title;
            FieldLabelText.Text = fieldLabel + ":";
            DescriptionText.Text = description;
            _allowNegative = allowNegative;
            SignButton.IsEnabled = allowNegative;
            HintText.Text = allowNegative
                ? "Use +/- for direction and . for fractional millimeters."
                : "Use the decimal key for fractional millimeters.";
            _text = initialValue.ToString("0.###", CultureInfo.InvariantCulture);
            _replaceOnNextInput = true;
            ValidationText.Visibility = Visibility.Collapsed;
            RefreshValue();
            Visibility = Visibility.Visible;
        }

        private void OnDigitClick(object sender, RoutedEventArgs e)
        {
            string digit = (sender as FrameworkElement)?.Tag as string ?? string.Empty;
            if (_replaceOnNextInput)
            {
                _text = digit;
            }
            else if (_text == "0")
            {
                _text = digit;
            }
            else if (_text == "-0")
            {
                _text = "-" + digit;
            }
            else
            {
                _text += digit;
            }

            _replaceOnNextInput = false;
            ValidateAndRefresh();
        }

        private void OnDecimalClick(object sender, RoutedEventArgs e)
        {
            // Unlike the first digit (which replaces the pre-filled value),
            // the decimal key extends that value. This lets an operator turn
            // "210" into "210." instead of unexpectedly clearing it to
            // "0.". If a decimal point already exists, keep it and let the
            // next digit append to the existing fractional part.
            if (!_text.Contains("."))
            {
                _text += ".";
            }

            _replaceOnNextInput = false;
            ValidateAndRefresh();
        }

        private void OnSignClick(object sender, RoutedEventArgs e)
        {
            if (!_allowNegative)
            {
                return;
            }

            _text = _text.StartsWith("-", StringComparison.Ordinal)
                ? _text.Substring(1)
                : "-" + _text;
            _replaceOnNextInput = false;
            ValidateAndRefresh();
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            _text = "0";
            _replaceOnNextInput = true;
            ValidationText.Visibility = Visibility.Collapsed;
            RefreshValue();
        }

        private void OnBackspaceClick(object sender, RoutedEventArgs e)
        {
            _replaceOnNextInput = false;
            if (_text.Length <= 1 || (_text.Length == 2 && _text.StartsWith("-", StringComparison.Ordinal)))
            {
                _text = "0";
            }
            else
            {
                _text = _text.Substring(0, _text.Length - 1);
            }

            ValidateAndRefresh();
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            double value;
            if (!TryGetValue(out value))
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

        private void ValidateAndRefresh()
        {
            double value;
            ValidationText.Visibility = TryGetValue(out value)
                ? Visibility.Collapsed
                : Visibility.Visible;
            RefreshValue();
        }

        private bool TryGetValue(out double value)
        {
            bool parsed = double.TryParse(_text, NumberStyles.AllowLeadingSign |
                NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value);
            return parsed && Math.Abs(value) <= MaximumAbsoluteValue;
        }

        private void RefreshValue()
        {
            ValueText.Text = _text;
        }

        private void Close()
        {
            Visibility = Visibility.Collapsed;
        }
    }
}
