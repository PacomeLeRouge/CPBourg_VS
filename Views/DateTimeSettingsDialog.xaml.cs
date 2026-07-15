using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CPBourg.NextGenGui.Views
{
    /// <summary>Edits the prototype application's operator-facing date and time.</summary>
    public partial class DateTimeSettingsDialog : UserControl
    {
        public event EventHandler<DateTime> Confirmed;

        public DateTimeSettingsDialog()
        {
            InitializeComponent();
            HourComboBox.ItemsSource = Enumerable.Range(0, 24).Select(value => value.ToString("00"));
            MinuteComboBox.ItemsSource = Enumerable.Range(0, 60).Select(value => value.ToString("00"));
        }

        public void Open(DateTime currentValue)
        {
            DatePickerControl.SelectedDate = currentValue.Date;
            HourComboBox.SelectedIndex = currentValue.Hour;
            MinuteComboBox.SelectedIndex = currentValue.Minute;
            ValidationText.Visibility = Visibility.Collapsed;
            Visibility = Visibility.Visible;
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            if (!DatePickerControl.SelectedDate.HasValue || HourComboBox.SelectedIndex < 0 ||
                MinuteComboBox.SelectedIndex < 0)
            {
                ValidationText.Visibility = Visibility.Visible;
                return;
            }

            DateTime date = DatePickerControl.SelectedDate.Value.Date;
            DateTime result = date.AddHours(HourComboBox.SelectedIndex)
                                  .AddMinutes(MinuteComboBox.SelectedIndex);
            Visibility = Visibility.Collapsed;
            Confirmed?.Invoke(this, result);
        }

        private void OnCancelClick(object sender, RoutedEventArgs e) => Visibility = Visibility.Collapsed;
        private void OnScrimMouseDown(object sender, MouseButtonEventArgs e) => Visibility = Visibility.Collapsed;
    }
}
